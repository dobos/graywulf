﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.ServiceModel.Security;
using System.Web.Routing;
using System.Security.Permissions;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.Api
{
    [ServiceContract]
    public interface ITablesService
    {
        [OperationContract]
        [WebGet(UriTemplate = "hello")]
        string Hello();
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
    [RestServiceBehavior]
    public class TablesService : ServiceBase, ITablesService
    {
        public string Hello()
        {
            return string.Format("Hello {0}", System.Threading.Thread.CurrentPrincipal.Identity.Name);
        }
    }
}
