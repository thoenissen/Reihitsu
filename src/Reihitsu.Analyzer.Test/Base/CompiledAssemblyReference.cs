using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Compiles source into an assembly image and references it as metadata, so a test project sees the declared
/// members as members of a referenced assembly. A project reference would make the members source, which the
/// renamer renames together with the code under test
/// </summary>
internal static class CompiledAssemblyReference
{
    #region Methods

    /// <summary>
    /// Compiles the provided source against the reference assemblies the analyzer tests use and returns a metadata
    /// reference to the emitted image
    /// </summary>
    /// <param name="assemblyName">Name of the compiled assembly</param>
    /// <param name="source">Source of the compiled assembly</param>
    /// <returns>A metadata reference to the emitted image</returns>
    internal static async Task<MetadataReference> CreateAsync(string assemblyName, string source)
    {
        var references = await ReferenceAssemblies.Net.Net90.ResolveAsync(LanguageNames.CSharp, CancellationToken.None).ConfigureAwait(false);
        var compilation = CSharpCompilation.Create(assemblyName,
                                                   [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
                                                   references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var stream = new MemoryStream())
        {
            var result = compilation.Emit(stream);

            if (result.Success == false)
            {
                throw new InvalidOperationException("The referenced assembly does not compile: " + string.Join(Environment.NewLine, result.Diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
            }

            return MetadataReference.CreateFromImage(stream.ToArray());
        }
    }

    #endregion // Methods
}