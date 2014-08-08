﻿using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Security;
using System.Security.Principal;

namespace Jhu.Graywulf.RemoteService
{
    /// <summary>
    /// Implements method to establish channels with remote services to
    /// execute delegated tasks.
    /// </summary>
    public static class RemoteServiceHelper
    {
        public const ImpersonationOption DefaultImpersonation = ImpersonationOption.Allowed;

        #region Remote object creation functions

        /// <summary>
        /// Returns a proxy to the remote service control object.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IRemoteServiceControl GetControlObject(string host)
        {
            var fdqn = GetFullyQualifiedDnsName(host);
            var tcp = CreateNetTcpBinding();
            var ep = CreateEndpointAddress(fdqn, "Control");

            return CreateChannel<IRemoteServiceControl>(tcp, ep);
        }

        /// <summary>
        /// Returns a proxy to a custom service object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="host"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the remote service does not serve the requested type of service yet, the
        /// function registers it before creating a proxy.
        /// </remarks>
        public static T CreateObject<T>(string host)
            where T : IRemoteService
        {
            // Get the uri to the requested service from the remote server
            var sc = RemoteServiceHelper.GetControlObject(GetFullyQualifiedDnsName(host));
            var uri = sc.GetServiceEndpointUri(typeof(T).AssemblyQualifiedName);

            return CreateChannel<T>(CreateNetTcpBinding(), CreateEndpointAddress(uri));
        }

        /// <summary>
        /// Creates a channel of type T to the given endpoint via the given binding.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tcp"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private static T CreateChannel<T>(NetTcpBinding tcp, EndpointAddress endpoint)
        {
            var cf = new ChannelFactory<T>(tcp, endpoint);

            // Ensure delegation
            cf.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
            cf.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

            return cf.CreateChannel();
        }

        #endregion

        /// <summary>
        /// Returns a NetTcpBinding object initialized to support Kerberos authentication over TCP
        /// </summary>
        /// <returns></returns>
        public static NetTcpBinding CreateNetTcpBinding()
        {
            var tcp = new NetTcpBinding();
            tcp.ReceiveTimeout = TimeSpan.FromHours(2);     // **** TODO
            tcp.SendTimeout = TimeSpan.FromHours(2);
            tcp.OpenTimeout = TimeSpan.FromHours(2);

            // Configure security to use windows credentials and signed messages
            tcp.Security.Mode = SecurityMode.Transport;
            tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            tcp.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.Sign;
            tcp.Security.Message.ClientCredentialType = MessageCredentialType.Windows;

            // Large XML messages require these quotas set to large values
            tcp.ReaderQuotas.MaxArrayLength = 0x7FFFFFFF;
            tcp.ReaderQuotas.MaxDepth = 0x7FFFFFFF;
            tcp.ReaderQuotas.MaxStringContentLength = 0x7FFFFFFF;

            // It might be necessary to increase buffer size on large messages
            //tcp.MaxBufferPoolSize = 0x1000000;  // 16M

            return tcp;
        }

        /// <summary>
        /// Creates an EndpointAddress from a hostname and service name.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static EndpointAddress CreateEndpointAddress(string host, string service)
        {
            return CreateEndpointAddress(CreateEndpointUri(host, service));
        }

        /// <summary>
        /// Creates and initializes an EndpointAddress object based on an URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <remarks>
        /// The EndpointAddress is initialized to support delegation via Kerberos.
        /// Different initialization for localhost is handled.
        /// </remarks>
        public static EndpointAddress CreateEndpointAddress(Uri uri)
        {
            EndpointAddress ea;

            if (StringComparer.InvariantCultureIgnoreCase.Compare(uri.Host, GetFullyQualifiedDnsName()) == 0)
            {
                ea = new EndpointAddress(uri);
            }
            else
            {
                ea = new EndpointAddress(uri, new SpnEndpointIdentity("Graywulf/RemoteService"));
            }

            return ea;
        }

        public static Uri CreateEndpointUri(string host, string service)
        {
            return new Uri(String.Format("net.tcp://{0}:{1}/{2}", host, AppSettings.TcpPort, service));
        }

        /// <summary>
        /// Creates a new type of service identified by its contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static Uri CreateService(Type contract, out ServiceHost host, out ServiceEndpoint endpoint)
        {
            // See if contractType is decorated with the RemoteServiceClassAttribute
            var attr = contract.GetCustomAttributes(typeof(RemoteServiceAttribute), false);

            if (attr == null || attr.Length != 1)
            {
                // TODO
                throw new InvalidOperationException("Contracts must be decorated with the RemoteServiceClassAttribute for automatic service registration.");
            }

            var serviceType = ((RemoteServiceAttribute)attr[0]).Type.AssemblyQualifiedName;

            // Attempt to load type
            var service = Type.GetType(serviceType);

            if (!service.IsSubclassOf(typeof(RemoteServiceBase)))
            {
                // TODO
                throw new InvalidOperationException("Service class must derive from Jhu.Graywulf.RemoteService.RemoteServiceBase");
            }

            if (service == null || contract == null)
            {
                throw new Exception("Type not found.");    // TODO
            }

            // Everything is OK, initialize service
            var fdqn = RemoteServiceHelper.GetFullyQualifiedDnsName();
            var ep = RemoteServiceHelper.CreateEndpointUri(fdqn, service.FullName);
            var tcp = RemoteServiceHelper.CreateNetTcpBinding();

            host = new ServiceHost(service, ep);

            // Create endpoint
            endpoint = host.AddServiceEndpoint(contract, tcp, ep);

            // Turn on detailed debug info
#if DEBUG
            var sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb == null)
            {
                sdb = new ServiceDebugBehavior();
                host.Description.Behaviors.Add(sdb);
            }
            sdb.IncludeExceptionDetailInFaults = true;
#endif

            // Turn on impersonation
            /*
            var sab = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            if (sab == null)
            {
                sab = new ServiceAuthorizationBehavior();
                host.Description.Behaviors.Add(sab);
            }
            sab.ImpersonateCallerForAllOperations = true;
            */

            // Unthrottle service to increase throughput
            // Service is behind a firewall, no DOS attacks will happen
            // TODO: copy these settings to the control endpoint
            var tb = host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (tb == null)
            {
                tb = new ServiceThrottlingBehavior();
                host.Description.Behaviors.Add(tb);
            }
            tb.MaxConcurrentCalls = 1024;
            tb.MaxConcurrentInstances = Int32.MaxValue;
            tb.MaxConcurrentSessions = 1024;

            host.Open();

            return endpoint.Address.Uri;
        }

        /// <summary>
        /// Returns the DNS name of the current machine.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This function should return a fully and well qualified domain name,
        /// otherwise authentication wont work. To set up DNS names correctly on
        /// machines outside a windows domain, the primary DNS suffix must be set
        /// in the computer name configurations. ipconfig /all should give the
        /// currently set suffix.
        /// </remarks>
        public static string GetFullyQualifiedDnsName()
        {
            var ipprop = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            // TODO: reverse lookup to get FQDN, it fails on current GW config at JHU!
            /*var name = string.Format("{0}.{1}", ipprop.HostName, ipprop.DomainName);
            return name;*/

            return ipprop.HostName;
        }

        /// <summary>
        /// Returns the DNS name of any host identified by its host name.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string GetFullyQualifiedDnsName(string host)
        {
            // If host is localhost, simply replace it with the known host name,
            // otherwise windows authentication will not work within the local
            // machine

            if (StringComparer.InvariantCultureIgnoreCase.Compare(host, "localhost") == 0)
            {
                return GetFullyQualifiedDnsName();
            }

            // TODO: reverse lookup to get FQDN, it fails on current GW config at JHU!
            /*var name = System.Net.Dns.GetHostEntry(host).HostName;
            return name;*/

            return host;
        }
    }
}
