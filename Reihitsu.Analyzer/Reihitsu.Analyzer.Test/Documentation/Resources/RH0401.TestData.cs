using System;
using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Analyzer.Test.Design.Resources
{
    internal abstract class RH0401Base
    {
        /// <summary>
        /// Base documentation
        /// </summary>
        public abstract void TestMethod();

        /// <summary>
        /// Base documentation
        /// </summary>
        public abstract int TestProperty { get; set; }

        /// <summary>
        /// Base documentation
        /// </summary>
        public abstract event EventHandler TestEvent;

        /// <summary>
        /// Base documentation
        /// </summary>
        public abstract int this[int i] { get; set; }
    }

    internal class RH0401Implementation : RH0401Base
    {
        ///{|#0: <summary>
        /// Implementation documentation
        /// </summary>
|}        public override void TestMethod()
        {
        }

        ///{|#1: <summary>
        /// Implementation documentation
        /// </summary>
|}        public override int TestProperty
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        ///{|#2: <summary>
        /// Implementation documentation
        /// </summary>
|}        public override event EventHandler TestEvent
        {
            add { }
            remove { }
        }

        ///{|#3: <summary>
        /// Implementation documentation
        /// </summary>
|}        public override int this[int i]
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
    }
}