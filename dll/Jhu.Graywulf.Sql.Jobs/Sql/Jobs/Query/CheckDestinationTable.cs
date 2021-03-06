﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Activities;
using Jhu.Graywulf.Tasks;

namespace Jhu.Graywulf.Sql.Jobs.Query
{
    public class CheckDestinationTable : JobCodeActivity, IJobActivity
    {
        [RequiredArgument]
        public InArgument<SqlQuery> Query { get; set; }

        protected override void OnExecute(CodeActivityContext activityContext)
        {
            SqlQuery query = Query.Get(activityContext);

            using (RegistryContext registryContext = query.CreateContext())
            {
                query.InitializeQueryObject(registryContext);

                // TODO
                query.Parameters.Destination.CheckTableExistence();
            }
        }
    }
}
