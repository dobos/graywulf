﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jhu.Graywulf.Web.Services.Templates {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Javascript {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Javascript() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Jhu.Graywulf.Web.Services.Templates.Javascript", typeof(Javascript).Assembly);
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
        ///   Looks up a localized string similar to /* This is an auto-generated proxy */
        ///
        ///function __serviceName__Service(serviceUrl) {
        ///    this.serviceUrl = serviceUrl ? serviceUrl : &quot;__serviceUrl__&quot;;
        ///}
        ///
        ///__serviceName__Service.prototype.error = function (xhr, status, message) {
        ///    alert(message);
        ///}
        ///
        ///__serviceName__Service.prototype.__createUrl = function (pathParts, queryParts) {
        ///    var finalUrl = this.serviceUrl;
        ///    $.each(pathParts, function (i, part) {
        ///        finalUrl += &quot;/&quot; + part;
        ///    });
        ///
        ///    if (queryParts) {
        ///        finalUrl += [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Class {
            get {
                return ResourceManager.GetString("Class", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to __serviceName__Service.prototype.__methodName__ = function (__parameterList__) {
        ///    var __me = this;
        ///    var __pathParts = __pathParts__;
        ///    var __queryParts = __queryParts__;
        ///    var __data = __bodyParameter__;
        ///    var __dataType = __returnType__;
        ///    var __url = this.__createUrl(__pathParts, __queryParts);
        ///    var __request = {
        ///        dataType: __dataType
        ///    };
        ///
        ///    if (__data) {
        ///        __request.contentType = &quot;application/json&quot;;
        ///        __request.data = JSON.stringify(__data);
        ///    };
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Method {
            get {
                return ResourceManager.GetString("Method", resourceCulture);
            }
        }
    }
}
