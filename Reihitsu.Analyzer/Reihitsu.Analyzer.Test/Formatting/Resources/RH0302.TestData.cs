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
                Test = "123"
            }|};

            var tmp2 = {|#1:new RH0301
                       {
                Test = "123"
                       }|};

            var tmp3 = {|#2:new RH0301
                                         {
                                             Test = "123"
                                         }|};
        }

        public string Test { get; set; }
    }
}