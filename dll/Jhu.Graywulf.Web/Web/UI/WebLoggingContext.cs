﻿using System;
using System.Web;
using System.Runtime.Serialization;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.Logging;

namespace Jhu.Graywulf.Web.UI
{
    public class WebLoggingContext : UserLoggingContext
    {
        #region Singleton access

        public static new WebLoggingContext Current
        {
            get
            {
                return LoggingContext.Current as WebLoggingContext;
            }
        }

        #endregion

        public WebLoggingContext()
            : this(true, AmbientContextStoreLocation.AsyncLocal | AmbientContextStoreLocation.WebHttpContext)
        {
            // overload
        }

        public WebLoggingContext(bool isAsync, AmbientContextStoreLocation supportedLocation)
            :base(isAsync, supportedLocation)
        {
            if (LoggingContext.Current is WebLoggingContext)
            {
                CopyMembers((WebLoggingContext)LoggingContext.Current);
            }
            else
            {
                InitializeMembers(new StreamingContext());
            }
        }


        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
        }

        private void CopyMembers(WebLoggingContext outerContext)
        {
        }

        public override void UpdateEvent(Event e)
        {
            base.UpdateEvent(e);

            e.Source |= EventSource.WebUI;

            string request = null;
            string taskname = null;
            string client = null;

            var context = System.Web.HttpContext.Current;

            if (context != null)
            {
                var req = context.Request;

                if (e.Request == null)
                {
                    request = req.HttpMethod + " " + req.Url.AbsolutePath;
                }

                if (e.TaskName == null)
                {
                    taskname = req.QueryString["taskname"];
                }

                client = req.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (client == null)
                {
                    client = req.UserHostAddress;
                }
            }

            if (request != null) e.Request = request;
            if (taskname != null) e.TaskName = taskname;
            if (client != null) e.Client = client;
        }
    }
}
