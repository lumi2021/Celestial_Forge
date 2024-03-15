using GameEngine.Util.Interfaces;
using GameEngine.Util.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

namespace GameEngine.Util.Resources;

public class CSharpCompiler : Resource, IScriptCompiler
{

    public static Assembly? Compile(Script script)
    {

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script.Code, null, script.path.GlobalPath);
        syntaxTree = PreprocessSyntaxTree(syntaxTree);
        
        return CompileAndGetAsm([syntaxTree], out _);

    }
    public static Assembly? CompileMultiple(Script[] scripts, out Dictionary<Script, string[]> fileTypeMap)
    {

        List<(Script, SyntaxTree)> scriptsAndTrees = [];

        foreach (var script in scripts)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script.Code, null, script.path.GlobalPath);
            syntaxTree = PreprocessSyntaxTree(syntaxTree);
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

        // generic tokens //
        var tokens = root.DescendantTokens();
        foreach (var token in tokens)
        {
            if (token.Kind().ToString().EndsWith("Keyword"))
                spans.Add(new(token.FullSpan.Start, token.FullSpan.End, new(255, 0, 0)));

            else if (token.IsKind(SyntaxKind.IdentifierToken))
            {
                if(token.GetNextToken().IsKind(SyntaxKind.OpenParenToken))
                    spans.Add(new(token.FullSpan.Start, token.FullSpan.End, new(0, 255, 0)));
            }

            else if (token.IsKind(SyntaxKind.StringLiteralToken))
                spans.Add(new(token.FullSpan.Start, token.FullSpan.End, new(255, 255, 0)));
            
        }

        // comments and trivias //
        //var comments = [];

        return [.. spans];

    }


    private static SyntaxTree PreprocessSyntaxTree(SyntaxTree syntaxTree)
    {

        SyntaxTree tree = syntaxTree;
        var root = (CompilationUnitSyntax) tree.GetRoot();

        #region add omitable using namespaces

        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

        var autoUsing = new KeyValuePair<string, string>[] {
            new("", "System"),
            new("", "GameEngine.Core"),
            new("", "GameEngine.Util.Nodes"),
            new("", "GameEngine.Util.Values"),
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
                .WithTrailingTrivia(SyntaxFactory.Space)));

            if (i == autoUsing.Length -1)
                usingDir = usingDir.WithTrailingTrivia(SyntaxFactory.LineFeed);

            usingTokens.Add(usingDir.WithLeadingTrivia(SyntaxFactory.LineFeed));
        }

        root = root.AddUsings([.. usingTokens]);

        #endregion

        tree = tree.WithRootAndOptions(root, tree.Options);

        //Console.WriteLine(root);

        return tree;

    }

    private static Assembly? CompileAndGetAsm(SyntaxTree[] trees, out Compilation comp)
    {

        string assemblyName = "DynamicAss.dll";
        List<MetadataReference> assembliesRefs = [];

        assembliesRefs.AddRange(Basic.Reference.Assemblies.Net80.References.All);
        assembliesRefs.Add(MetadataReference.CreateFromFile(typeof(Program).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: trees,
            references: assembliesRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

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

}