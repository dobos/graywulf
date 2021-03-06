﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace Jhu.Graywulf.Web.Api.V1
{
    [DataContract]
    [Description("Represents a job of a specific type.")]
    public class JobRequest
    {
        [DataMember(Name = "queryJob", EmitDefaultValue = false)]
        [DefaultValue(null)]
        [Description("Conveys a query job.")]
        public QueryJob QueryJob { get; set; }

        [DataMember(Name = "exportJob", EmitDefaultValue = false)]
        [DefaultValue(null)]
        [Description("Conveys a data table export job.")]
        public ExportJob ExportJob { get; set; }

        [DataMember(Name = "importJob", EmitDefaultValue = false)]
        [DefaultValue(null)]
        [Description("Conveys a data table import job.")]
        public ImportJob ImportJob { get; set; }

        [DataMember(Name = "copyJob", EmitDefaultValue = false)]
        [DefaultValue(null)]
        [Description("Conveys a data table copy job.")]
        public CopyJob CopyJob { get; set; }

        [DataMember(Name = "sqlScriptJob", EmitDefaultValue = false)]
        [DefaultValue(null)]
        [Description("Conveys a SQL script job.")]
        public SqlScriptJob SqlScriptJob { get; set; }

        public JobRequest()
        {
        }

        public Job GetValue()
        {
            if (QueryJob != null)
            {
                return QueryJob;
            }
            else if (ExportJob != null)
            {
                return ExportJob;
            }
            else if (ImportJob != null)
            {
                return ImportJob;
            }
            else if (CopyJob != null)
            {
                return CopyJob;
            }
            else if (SqlScriptJob != null)
            {
                return SqlScriptJob;
            }
            else
            {
                throw new ArgumentNullException();  // TODO
            }
        }

        public void SetValue(Job job)
        {
            if (job is QueryJob)
            {
                QueryJob = (QueryJob)job;
            }
            else if (job is ExportJob)
            {
                ExportJob = (ExportJob)job;
            }
            else if (job is ImportJob)
            {
                ImportJob = (ImportJob)job;
            }
            else if (job is CopyJob)
            {
                CopyJob = (CopyJob)job;
            }
            else if (job is SqlScriptJob)
            {
                SqlScriptJob = (SqlScriptJob)job;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
