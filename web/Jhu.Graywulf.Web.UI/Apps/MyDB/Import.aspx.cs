﻿using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO.Jobs.ImportTables;
using Jhu.Graywulf.Sql.Schema;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Tasks;
using Jhu.Graywulf.Web.Api.V1;

namespace Jhu.Graywulf.Web.UI.Apps.MyDB
{
    public partial class Import : FederationPageBase
    {
        public static string GetUrl()
        {
            return GetUrl(null);
        }

        public static string GetUrl(string datasetName)
        {
            var url = "~/Apps/MyDb/Import.aspx";

            if (datasetName != null)
            {
                url += "?dataset=" + HttpContext.Current.Server.UrlEncode(datasetName);
            }

            return url;
        }

        #region Private member variables

        private Dictionary<string, Control> importForms;
        private CopyTableBase task;

        #endregion

        public Import()
            : base(false)
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.importForms = new Dictionary<string, Control>();
        }

        #region Event handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            CreateImportMethodForms();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RefreshDatasetList(destinationTableForm.DatasetList);
                RefreshImportMethodList();
            }
        }
        
        protected void ImportMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (importMethod.SelectedValue == "upload")
            {
                uploadForm.Visible = true;

                // Set all plugin forms invisible
                foreach (var control in importForms)
                {
                    control.Value.Visible = false;
                }
            }
            else
            {
                uploadForm.Visible = false;

                foreach (var control in importForms)
                {
                    control.Value.Visible = StringComparer.InvariantCultureIgnoreCase.Compare(control.Key, importMethod.SelectedValue) == 0;
                }
            }
        }

        protected void Ok_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                if (importMethod.SelectedValue == "upload")
                {
                    InitializeImportTask();
                    uploadResultsForm.Visible = true;
                }
                else
                {
                    ScheduleImportJob();
                    jobResultsForm.Visible = true;
                }

                importForm.Visible = false;
            }
        }

        protected void Cancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(OriginalReferer, false);
        }

        protected void Back_Click(object sender, EventArgs e)
        {
            Response.Redirect(Jhu.Graywulf.Web.UI.Apps.Jobs.Default.GetUrl(), false);
        }

        #endregion

        private void CreateImportMethodForms()
        {
            var factory = ImportTablesJobFactory.Create(RegistryContext.Federation);

            foreach (var method in factory.EnumerateMethods())
            {
                var control = LoadControl(method.GetForm());

                control.Visible = false;
                importFormPlaceholder.Controls.Add(control);
                importForms.Add(method.ID, control);
            }
        }

        private void RefreshImportMethodList()
        {
            var factory = ImportTablesJobFactory.Create(RegistryContext.Federation);

            foreach (var method in factory.EnumerateMethods())
            {
                importMethod.Items.Add(new ListItem(method.Description, method.ID));
            }
        }

        private void InitializeImportTask()
        {
            // TODO: how to import archives?

            var uri = uploadForm.Uri;
            var file = fileFormatForm.GetDataFile(uri);
            var table = Web.Api.V1.DestinationTable.GetDestinationTable(FederationContext, destinationTableForm.Dataset.Name, destinationTableForm.TableName);

            importOptionsForm.SetFileOptions(file);

            task = uploadForm.CreateTableImporter(file, table);

            RegisterAsyncTask(new PageAsyncTask(ImportViaBrowserAsync));
        }

        private async Task ImportViaBrowserAsync()
        {
            await uploadForm.OpenTableImporter(task);
            await task.ExecuteAsync();

            foreach (var r in task.Results)
            {
                var li = new ListItem()
                {
                    Text = String.Format("{0} > {1} ({2})", r.SourceFileName, r.DestinationTable, r.Status)
                };

                resultTableList.Items.Add(li);
            }
        }

        private void ScheduleImportJob()
        {
            var form = (IImportTablesForm)importForms[importMethod.SelectedValue];

            var uri = form.Uri;
            var credentials = form.Credentials;
            var file = fileFormatForm.GetFormat();
            var dataset = destinationTableForm.DatasetName;
            var table = destinationTableForm.TableName;

            importOptionsForm.SetFileOptions(file);

            var job = new ImportJob()
            {
                Uri = uri,
                Credentials = credentials == null ? null : new Web.Api.V1.Credentials(credentials),
                FileFormat = file,
                Destination = new Web.Api.V1.DestinationTable()
                {
                    Dataset = dataset,
                    Table = table,
                },
                Comments = commentsForm.Comments,
                Queue = JobQueue.Long,
            };

            new JobsService(FederationContext).SubmitJob(job);
        }
    }
}