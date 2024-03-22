using GameEngine.Util.Interfaces;
using GameEngine.Util.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using static GameEngine.Util.Resources.Script;

namespace GameEngine.Util.Resources;

public class CSharpCompiler : Resource, IScriptCompiler
{

    public static Assembly? Compile(Script script)
    {

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script.Code, null, script.path.GlobalPath);
        syntaxTree = PreprocessSyntaxTree(syntaxTree, out _);
        
        return CompileAndGetAsm([syntaxTree], out _);

    }
    public static Assembly? CompileMultiple(Script[] scripts, out Dictionary<Script, string[]> fileTypeMap)
    {

        List<(Script, SyntaxTree)> scriptsAndTrees = [];

        foreach (var script in scripts)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script.Code, null, script.path.GlobalPath);
            syntaxTree = PreprocessSyntaxTree(syntaxTree, out _);
            scriptsAndTrees.Add( (script, syntaxTree) );
        }

        var asm = CompileAndGetAsm([.. scriptsAndTrees.Select(e => e.Item2)], out var compilation);

        Dictionary<Script, string[]> typesPerFile = [];

        foreach (var (script, syntaxTree) in scriptsAndTrees)
        {
            var sm = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            var namedTypes = root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .Select(ts => sm.GetDeclaredSymbol(ts)).OfType<INamedTypeSymbol>()
            .ToList();

            typesPerFile.Add(script, [.. namedTypes.Select(e => e.Name)]);
        }

        fileTypeMap = typesPerFile;

        return asm;
    }

    public static TextField.ColorSpan[] Highlight(string src)
    {
        
        List<TextField.ColorSpan> spans = [];

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(src);
        var root = (CompilationUnitSyntax) syntaxTree.GetRoot();

        // comments and trivias //
        var comments = root.DescendantTrivia()
        .Where(e => e.IsKind(SyntaxKind.SingleLineCommentTrivia) || e.IsKind(SyntaxKind.MultiLineCommentTrivia));
        foreach (var comment in comments)
        {
            spans.Add(new(comment.FullSpan.Start, comment.FullSpan.End, new(255, 255, 255, 0.5f)));
        }

        // generic tokens //
        var tokens = root.DescendantTokens();
        foreach (var token in tokens)
        {
            //Console.WriteLine($"{token} ({token.Kind()}");

            if (token.Kind().ToString().EndsWith("Keyword"))
                spans.Add(new(token.FullSpan.Start, token.FullSpan.End, new(255, 0, 0)));

            else if (token.IsKind(SyntaxKind.IdentifierToken))
            {
                if(token.GetNextToken().IsKind(SyntaxKind.OpenParenToken))
                    spans.Add(new(token.FullSpan.Start, token.FullSpan.End, new(0, 255, 0)));
            }

            else if (
                token.IsKind(SyntaxKind.StringLiteralToken) ||

                token.IsKind(SyntaxKind.InterpolatedStringTextToken) ||
                token.IsKind(SyntaxKind.InterpolatedStringStartToken) ||
                token.IsKind(SyntaxKind.InterpolatedStringEndToken)
                )
                spans.Add(new(token.FullSpan.Start, token.FullSpan.End, new(255, 255, 0)));
            
        }

        // compile //
        syntaxTree = PreprocessSyntaxTree(syntaxTree, out var jumps);
        var compilation = Compile([syntaxTree]);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        root = (CompilationUnitSyntax) syntaxTree.GetRoot();

        /*
        var identfiers = root.DescendantNodes();
        foreach (var identfier in identfiers)
        {
            var mSymbol = semanticModel.GetDeclaredSymbol(identfier);
            if (mSymbol != null)
                Console.WriteLine($"\"{identfier}\"\n({identfier.Kind()}, {mSymbol.Kind})\n");
            
            {
                int jumpLen = 0;
                var toJump = jumps.Where(j => j.position < identfier.FullSpan.Start);
                foreach (var i in toJump) jumpLen += i.length;

                spans.Add(new(
                    identfier.FullSpan.Start - jumpLen,
                    identfier.FullSpan.End - jumpLen,
                    new(0, 255, 0)
                ));
            }
        }
        */

        return [.. spans];
    }


    private static SyntaxTree PreprocessSyntaxTree(SyntaxTree syntaxTree, out CodeJump[] jumps)
    {
        List<CodeJump> codeJumps = [];

        SyntaxTree tree = syntaxTree;
        var root = (CompilationUnitSyntax) tree.GetRoot();

        #region add omitable using namespaces

        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
        int lastUsingIndex = usingDirectives.Length > 0 ? usingDirectives[^1].FullSpan.End : 0;
        int newUsingsLength = 0;

        var autoUsing = new KeyValuePair<string, string>[] {
            new("", "System"),
            new("", "GameEngine.Core"),
            new("", "GameEngine.Util.Nodes"),
            new("", "GameEngine.Util.Values"),
            new("", "GameEngine.Util.Attributes"),
            new("Console", "GameEngine.Debugging.Debug")
        };

        autoUsing = autoUsing.Where(e =>
            !usingDirectives.Select(e => e.Name!.ToString()).Contains(e.Value)
        ).ToArray();


        List<UsingDirectiveSyntax> usingTokens = [];
        for (int i = 0; i < autoUsing.Length; i++)
        {
            var nSpace = autoUsing[i];

            UsingDirectiveSyntax usingDir;

            if (nSpace.Key == string.Empty)
                usingDir = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nSpace.Value)
                .WithLeadingTrivia(SyntaxFactory.Space))
                .WithTrailingTrivia(SyntaxFactory.Space);
            else
                usingDir = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nSpace.Value)
                .WithLeadingTrivia(SyntaxFactory.Space))
                .WithAlias(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(nSpace.Key)
                .WithLeadingTrivia(SyntaxFactory.Space)
                .WithTrailingTrivia(SyntaxFactory.Space)));

            if (i != 0)
                usingDir = usingDir.WithLeadingTrivia(SyntaxFactory.LineFeed);

            if (i == autoUsing.Length - 1)
                usingDir = usingDir.WithTrailingTrivia(new SyntaxTrivia[] { SyntaxFactory.LineFeed, SyntaxFactory.LineFeed });
            

            usingTokens.Add(usingDir);
            newUsingsLength += usingDir.FullSpan.End - usingDir.FullSpan.Start;
        }

        root = root.AddUsings([.. usingTokens]);
        codeJumps.Add(new(lastUsingIndex, newUsingsLength));

        #endregion

        tree = tree.WithRootAndOptions(root, tree.Options);

        jumps = [.. codeJumps];

        return tree;

    }

    private static Assembly? CompileAndGetAsm(SyntaxTree[] trees, out CSharpCompilation comp)
    {

        var compilation = Compile(trees);

        comp = compilation;

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success)
        {
            Console.WriteLine("Compilation error:");
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()} (l. {diagnostic.Location.GetLineSpan().StartLinePosition.Line})");
            }
        }
        else
        {
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            return assembly;
        }

        return null;
    }
    private static CSharpCompilation Compile(SyntaxTree[] trees)
    {
        string assemblyName = "DynamicAss.dll";
        List<MetadataReference> assembliesRefs = [];

        assembliesRefs.AddRange(Basic.Reference.Assemblies.Net80.References.All);
        assembliesRefs.Add(MetadataReference.CreateFromFile(typeof(Program).Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: trees,
            references: assembliesRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }


    private enum MySyntaxKind
    {
        ExtendsKeyword = 9078 + 1,
    }

}

