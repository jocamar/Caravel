using System;
using System.Collections.Generic;
using static Caravel.Core.Process.Cv_Process;

namespace Caravel.Core.Process
{
    public class Cv_ProcessManager
    {
        public static Cv_ProcessManager Instance
        {
            get; private set;
        }

        public int ProcessCount
        { 
            get
            {
                return m_ProcessList.Count;
            }
        }

        private List<Cv_Process> m_ProcessList;

        public void AttachProcess(Cv_Process process)
        {
            m_ProcessList.Add(process);
        }

        public void AbortAllProcesses(bool immediate)
        {
            for (var i = 0; i < m_ProcessList.Count;)
            {
                var process = m_ProcessList[i];
                if (process.IsAlive)
                {
                    process.State = Cv_ProcessState.Aborted;
                    if (immediate)
                    {
                        process.VOnAbort();
                        m_ProcessList.Remove(process);
                        continue;
                    }
                }

                i++;
            }
        }

        internal Cv_ProcessManager()
        {
            m_ProcessList = new List<Cv_Process>();
            Instance = this;
        }

        internal bool Initialize()
        {
            return true;
        }

        internal Tuple<int, int> OnUpdate(float time, float elapsedTime)
        {
            int successCount = 0;
            int failCount = 0;

            for (var i = 0; i < m_ProcessList.Count;)
            {
                Cv_Process currProcess = m_ProcessList[i];

                if (currProcess.State == Cv_ProcessState.Uninitialized)
                {
                    currProcess.VOnInit();
                }

                if (currProcess.State == Cv_ProcessState.Running)
                {
                    currProcess.VOnUpdate(elapsedTime);
                }

                if (currProcess.IsDead)
                {
                    switch (currProcess.State)
                    {
                        case Cv_ProcessState.Succeeded:
                            currProcess.VOnSuccess();
                            Cv_Process child = currProcess.RemoveChild();
                            if (child != null)
                            {
                                AttachProcess(child);
                            }
                            else
                            {
                                ++successCount;  // only counts if the whole chain completed
                            }
                            break;
                        case Cv_ProcessState.Failed:
                            currProcess.VOnFail();
                            ++failCount;
                            break;
                        case Cv_ProcessState.Aborted:
                            currProcess.VOnAbort();
                            ++failCount;
                            break;
                    }

                    m_ProcessList.Remove(currProcess);
                }
				else
				{
					i++;
				}
            }

            return new Tuple<int,int>(successCount, failCount);
        }
    }
}
