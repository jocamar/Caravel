using System;
using Caravel.Core.Scripting;

namespace Caravel.Core.Process
{
    public class Cv_TimerProcess : Cv_Process
    {
        private float m_fTimeoutMillis;
        private float m_fRemainingTime;
        private Action m_OnEnd;
        private string m_sOnEndScript;

        public Cv_TimerProcess(float timeoutMillis, Action onEnd)
        {
            m_fTimeoutMillis = timeoutMillis;
            m_fRemainingTime = m_fTimeoutMillis;
            m_OnEnd = onEnd;
        }

        public Cv_TimerProcess(float timeoutMillis, string onEndScript)
        {
            m_fTimeoutMillis = timeoutMillis;
            m_fRemainingTime = m_fTimeoutMillis;
            m_sOnEndScript = onEndScript;
        }

        protected internal override void VOnAbort()
        {
        }

        protected internal override void VOnFail()
        {
        }

        protected internal override void VOnSuccess()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            m_fRemainingTime -= elapsedTime;

            if (m_fRemainingTime <= 0)
            {
                if (m_sOnEndScript != null)
                {
                    Cv_ScriptManager.Instance.VExecuteString(m_sOnEndScript);
                }
                else
                {
                    m_OnEnd();
                }
                
                Succeed();
            }
        }
    }
}