﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Reihitsu.Analyzer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AnalyzerResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AnalyzerResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Reihitsu.Analyzer.AnalyzerResources", typeof(AnalyzerResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The logical operator ! should not be used for clarity..
        /// </summary>
        internal static string RH0001MessageFormat {
            get {
                return ResourceManager.GetString("RH0001MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The logical operator ! should not be used for clarity..
        /// </summary>
        internal static string RH0001Title {
            get {
                return ResourceManager.GetString("RH0001Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Private auto-implemented properties should not be used..
        /// </summary>
        internal static string RH0101MessageFormat {
            get {
                return ResourceManager.GetString("RH0101MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Private auto-implemented properties should not be used..
        /// </summary>
        internal static string RH0101Title {
            get {
                return ResourceManager.GetString("RH0101Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The description of the #region and #endregion should match..
        /// </summary>
        internal static string RH0301MessageFormat {
            get {
                return ResourceManager.GetString("RH0301MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The description of the #region and #endregion should match..
        /// </summary>
        internal static string RH0301Title {
            get {
                return ResourceManager.GetString("RH0301Title", resourceCulture);
            }
        }
    }
}
