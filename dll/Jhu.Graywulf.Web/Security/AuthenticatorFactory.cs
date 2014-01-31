﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.Graywulf.Security
{
    /// <summary>
    /// Implements functionality to manage authenticator and
    /// protocols
    /// </summary>
    public class AuthenticatorFactory
    {
        /// <summary>
        /// Creates an authenticator factory class from the
        /// type name.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static AuthenticatorFactory Create(string typename)
        {
            Type type = null;

            if (typename != null)
            {
                type = Type.GetType(typename);
            }

            // If config is incorrect, fall back to known types.
            if (type == null)
            {
                type = typeof(AuthenticatorFactory);
            }

            return (AuthenticatorFactory)Activator.CreateInstance(type, true);
        }

        public InteractiveAuthenticatorBase[] CreateInteractiveAuthenticators()
        {
            return new[] 
            {
                new OpenIDAuthenticator()
                {
                    AuthorityName = "VOID",
                    AuthorityUri="https://sso.usvao.org/openid/provider",
                    DisplayName = "VO OpenID",
                    DiscoveryUrl = "https://sso.usvao.org/openid/provider_id"
                },
                new OpenIDAuthenticator()
                {
                    AuthorityName = "Google",
                    AuthorityUri="https://www.google.com/accounts/o8/ud",
                    DisplayName = "GoogleID",
                    DiscoveryUrl="https://www.google.com/accounts/o8/id"
                }
            };
        }

        public InteractiveAuthenticatorBase CreateInteractiveAuthenticator(string protocol, string authority)
        {
            var q = from a in CreateInteractiveAuthenticators()
                    where
                        StringComparer.InvariantCultureIgnoreCase.Compare(a.Protocol, protocol) == 0 &&
                        StringComparer.InvariantCultureIgnoreCase.Compare(a.AuthorityUri, authority) == 0
                    select a;

            return q.FirstOrDefault();
        }

        public RequestAuthenticatorBase[] CreateRequestAuthenticators()
        {
            return new RequestAuthenticatorBase[0];
        }

    }
}