﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jhu.Graywulf.Sql.NameResolution {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ExceptionMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Jhu.Graywulf.Sql.NameResolution.ExceptionMessages", typeof(ExceptionMessages).Assembly);
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
        ///   Looks up a localized string similar to Ambigous column reference &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string AmbigousColumnReference {
            get {
                return ResourceManager.GetString("AmbigousColumnReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ambigous table reference &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string AmbigousTableReference {
            get {
                return ResourceManager.GetString("AmbigousTableReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Database name &apos;{0}&apos; illegally specified at ({1},{2})..
        /// </summary>
        internal static string DatabaseNameNotAllowed {
            get {
                return ResourceManager.GetString("DatabaseNameNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reference to a different table &apos;{0}&apos; is not allowed in table hint expression but found at ({1},{2})..
        /// </summary>
        internal static string DifferentTableReferenceInHintNotAllowed {
            get {
                return ResourceManager.GetString("DifferentTableReferenceInHintNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate column alias &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string DuplicateColumnAlias {
            get {
                return ResourceManager.GetString("DuplicateColumnAlias", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate output table name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string DuplicateOutputTable {
            get {
                return ResourceManager.GetString("DuplicateOutputTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate table alias &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string DuplicateTableAlias {
            get {
                return ResourceManager.GetString("DuplicateTableAlias", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate variable name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string DuplicateVariableName {
            get {
                return ResourceManager.GetString("DuplicateVariableName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to System function calls are not allowed as table sources. Use user-defined function instead..
        /// </summary>
        internal static string FunctionCallNotAllowed {
            get {
                return ResourceManager.GetString("FunctionCallNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alias not defined for subquery column at ({1},{2})..
        /// </summary>
        internal static string MissingColumnAlias {
            get {
                return ResourceManager.GetString("MissingColumnAlias", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scalar variable or variable with user-defined type expected at ({1},{2})..
        /// </summary>
        internal static string ScalarVariableExpected {
            get {
                return ResourceManager.GetString("ScalarVariableExpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stars (*) in column expressions are not allowed but found at ({1},{2})..
        /// </summary>
        internal static string StarColumnNotAllowed {
            get {
                return ResourceManager.GetString("StarColumnNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The target dataset &apos;{0}&apos; specified at ({1},{2}) is read only..
        /// </summary>
        internal static string TargetDatasetReadOnly {
            get {
                return ResourceManager.GetString("TargetDatasetReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown function name found at ({1},{2})..
        /// </summary>
        internal static string UnknownFunctionName {
            get {
                return ResourceManager.GetString("UnknownFunctionName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolvable column name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string UnresolvableColumnReference {
            get {
                return ResourceManager.GetString("UnresolvableColumnReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolvable dataset name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string UnresolvableDatasetReference {
            get {
                return ResourceManager.GetString("UnresolvableDatasetReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolvable function name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string UnresolvableFunctionReference {
            get {
                return ResourceManager.GetString("UnresolvableFunctionReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolvable table or view name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string UnresolvableTableReference {
            get {
                return ResourceManager.GetString("UnresolvableTableReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolvable function name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string UnresolvableUdfReference {
            get {
                return ResourceManager.GetString("UnresolvableUdfReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unresolvable variable name &apos;{0}&apos; found at ({1},{2})..
        /// </summary>
        internal static string UnresolvableVariableReference {
            get {
                return ResourceManager.GetString("UnresolvableVariableReference", resourceCulture);
            }
        }
    }
}
