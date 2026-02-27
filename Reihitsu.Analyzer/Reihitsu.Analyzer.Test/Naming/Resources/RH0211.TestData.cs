using System;

namespace Reihitsu.Analyzer.Test.Naming.Resources;

/// <summary>
/// Test class
/// </summary>
/// <param name="PrimaryParameterName">Primary parameter</param>
public class TestClass(int {|#0:PrimaryParameterName|})
{
    /// <summary>
    /// Test method
    /// </summary>
    /// <param name="MethodParameterName">Method parameter</param>
    public void TestMethod(int {|#1:MethodParameterName|})
    {
    }
}

/// <summary>
/// Test struct
/// </summary>
/// <param name="ParameterName">Parameter</param>
public struct TestStruct(int {|#2:ParameterName|});