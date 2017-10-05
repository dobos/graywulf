﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Jhu.Graywulf.Web.UI.Apps.Query.Examples
{
    public partial class Examples : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var master = ((UI.Masters.UI)Master);
            master.Menu.SelectedButton = "examples";

            toolbar.RootPath = "~/Assets/Query/Examples";
        }
    }
}