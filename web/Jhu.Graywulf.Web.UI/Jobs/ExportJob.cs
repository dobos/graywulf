﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.UI.Jobs
{
    public class ExportJob : Job
    {
        private string[] tables;
        private string fileFormat;
        private string uri;

        public override JobType Type
        {
            get { return JobType.Export; }
            set { throw new NotImplementedException(); }
        }

        public string[] Tables
        {
            get { return tables; }
            set { tables = value; }
        }

        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Used to display list of tables on the web UI.
        /// </remarks>
        public string TableList
        {
            get
            {
                if (tables != null)
                {
                    string res = "";
                    for (int i = 0; i < tables.Length; i++)
                    {
                        if (i > 0)
                        {
                            res += ", ";
                        }

                        res += tables[i];
                    }

                    return res;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public ExportJob()
        {
            InitializeMembers();
        }

        public ExportJob(JobInstance jobInstance)
            :base(jobInstance)
        {
            InitializeMembers();

            CopyFromJobInstance(jobInstance);
        }

        private void InitializeMembers()
        {
            this.tables = null;
            this.uri = null;
        }

        private void CopyFromJobInstance(JobInstance jobInstance)
        {
            // Because job parameter type might come from an unknown 
            // assembly, instead of deserializing, read xml directly here

            if (jobInstance.Parameters.ContainsKey(Jhu.Graywulf.Jobs.Constants.JobParameterExport))
            {
                var xml = new XmlDocument();
                xml.LoadXml(jobInstance.Parameters[Jhu.Graywulf.Jobs.Constants.JobParameterExport].XmlValue);

                // TODO:
                // jobDescription.SchemaName = GetXmlInnerText(xml, "ExportTables/Sources/TableOrView/SchemaName");
                // jobDescription.ObjectName = GetXmlInnerText(xml, "ExportTables/Sources/TableOrView/ObjectName");
                // jobDescription.Path = GetXmlInnerText(xml, "ExportTables/Destinations/DataFileBase/Uri");
            }
        }
    }
}
