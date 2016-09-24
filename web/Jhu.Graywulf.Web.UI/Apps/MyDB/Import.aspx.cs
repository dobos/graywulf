﻿using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.Jobs.ImportTables;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Tasks;
using Jhu.Graywulf.Jobs.ImportTables;
using Jhu.Graywulf.Web.Api.V1;

namespace Jhu.Graywulf.Web.UI.Apps.MyDB
{
    public partial class Import : CustomPageBase
    {
        public static string GetUrl()
        {
            return "~/Apps/MyDb/Import.aspx";
        }

        #region Private member variables

        private Dictionary<string, Control> importForms;

        #endregion

        public Import()
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

        protected void ToggleAdvanced_Click(object sender, EventArgs e)
        {
            detailsPanel.Visible = !detailsPanel.Visible;

            if (detailsPanel.Visible)
            {
                toggleAdvanced.Text = "simple mode";
            }
            else
            {
                toggleAdvanced.Text = "advanced mode";
            }
        }

        protected void Ok_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                if (importMethod.SelectedValue == "upload")
                {
                    ImportUploadedFile();
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
            Response.Redirect(Jhu.Graywulf.Web.UI.Apps.MyDB.Tables.GetUrl(), false);
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

        private void ImportUploadedFile()
        {
            var uri = uploadForm.Uri;
            var file = fileFormatForm.GetDataFile(uri);
            var table = ImportJob.GetDestinationTable(FederationContext, destinationTableForm.TableName);
            var importer = uploadForm.GetTableImporter(file, table);

            importer.Execute();

            foreach (var r in importer.Results)
            {
                var li = new ListItem()
                {
                    Text = String.Format("{0} > {1}.{2} ({3})", r.FileName, r.SchemaName, r.TableName, r.Status)
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
            var table = destinationTableForm.TableName;

            var job = new ImportJob()
            {
                Uri = uri,
                Credentials = credentials == null ? null : new Web.Api.V1.Credentials(credentials),
                FileFormat = file,
                Table = table,

                Comments = commentsForm.Comments,
                Queue = JobQueue.Long,
            };

            job.Schedule(FederationContext);
        }
    }
}