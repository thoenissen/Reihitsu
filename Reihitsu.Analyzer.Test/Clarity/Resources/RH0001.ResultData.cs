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
            return false == false;
        }

        public bool GetField()
        {
            return _field == false;
        }

        public bool GetProperty()
        {
            return Property == false;
        }

        public bool GetMethod()
        {
            return GetBool() == false;
        }
    }
}