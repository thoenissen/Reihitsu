using System;
using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Analyzer.Test.Design.Resources
{
    internal class RH0301
    {
        public RH0301()
        {
            var tmp1 = {|#0:new RH0301
            {
                Test1 = "123"
            }|};

            var tmp2 = {|#1:new RH0301
                       {
                Test1 = "123"
                       }|};

            var tmp3 = {|#2:new RH0301
                                         {
                                             Test1 = "123"
                                         }|};

            var tmp4 = {|#3:new RH0301
                {
                Test1 = "123",
                Test2 = "123",
                Test3 = "123",
                Test4 = "123"
                }|};
        }

        public string Test1 { get; set; }
        public string Test2 { get; set; }
        public string Test3 { get; set; }
        public string Test4 { get; set; }
    }
}