using System;

namespace Reihitsu.Analyzer.Test.Naming.Resources
{
    /// <summary>
    /// Test class
    /// </summary>
    public class TestClass
    {
        /// <summary>
        /// Test method
        /// </summary>
        public void TestMethod()
        {
            int {|#0:LocalVariable|} = 42;
        }
    }
}