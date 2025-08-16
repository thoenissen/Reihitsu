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
        public (int {|#0:firstElement|}, int SecondElement) TestMethod()
        {
            return default;
        }
    }
}