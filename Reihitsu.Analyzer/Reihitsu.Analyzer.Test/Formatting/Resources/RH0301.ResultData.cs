using System;
using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Analyzer.Test.Design.Resources
{
    internal class RH0101
    {
        #region Fields
        private bool _field;
        #endregion // Fields
        #region Properties

        public bool Property { get { return _field; } }

        #endregion // Properties

        #region Methods

        public bool GetValue() => _field;

        #endregion // Methods

        #region Outer 1

        #region Inner 1

        #endregion // Inner 1

        #endregion // Outer 1

        #region Outer 2

        #region Inner 2

        #endregion // Inner 2

        #endregion // Outer 2

        #region Outer 3

        #region Inner 3

        #endregion // Inner 3

        #endregion // Outer 3
    }
}