﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using mscoree;

namespace Jhu.Graywulf.Test
{
    [Serializable]
    public class CrossAppDomainSingleton<T> : MarshalByRefObject
        where T : new()
    {
        private const string SingletonName = "CrossAppDomainSingleton";

        private static object syncRoot;
        private static Dictionary<string, T> instances;

        static CrossAppDomainSingleton()
        {
            syncRoot = new object();
            instances = new Dictionary<string, T>();
        }

        public static T Instance
        {
            get
            {
                lock (syncRoot)
                {
                    return GetNamedInstance("__defaultinstance");
                }
            }
        }


        public static T GetNamedInstance(string name)
        {
            lock (syncRoot)
            {
                if (!instances.ContainsKey(name))
                {
                    CreateStaticInstance(name);
                }

                return instances[name];
            }
        }

        private T wrappedInstance;

        public T WrappedInstance
        {
            get { return wrappedInstance; }
        }

        public CrossAppDomainSingleton()
        {
            this.wrappedInstance = new T();
        }

        private static void CreateStaticInstance(string name)
        {
            var t = typeof(CrossAppDomainSingleton<T>);
            var ad = GetSingletonAppDomain();

            var s = (CrossAppDomainSingleton<T>)ad.GetData(String.Format("{0}+{1}", t.FullName, name));

            if (s == null)
            {
                s = (CrossAppDomainSingleton<T>)ad.CreateInstanceAndUnwrap(t.Assembly.FullName, t.FullName);
                ad.SetData(String.Format("{0}+{1}", t.FullName, name), s);
            }

            instances.Add(name, s.WrappedInstance);
        }

        private static AppDomain GetSingletonAppDomain()
        {
            AppDomain ad;

            using (var mutex = new Mutex(false, SingletonName))
            {
                mutex.WaitOne();

                ad = GetAppDomain(SingletonName);

                mutex.ReleaseMutex();
            }

            return ad;
        }

        private static AppDomain GetAppDomain(string name)
        {
            var ad = EnumerateAppDomains().FirstOrDefault(i => i.FriendlyName == name);

            if (ad == null)
            {
                var info = new AppDomainSetup();
                info.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                info.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                info.ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;
                info.DisallowApplicationBaseProbing = false;

                ad = AppDomain.CreateDomain(name, null, info);
            }

            return ad;
        }

        /// <summary>
        /// Code from https://stackoverflow.com/questions/388554/list-appdomains-in-process
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AppDomain> EnumerateAppDomains()
        {
            IntPtr enumHandle = IntPtr.Zero;
            ICorRuntimeHost host = null;

            try
            {
                host = new CorRuntimeHostClass();
                host.EnumDomains(out enumHandle);
                object domain = null;

                host.NextDomain(enumHandle, out domain);
                while (domain != null)
                {
                    yield return (AppDomain)domain;
                    host.NextDomain(enumHandle, out domain);
                }
            }
            finally
            {
                if (host != null)
                {
                    if (enumHandle != IntPtr.Zero)
                    {
                        host.CloseEnum(enumHandle);
                    }

                    Marshal.ReleaseComObject(host);
                }
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
