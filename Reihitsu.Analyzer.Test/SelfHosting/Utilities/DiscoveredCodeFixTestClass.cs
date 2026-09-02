using System;

namespace Reihitsu.Analyzer.Test.SelfHosting.Utilities;

/// <summary>
/// Reflected metadata of a test class that verifies an analyzer together with its code fix
/// </summary>
/// <param name="TestClassType">Test class type</param>
/// <param name="CodeFixProviderType">Code-fix provider type verified by the test class</param>
/// <param name="SupportsFixAll">Value indicating whether the code-fix provider offers a Fix All provider</param>
/// <param name="CodeFixTestsBaseDefinition">Generic type definition of the code-fix test base the test class derives from, or <see langword="null"/> when the test class has not been migrated to one of them</param>
internal sealed record DiscoveredCodeFixTestClass(Type TestClassType,
                                                  Type CodeFixProviderType,
                                                  bool SupportsFixAll,
                                                  Type CodeFixTestsBaseDefinition);