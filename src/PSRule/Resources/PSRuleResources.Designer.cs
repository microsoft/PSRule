﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PSRule.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class PSRuleResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PSRuleResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PSRule.Resources.PSRuleResources", typeof(PSRuleResources).Assembly);
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
        ///   Looks up a localized string similar to A circular rule dependency was detected. The rule &apos;{0}&apos; depends on &apos;{1}&apos; which also depend on &apos;{0}&apos;..
        /// </summary>
        internal static string DependencyCircularReference {
            get {
                return ResourceManager.GetString("DependencyCircularReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The dependency &apos;{0}&apos; for &apos;{1}&apos; could not be found. Check that the rule is defined in a .Rule.ps1 file within the search path..
        /// </summary>
        internal static string DependencyNotFound {
            get {
                return ResourceManager.GetString("DependencyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A rule with the same name &apos;{0}&apos; already exists..
        /// </summary>
        internal static string DuplicateRuleId {
            get {
                return ResourceManager.GetString("DuplicateRuleId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Rule nesting was detected in rule &apos;{0}&apos;. Rules must not be nested..
        /// </summary>
        internal static string InvalidRuleNesting {
            get {
                return ResourceManager.GetString("InvalidRuleNesting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An invalid rule result was returned for {0}. Conditions must return boolean $True or $False..
        /// </summary>
        internal static string InvalidRuleResult {
            get {
                return ResourceManager.GetString("InvalidRuleResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Target object &apos;{0}&apos; has not been processed because no matching rules were found..
        /// </summary>
        internal static string ObjectNotProcessed {
            get {
                return ResourceManager.GetString("ObjectNotProcessed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options file does not exist..
        /// </summary>
        internal static string OptionsNotFound {
            get {
                return ResourceManager.GetString("OptionsNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [FAIL] -- {0}:: Reported for &apos;{1}&apos;.
        /// </summary>
        internal static string OutcomeRuleFail {
            get {
                return ResourceManager.GetString("OutcomeRuleFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PASS] -- {0}:: Reported for &apos;{1}&apos;.
        /// </summary>
        internal static string OutcomeRulePass {
            get {
                return ResourceManager.GetString("OutcomeRulePass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Inconclusive result reported for &apos;{1}&apos; @{0}..
        /// </summary>
        internal static string RuleInconclusive {
            get {
                return ResourceManager.GetString("RuleInconclusive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find a matching rule. Please check that Path, Name and Tag parameters are correct..
        /// </summary>
        internal static string RuleNotFound {
            get {
                return ResourceManager.GetString("RuleNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The script was not found..
        /// </summary>
        internal static string ScriptNotFound {
            get {
                return ResourceManager.GetString("ScriptNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not serialize a null PSObject..
        /// </summary>
        internal static string SerializeNullPSObject {
            get {
                return ResourceManager.GetString("SerializeNullPSObject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create path.
        /// </summary>
        internal static string ShouldCreatePath {
            get {
                return ResourceManager.GetString("ShouldCreatePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Write file.
        /// </summary>
        internal static string ShouldWriteFile {
            get {
                return ResourceManager.GetString("ShouldWriteFile", resourceCulture);
            }
        }
    }
}
