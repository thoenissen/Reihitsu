using System;
using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Analyzer.Test.Design.Resources
{
    internal class RH0101
    {
        #region Fields
        private bool _field;
        {|#0:#endregion Fields|}
        #region Properties

        public bool Property { get { return _field; } }

        {|#1:#endregion // Fields|}

        #region Methods

        public bool GetValue() => _field;

        #endregion // Methods
    }
}