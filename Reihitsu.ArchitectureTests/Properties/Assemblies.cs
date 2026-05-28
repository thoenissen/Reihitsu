using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using Assembly = System.Reflection.Assembly;

namespace Reihitsu.ArchitectureTests.Properties;

/// <summary>
/// Assemblies
/// </summary>
internal class Assemblies
{
    #region Properties

    /// <summary>
    /// All assemblies
    /// </summary>
    public static Architecture All { get; private set; } = new ArchLoader().LoadAssemblies(Assembly.Load("Reihitsu.Analyzer"),
                                                                                           Assembly.Load("Reihitsu.Analyzer.CodeFixes"),
                                                                                           Assembly.Load("Reihitsu.Core"),
                                                                                           Assembly.Load("Reihitsu.Cli"),
                                                                                           Assembly.Load("Reihitsu.Formatter"),
                                                                                           Assembly.Load("Reihitsu.Analyzer.Test"),
                                                                                           Assembly.Load("Reihitsu.ArchitectureTests"),
                                                                                           Assembly.Load("Reihitsu.Cli.Test"),
                                                                                           Assembly.Load("Reihitsu.Core.Test"),
                                                                                           Assembly.Load("Reihitsu.Formatter.Test"))
                                                                           .Build();

    #endregion // Properties
}