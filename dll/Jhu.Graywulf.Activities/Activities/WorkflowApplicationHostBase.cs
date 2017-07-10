﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Activities;
using System.Threading;

namespace Jhu.Graywulf.Activities
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class is used by command-line executor
    /// </remarks>
    public abstract class WorkflowApplicationHostBase : MarshalByRefObject
    {
        public class WorkflowApplicationDetails
        {
            public WorkflowApplication WorkflowApplication;
            public Exception LastException;
        }

        #region Private variables

        private bool stopRequested;

        /// <summary>
        /// Holds the workflows hosted in the app domain
        /// </summary>
        protected ConcurrentDictionary<Guid, WorkflowApplicationDetails> workflows;

        /// <summary>
        /// Logging participant, same for all WorkflowApplications
        /// </summary>
        private GraywulfTrackingParticipant trackingParticipant;

        #endregion
        #region Events

        /// <summary>
        /// Reports workflow events to the main scheduler class
        /// </summary>
        public event EventHandler<WorkflowApplicationHostEventArgs> WorkflowEvent;

        #endregion
        #region Constructors and initializers

        public WorkflowApplicationHostBase()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.stopRequested = false;
            this.workflows = new ConcurrentDictionary<Guid, WorkflowApplicationDetails>();
            this.trackingParticipant = null;
        }

        public override object InitializeLifetimeService()
        {
            // Prevent remoting timeouts
            return null;
        }

        #endregion

        protected void EnsureNotStopping()
        {
            if (stopRequested)
            {
                throw new InvalidOperationException();
            }
        }

        public virtual void Start(bool iteractive)
        {
            EnsureNotStopping();

            // Initialize logging
            Logging.Logger.Instance.Start(iteractive);
            trackingParticipant = new GraywulfTrackingParticipant();
        }

        public virtual void Stop(TimeSpan timeout)
        {
            EnsureNotStopping();
            stopRequested = true;

            // Wait until all workflows complete
            while (!workflows.IsEmpty)
            {
                Thread.Sleep(100);  // TODO: use constant
            }

            trackingParticipant = null;
            stopRequested = false;

            Logging.Logger.Instance.Stop();
        }

        /// <summary>
        /// Initializes a WorkflowApplication for the job
        /// </summary>
        /// <param name="wftype"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        protected virtual WorkflowApplication CreateWorkflowApplication(Type wftype, Dictionary<string, object> par)
        {
            // Instantiate workflow
            Activity wf = (Activity)Activator.CreateInstance(wftype);
            return CreateWorkflowApplication(wf, par);
        }

        protected virtual WorkflowApplication CreateWorkflowApplication(Activity wf, Dictionary<string, object> par)
        {
            WorkflowApplication wfapp =
                par == null ? new WorkflowApplication(wf) : new WorkflowApplication(wf, par);

            // Add necessary participants
            wfapp.Extensions.Add(trackingParticipant);

            // Wire-up workflow runtime events
            wfapp.OnUnhandledException = WorkflowApplication_OnUnhandledException;
            wfapp.Completed = WorkflowApplication_WorkflowCompleted;
            wfapp.Unloaded = WorkflowApplication_WorkflowUnloaded;
            wfapp.Aborted = WorkflowApplication_WorkflowAborted;
            wfapp.Idle = WorkflowApplication_WorkflowIdle;
            wfapp.PersistableIdle = WorkflowApplication_PersistableIdle;

            return wfapp;
        }

        protected void RegisterWorkflow(WorkflowApplication wfapp, WorkflowApplicationDetails workflow)
        {
            workflows.TryAdd(wfapp.Id, workflow);
        }

        public Guid PrepareStartWorkflow(Activity wf, Dictionary<string, object> par)
        {
            var wfapp = CreateWorkflowApplication(wf, par);

            var workflow = new WorkflowApplicationDetails()
            {
                WorkflowApplication = wfapp,
            };

            RegisterWorkflow(wfapp, workflow);

            return wfapp.Id;
        }

        public void RunWorkflow(Guid instanceId)
        {
            EnsureNotStopping();

            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(instanceId, out workflow))
            {
                workflow.WorkflowApplication.Run();
            };
        }

        protected Guid CancelWorkflow(Guid instanceId, TimeSpan timeout)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(instanceId, out workflow))
            {
                OnWorkflowCancelling(workflow);
                workflow.WorkflowApplication.Cancel(timeout);
            }

            return instanceId;
        }

        protected virtual void OnWorkflowCancelling(WorkflowApplicationDetails workflow)
        {
        }

        protected Guid TimeOutWorkflow(Guid instanceId, TimeSpan timeout)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(instanceId, out workflow))
            {
                OnWorkflowTimingOut(workflow);

                // Cancel workflow explicitly, this will cause all
                // long async operations to cancel
                workflow.WorkflowApplication.Cancel(timeout);
            }

            return instanceId;
        }

        protected virtual void OnWorkflowTimingOut(WorkflowApplicationDetails workflow)
        {
        }

        protected Guid PersistWorkflow(Guid instanceId)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(instanceId, out workflow))
            {
                OnWorkflowPersisting(workflow);
            }

            return instanceId;
        }

        protected virtual void OnWorkflowPersisting(WorkflowApplicationDetails workflow)
        {
        }

        /// <summary>
        /// Do bookkeeping required when a workflow finishes
        /// </summary>
        /// <param name="instanceId"></param>
        protected Guid FinishWorkflow(Guid instanceId)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(instanceId, out workflow))
            {
                OnWorkflowFinishing(workflow);
                workflows.TryRemove(instanceId, out workflow);
            }

            return instanceId;
        }

        protected virtual void OnWorkflowFinishing(WorkflowApplicationDetails workflow)
        {
        }

        #region Workflow host events

        protected virtual void OnWorkflowEvent(WorkflowApplicationHostEventArgs e)
        {
            WorkflowEvent(this, e);
        }

        private UnhandledExceptionAction WorkflowApplication_OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {

#if BREAKDEBUG
            if (System.Diagnostics.Debugger != null)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            // This is important to gracefully unload faulted or canceled/timed-out.
            // It causes the runtime to cancel the workflow instead of abort it instantenously.
            // This will cause the cancel logic and finally activities of a try-catch-finally activity to
            // execute prior to the unloading of the workflow.

            // First we have to store the exception because it won't be available after the
            // workflow gracefully cancels
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(e.InstanceId, out workflow))
            {
                OnWorkflowUnHandledException(e, workflow);
            }

            // Force the workflow to execute the cancel logic
            return UnhandledExceptionAction.Cancel;
        }

        protected virtual void OnWorkflowUnHandledException(WorkflowApplicationUnhandledExceptionEventArgs e, WorkflowApplicationDetails workflow)
        {
            workflow.LastException = e.UnhandledException;
        }

        /// <summary>
        /// Executes when a workflow completed either successfully, either failing.
        /// </summary>
        /// <param name="e"></param>
        private void WorkflowApplication_WorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {
            WorkflowApplicationDetails workflow;
            if (workflows.TryGetValue(e.InstanceId, out workflow))
            {
                if (WorkflowEvent != null)
                {
                    OnWorkflowCompleted(e, workflow);
                }
            }
        }

        protected virtual void OnWorkflowCompleted(WorkflowApplicationCompletedEventArgs e, WorkflowApplicationDetails workflow)
        {
            switch (e.CompletionState)
            {
                case ActivityInstanceState.Closed:
                    // Completed successfully
                    OnWorkflowEvent(new WorkflowApplicationHostEventArgs(WorkflowEventType.Completed, e.InstanceId));
                    break;
                case ActivityInstanceState.Canceled:
                    if (workflow.LastException == null)
                    {
                        OnWorkflowEvent(new WorkflowApplicationHostEventArgs(WorkflowEventType.Cancelled, e.InstanceId));
                    }
                    else
                    {
                        OnWorkflowEvent(new WorkflowApplicationHostEventArgs(WorkflowEventType.Failed, e.InstanceId, workflow.LastException.Message));
                    }
                    break;
                case ActivityInstanceState.Faulted:
                // This should not happen, workflows are forced to cancel
                case ActivityInstanceState.Executing:
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual void WorkflowApplication_WorkflowUnloaded(WorkflowApplicationEventArgs e)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(e.InstanceId, out workflow))
            {
                OnWorkflowUnloaded(e, workflow);
                FinishWorkflow(e.InstanceId);
            }
        }

        protected virtual void OnWorkflowUnloaded(WorkflowApplicationEventArgs e, WorkflowApplicationDetails workflow)
        {
            // Nothing to do here
        }

        protected virtual void WorkflowApplication_WorkflowAborted(WorkflowApplicationEventArgs e)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(e.InstanceId, out workflow))
            {
                OnWorkflowAborted(e, workflow);
                FinishWorkflow(e.InstanceId);
            }
        }

        protected virtual void OnWorkflowAborted(WorkflowApplicationEventArgs e, WorkflowApplicationDetails workflow)
        {
            // Workflows are aborted when an exception is thrown during cancellation
            if (workflow.LastException != null)
            {
                WorkflowEvent(
                    this,
                    new WorkflowApplicationHostEventArgs(
                        WorkflowEventType.Failed,
                        e.InstanceId,
                        GetExceptionMessage(workflow.LastException)));
            }
            else
            {
                WorkflowEvent(this, new WorkflowApplicationHostEventArgs(WorkflowEventType.Failed, e.InstanceId));
            }
        }

        protected virtual void WorkflowApplication_WorkflowIdle(WorkflowApplicationIdleEventArgs e)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(e.InstanceId, out workflow))
            {
                OnWorkflowIdle(e, workflow);
            }
        }

        protected virtual void OnWorkflowIdle(WorkflowApplicationEventArgs e, WorkflowApplicationDetails workflow)
        {
        }

        protected virtual PersistableIdleAction WorkflowApplication_PersistableIdle(WorkflowApplicationIdleEventArgs e)
        {
            WorkflowApplicationDetails workflow;

            if (workflows.TryGetValue(e.InstanceId, out workflow))
            {
                return OnWorkflowPersistableIdle(e, workflow);
            }
            else
            {
                // TODO: this shouldn't happen
                return PersistableIdleAction.None;
            }
        }

        protected virtual PersistableIdleAction OnWorkflowPersistableIdle(WorkflowApplicationIdleEventArgs e, WorkflowApplicationDetails workflow)
        {
            return PersistableIdleAction.Persist;
        }

        protected string GetExceptionMessage(Exception exception)
        {
            if (exception is AggregateException)
            {
                return exception.InnerException.Message;
            }
            else
            {
                return exception.Message;
            }
        }

        #endregion
    }
}
