﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using Jhu.Graywulf.Logging;
using Jhu.Graywulf.Tasks;


namespace Jhu.Graywulf.Activities
{
    public abstract class JobAsyncCodeActivity : AsyncCodeActivity, IJobActivity
    {
        #region Properties

        [RequiredArgument]
        public InArgument<JobInfo> JobInfo { get; set; }

        #endregion

        protected sealed override IAsyncResult BeginExecute(AsyncCodeActivityContext activityContext, AsyncCallback callback, object state)
        {
            var activityState = new JobAsyncCodeActivityState();
            activityContext.UserState = activityState;

            var task = ExecuteAsync(activityContext);
            var tcs = new TaskCompletionSource<object>(state);

            task.ContinueWith(t =>
            {
                bool res;

                if (t.IsFaulted)
                {
                    res = tcs.TrySetException(t.Exception.InnerExceptions);
                }
                else if (t.IsCanceled)
                {
                    res = tcs.TrySetCanceled();
                }
                else
                {
                    res = tcs.TrySetResult(null);
                }

                if (!res)
                {
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#endif

                    throw new InvalidOperationException();
                }

                callback?.Invoke(tcs.Task);
            });

            return tcs.Task;
        }

        protected sealed override void EndExecute(AsyncCodeActivityContext activityContext, IAsyncResult result)
        {
            var activityState = (JobAsyncCodeActivityState)activityContext.UserState;
            var task = (Task)result;

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                var helper = new Util.CancellationHelper(ex);

                if (activityContext.IsCancellationRequested && helper.IsCancelled)
                {
                    activityContext.MarkCanceled();
                }
                else
                {
                    // TODO: this sometimes throws an exception, even though the activity
                    // is being canceled
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(helper.DispatchException()).Throw();
                }
            }
            //finally
            //{
            // TODO: for some reason, in some situations, this here gets
            // called before the task above completes which leads to a
            // NullException

            /*lock (activityState.SyncRoot)
            {
                activityState.Dispose();
            }*/
            //}

            using (var loggingContext = new LoggingContext(activityState.EventQueue))
            {
                loggingContext.FlushEvents(activityContext);
            }
        }

        protected override void Cancel(AsyncCodeActivityContext activityContext)
        {
            // Any exceptions thrown from this method are fatal to the workflow instance. 
            // This call can happen on a thread concurrent to OnExecuteAsync
            var state = (JobAsyncCodeActivityState)activityContext.UserState;
            var jobContext = new JobContext(this, activityContext);

            using (new LoggingContext(state.EventQueue))
            {
                // Context becomes invalid once the activity has completed but cancel
                // can be called after ExecuteAsync
                if (state.CancellationContext.IsValid && !state.CancellationContext.IsRequested)
                {
                    lock (state.SyncRoot)
                    {
                        state.CancellationContext.Cancel();
                    }
                }
            }
        }

        private async Task ExecuteAsync(AsyncCodeActivityContext activityContext)
        {
            var state = (JobAsyncCodeActivityState)activityContext.UserState;
            var jobContext = new JobContext(this, activityContext);

            using (var loggingContext = new LoggingContext(state.EventQueue))
            {
                jobContext.UpdateLoggingContext(loggingContext);
                jobContext.Push();

                await OnExecuteAsync(activityContext, state.CancellationContext);

                jobContext.Pop();
            }
        }

        protected abstract Task OnExecuteAsync(AsyncCodeActivityContext activityContext, CancellationContext cancellationContext);


    }
}
