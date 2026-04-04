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

        #region Outer 1

        #region Inner 1

        #endregion // Inner 1

        #endregion // Outer 1

        #region Outer 2

        #region Inner 2

        {|#2:#endregion|}

        #endregion // Outer 2

        #region Outer 3

        #region Inner 3

        #endregion // Inner 3

        {|#3:#endregion|}
    }
}