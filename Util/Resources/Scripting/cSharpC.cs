using GameEngine.Util.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Reflection.Metadata;

namespace GameEngine.Util.Resources;

public class CSharpCompiler : Resource, IScriptCompiler
{

    public void Compile(string src, string sourcePath="")
    {

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(src, null, sourcePath);

        string assemblyName = "DynamicAss.dll";
        List<MetadataReference> assembliesRefs = [];

        assembliesRefs.AddRange(Basic.Reference.Assemblies.Net80.References.All);
        assembliesRefs.Add(MetadataReference.CreateFromFile(typeof(Program).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [ syntaxTree ],
            references: assembliesRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

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

            Type scriptType = assembly.GetType("Script")!;
            object scriptInstance = Activator.CreateInstance(scriptType)!;

            MethodInfo executeMethod = scriptType.GetMethod("Execute")!;
            executeMethod.Invoke(scriptInstance, null);
        }

    }

}