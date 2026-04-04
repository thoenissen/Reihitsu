using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Reihitsu.Analyzer.Test.Clarity
{
    public class Test
    {
        private bool _field;
        public bool Property { get; set; }

        public bool GetBool()
        {
            return {|#0:!|}false;
        }

        public bool GetField()
        {
            return {|#1:!|}_field;
        }

        public bool GetProperty()
        {
            return {|#2:!|}Property;
        }

        public bool GetMethod()
        {
            return {|#3:!|}GetBool();
        }
    }
}