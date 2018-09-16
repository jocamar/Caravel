using Caravel.Debugging;

namespace Caravel.Core.Process
{
    public abstract class Cv_Process
    {
        public enum Cv_ProcessState
        {
            Uninitialized = 0,  // created but not running
            Removed,  // removed from the process list but not destroyed; this can happen when a process that is already running is parented to another process
            Running,  // initialized and running
            Paused,  // initialized but paused
            Succeeded,  // completed successfully
            Failed,  // failed to complete
            Aborted,  // aborted; may not have started
        };
	
	    public Cv_ProcessState State
        {
            get; internal set;
        }

        public bool IsAlive
        {
            get
            {
                return (State == Cv_ProcessState.Running || State == Cv_ProcessState.Paused);
            }
        }

        public bool IsDead
        {
            get
            {
                return (State == Cv_ProcessState.Succeeded || State == Cv_ProcessState.Failed ||  State == Cv_ProcessState.Aborted);
            }
        }

        public bool IsRemoved
        {
            get
            {
                return State == Cv_ProcessState.Removed;
            }
        }

        public bool IsPaused
        {
            get
            {
                return State == Cv_ProcessState.Paused;
            }
        }

	    public Cv_Process Child
        {
            get; private set;
        }

        public Cv_Process()
        {
            State = Cv_ProcessState.Uninitialized;
            Child = null;
        }

        ~Cv_Process()
        {
            if (Child != null)
            {
                Child.VOnAbort();
            }
        }

        public void Succeed()
        {
            Cv_Debug.Assert(State == Cv_ProcessState.Running || State == Cv_ProcessState.Paused, "Only running processes can succeed.");
	        State = Cv_ProcessState.Succeeded;
        }

        public void Fail()
        {
            Cv_Debug.Assert(State == Cv_ProcessState.Running || State == Cv_ProcessState.Paused, "Only running processes can fail.");
	        State = Cv_ProcessState.Failed;
        }
        
        public void Pause()
        {
            if (State == Cv_ProcessState.Running)
            {
                State = Cv_ProcessState.Paused;
            }
            else
            {
                Cv_Debug.Warning("Attempting to pause a process that isn't running");
            }
        }

        public void Resume()
        {
            if (State == Cv_ProcessState.Paused)
            {
                State = Cv_ProcessState.Running;
            }
            else
            {
                Cv_Debug.Warning("Attempting to resume a process that isn't paused");
            }
        }

        public void AttachChild(Cv_Process child)
        {
            if (Child != null)
            {
                Child.AttachChild(child);
            }
            else
            {
                Child = child;
            }
        }

        public Cv_Process RemoveChild()
        {
            if (Child != null)
            {
                Cv_Process child = Child;
                Child = null;
                return child;
            }

            return null;
        }

	    protected internal abstract void VOnUpdate(float elapsedTime);
	    protected internal abstract void VOnSuccess();
	    protected internal abstract void VOnFail();
	    protected internal abstract void VOnAbort();

        protected internal virtual void VOnInit()
        {
             State = Cv_ProcessState.Running;
        }
    }
}