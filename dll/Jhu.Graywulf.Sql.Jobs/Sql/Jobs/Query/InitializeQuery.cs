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
    public class InitializeQuery : JobCodeActivity, IJobActivity
    {
        public OutArgument<Guid> EntityGuid { get; set; }

        [RequiredArgument]
        public InArgument<SqlQueryParameters> Parameters { get; set; }

        [RequiredArgument]
        public OutArgument<SqlQuery> Query { get; set; }

        protected override void OnExecute(CodeActivityContext activityContext)
        {
            var parameters = Parameters.Get(activityContext);
            var query = (SqlQuery)Activator.CreateInstance(Type.GetType(parameters.QueryTypeName));
            query.Parameters = parameters;

            // Single server mode will run on one partition by definition,
            // Graywulf mode has to look at the registry for available machines
            switch (query.Parameters.ExecutionMode)
            {
                case ExecutionMode.SingleServer:
                    query.InitializeQueryObject(null, null);
                    break;
                case ExecutionMode.Graywulf:
                    using (RegistryContext registryContext = ContextManager.Instance.CreateReadOnlyContext())
                    {
                        query.InitializeQueryObject(registryContext);
                        query.Validate();
                        query.UpdateParameters();
                        query.IdentifyTablesForStatistics();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            Query.Set(activityContext, query);
        }
    }
}
