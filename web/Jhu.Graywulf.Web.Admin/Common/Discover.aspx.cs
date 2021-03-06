﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.Admin.Common
{
    public partial class Discover : CommonPage
    {
        private List<Entity> update;
        private List<Entity> delete;
        private List<Entity> create;
        private List<Entity> error;

        public static string GetUrl(Guid key)
        {
            return String.Format("~/Common/Discover.aspx?key={0}", key);
        }

        private void RunDiscovery()
        {
            LoadEntities();

            update = new List<Entity>();
            delete = new List<Entity>();
            create = new List<Entity>();
            error = new List<Entity>();

            foreach (var e in Entities)
            {
                e.Discover(update, delete, create, error, true);
            }
        }

        protected override void UpdateForm()
        {
            RunDiscovery();

            if (Entities.Length > 1)
            {
                Name.Text = "Multiple objects";
            }
            else
            {
                Name.Text = Entities[0].Name;
            }

            if (error.Count > 0)
            {
                errorListDiv.Visible = true;
                errorList.Items.Clear();

                foreach (var entity in error)
                {
                    errorList.Items.Add(String.Format("{0} - {1} ({2})", entity.Name, entity.EntityType, "discovery error"));
                }
            }

            updateList.Items.Clear();

            foreach (var entity in update)
            {
                updateList.Items.Add(String.Format("{0} - {1} ({2})", entity.Name, entity.EntityType, "to be updated"));
            }

            foreach (var entity in delete)
            {
                updateList.Items.Add(String.Format("{0} - {1} ({2})", entity.Name, entity.EntityType, "to be deleted"));
            }

            foreach (var entity in create)
            {
                updateList.Items.Add(String.Format("{0} - {1} ({2})", entity.Name, entity.EntityType, "to be created"));
            }
        }

        protected override void ProcessForm()
        {
            RunDiscovery();

            var ef = new EntityFactory(RegistryContext);
            ef.ApplyChanges(update, delete, create);

            Response.Redirect(OriginalReferer, false);
        }
    }
}