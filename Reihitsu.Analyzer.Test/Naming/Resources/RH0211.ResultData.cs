using System;

namespace Reihitsu.Analyzer.Test.Naming.Resources;

/// <summary>
/// Test class
/// </summary>
/// <param name="primaryParameterName">Primary parameter</param>
public class TestClass(int primaryParameterName)
{
    /// <summary>
    /// Test method
    /// </summary>
    /// <param name="methodParameterName">Method parameter</param>
    public void TestMethod(int methodParameterName)
    {
    }
}

/// <summary>
/// Test struct
/// </summary>
/// <param name="parameterName">Parameter</param>
public struct TestStruct(int parameterName);