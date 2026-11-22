using System;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Lets a test project compile C# union declarations. The analyzer tests compile against the .NET 9 reference
/// assemblies, which lack the runtime support types the compiler requires for a union (CS0518, CS0656). This class
/// declares those types in a referenced assembly, so a union compiles without any diagnostic, and the declarations stay
/// outside the test's own source, where no rule reports them
/// </summary>
internal static class UnionSupport
{
    #region Constants

    /// <summary>
    /// Source of the runtime support types the compiler requires for a union declaration
    /// </summary>
    private const string SupportSource = """
                                         namespace System.Runtime.CompilerServices
                                         {
                                             public interface IUnion
                                             {
                                                 object Value { get; }
                                             }

                                             [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
                                             public sealed class UnionAttribute : Attribute
                                             {
                                             }
                                         }
                                         """;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Compiled support assembly, shared by every test
    /// </summary>
    private static readonly Lazy<Task<MetadataReference>> _reference = new(() => CompiledAssemblyReference.CreateAsync("UnionSupport", SupportSource));

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Returns the support assembly as a metadata reference
    /// </summary>
    /// <returns>Metadata reference to the support assembly</returns>
    internal static Task<MetadataReference> GetReferenceAsync()
    {
        return _reference.Value;
    }

    /// <summary>
    /// Returns the support assembly as a metadata reference, for configuration that cannot await, such as a
    /// <see cref="FixAllScenario"/>
    /// </summary>
    /// <returns>Metadata reference to the support assembly</returns>
    internal static MetadataReference GetReference()
    {
        return _reference.Value.GetAwaiter().GetResult();
    }

    #endregion // Methods
}