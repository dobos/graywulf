﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Tasks;
using Jhu.Graywulf.Sql.Schema;

namespace Jhu.Graywulf.IO.Jobs.CopyTables
{
    [Serializable]
    public class CopyTablesJobFactory : JobFactoryBase
    {
        #region Static members

        public static CopyTablesJobFactory Create(RegistryContext context)
        {
            return new CopyTablesJobFactory(context);
        }

        #endregion
        #region Constructors and initializer

        protected CopyTablesJobFactory()
            : base()
        {
            InitializeMembers(new StreamingContext());
        }

        protected CopyTablesJobFactory(RegistryContext context)
            : base(context)
        {
            InitializeMembers(new StreamingContext());
        }

        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
        }

        #endregion

        public CopyTablesParameters CreateParameters()
        {
            return new CopyTablesParameters();
        }

        public JobInstance ScheduleAsJob(CopyTablesParameters parameters, string queueName, TimeSpan timeout, string comments)
        {
            var job = CreateJobInstance(null, GetJobDefinitionName(), queueName, timeout, comments);
            job.Parameters[Registry.Constants.JobParameterParameters].Value = parameters;
            return job;
        }

        private string GetJobDefinitionName()
        {
            return EntityFactory.CombineName(EntityType.JobDefinition, RegistryContext.Cluster.Name, RegistryContext.Domain.Name, RegistryContext.Federation.Name, typeof(CopyTablesJob).Name);
        }
    }
}
