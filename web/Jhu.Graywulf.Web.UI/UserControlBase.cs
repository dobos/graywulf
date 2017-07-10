﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Jhu.Graywulf.Web.UI
{
    public abstract class UserControlBase : UserControl, Registry.IRegistryContextObject
    {
        public string ReturnUrl
        {
            get { return Page.Request.QueryString[Constants.ReturnUrl] ?? ""; }
        }

        public Registry.RegistryContext RegistryContext
        {
            get { return ((PageBase)Page).RegistryContext; }
        }

        Registry.RegistryContext Registry.IRegistryContextObject.RegistryContext
        {
            get { return ((PageBase)Page).RegistryContext; }
            set { throw new InvalidOperationException(); }
        }
    }
}
