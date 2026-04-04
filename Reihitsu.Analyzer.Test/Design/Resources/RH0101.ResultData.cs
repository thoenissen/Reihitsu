using System;
using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Analyzer.Test.Design.Resources
{
    internal class RH0101
    {
        private bool _field;

        private bool _privateAutoProperty;

        protected bool ProtectedAutoProperty { get; set; }

        internal bool InternalAutoProperty { get; set; }

        public bool PublicAutoProperty { get; set; }

        private bool PrivateProperty { get => _field; set => _field = value; }

        protected bool ProtectedProperty { get => _field; set => _field = value; }

        internal bool InternalProperty { get => _field; set => _field = value; }

        public bool PublicProperty { get => _field; set => _field = value; }
    }
}
