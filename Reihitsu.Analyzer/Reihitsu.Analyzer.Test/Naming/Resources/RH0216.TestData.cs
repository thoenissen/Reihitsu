using System;

namespace Reihitsu.Analyzer.Test.Naming.Resources
{
    /// <summary>
    /// Test class
    /// </summary>
    public class TestClass
    {
        /// <summary>
        /// Test field
        /// </summary>
        private const int {|#0:testField|} = 42;
    }
}