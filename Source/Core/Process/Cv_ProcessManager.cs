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
                return m_ProcessLists[m_iCurrentProcessList].Count;
            }
        }

        private readonly int NUM_LISTS = 2;
        private List<Cv_Process>[] m_ProcessLists;
        private int m_iCurrentProcessList = 0;

        public void AttachProcess(Cv_Process process)
        {
            lock (m_ProcessLists[m_iCurrentProcessList])
            {
                m_ProcessLists[m_iCurrentProcessList].Add(process);
            }
        }

        public void AbortAllProcesses(bool immediate)
        {
            foreach (var list in m_ProcessLists)
            {
                lock (list)
                {
                    for (var i = 0; i < list.Count;)
                    {
                        var process = list[i];
                        if (process.IsAlive)
                        {
                            process.State = Cv_ProcessState.Aborted;
                            if (immediate)
                            {
                                process.VOnAbort();
                                list.Remove(process);
                                continue;
                            }
                        }

                        i++;
                    }
                }
            }
        }

        internal Cv_ProcessManager()
        {
            m_ProcessLists = new List<Cv_Process>[NUM_LISTS];

            for (var i = 0; i < NUM_LISTS; i++)
            {
                m_ProcessLists[i] = new List<Cv_Process>();
            }

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

            var currUpdatingList = m_iCurrentProcessList;
            m_iCurrentProcessList = ++m_iCurrentProcessList % NUM_LISTS;

            lock (m_ProcessLists[currUpdatingList])
            {
                for (var i = 0; i < m_ProcessLists[currUpdatingList].Count;)
                {
                    Cv_Process currProcess = m_ProcessLists[currUpdatingList][i];

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

                        m_ProcessLists[currUpdatingList].Remove(currProcess);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return new Tuple<int,int>(successCount, failCount);
        }
    }
}
