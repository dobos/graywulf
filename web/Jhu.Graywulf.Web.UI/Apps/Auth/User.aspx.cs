﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.IO;
using Jhu.Graywulf.AccessControl;
using Jhu.Graywulf.Web;
using Jhu.Graywulf.Web.Security;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.UI.Apps.Auth
{
    public partial class User : PageBase
    {
        public static string GetUrl(string returnUrl)
        {
            return String.Format("~/Apps/Auth/User.aspx?ReturnUrl={0}", returnUrl);
        }

        protected Jhu.Graywulf.Registry.User user;

        private void UpdateForm()
        {
            if (RegistryUser == null)
            {
                UserForm.Text = "Register new user";
                ChangePasswordPanel.Visible = false;
            }
            else
            {
                UserForm.Text = "Modify user account";
                Username.ReadOnly = true;

                PasswordTable.Visible = false;
                CaptchaTable.Visible = false;
                ChangePasswordPanel.Visible = true;
            }

            Username.Text = user.Name;
            FirstName.Text = user.FirstName;
            MiddleName.Text = user.MiddleName;
            LastName.Text = user.LastName;
            Email.Text = user.Email;
            Company.Text = user.Company;
            Address.Text = user.Address;
            WorkPhone.Text = user.WorkPhone;
        }

        private void SaveForm()
        {
            user.FirstName = FirstName.Text;
            user.MiddleName = MiddleName.Text;
            user.LastName = LastName.Text;
            user.Email = Email.Text;
            user.Company = Company.Text;
            user.Address = Address.Text;
            user.WorkPhone = WorkPhone.Text;
        }

        private void CreateUser()
        {
            var ip = IdentityProvider.Create(RegistryContext.Domain);

            user.Name = Username.Text.Trim();
            ip.CreateUser(user, Password.Text);

            // If user signed in with a temporary identity that means
            // it's a postback from a foreign identity provider. In this case
            // we need to associate the identity with the user now.
            if (TemporaryPrincipal != null)
            {
                var gwip = new GraywulfIdentityProvider(RegistryContext.Domain);
                var identity = (GraywulfIdentity)TemporaryPrincipal.Identity;
                gwip.AddUserIdentity(user, identity);
            }

            Util.EmailSender.Send(user, File.ReadAllText(MapPath("~/templates/ActivationEmail.xml")), BaseUrl);
        }

        private void ModifyUser()
        {
            var ip = IdentityProvider.Create(RegistryContext.Domain);
            ip.ModifyUser(user);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Create user object and load data if required

            if (RegistryUser != null)
            {
                user = RegistryUser;
            }
            else if (TemporaryPrincipal != null)
            {
                // Create a user object based on the temporary user generated
                // based on the OpenID etc. used at sign in.
                var identity = (GraywulfIdentity)TemporaryPrincipal.Identity;
                user = new Registry.User(identity.User);
                user.ParentReference.Value = RegistryContext.Domain;
                user.RegistryContext = RegistryContext;
            }
            else
            {
                // Must be under the current domain!
                user = new Registry.User(RegistryContext.Domain);
            }

            // Update form
            if (!IsPostBack)
            {
                UpdateForm();
            }
        }

        protected void ConfirmPasswordValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Make sure password and confirmation match
            args.IsValid = (Password.Text == ConfirmPassword.Text);
        }

        protected void DuplicateUsernameValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (RegistryUser == null)
            {
                var ip = IdentityProvider.Create(RegistryContext.Domain);
                args.IsValid = !ip.IsNameExisting(args.Value);
            }
        }

        protected void DuplicateEmailValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (RegistryUser == null)
            {
                var ip = IdentityProvider.Create(RegistryContext.Domain);
                args.IsValid = !ip.IsEmailExisting(args.Value);
            }
        }

        protected void OK_Click(object sender, EventArgs e)
        {
            if (IsValid)
            {
                SaveForm();

                if (!user.IsExisting)
                {
                    CreateUser();

                    Response.Redirect(Activate.GetUrl(ReturnUrl), false);
                }
                else
                {
                    ModifyUser();

                    UserForm.Visible = false;
                    SuccessForm.Visible = true;
                    SuccessOK.Attributes.Add("onClick", Util.UrlFormatter.GetClientRedirect(ReturnUrl));
                }
            }
        }

        protected void ChangePassword_Click(object sender, EventArgs e)
        {
            Response.Redirect(Jhu.Graywulf.Web.UI.Apps.Auth.ChangePassword.GetUrl(ReturnUrl), false);
        }

        protected void Cancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(ReturnUrl, false);
        }
    }
}