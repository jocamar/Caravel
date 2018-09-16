using System.Globalization;
using System.Xml;
using Caravel.Core.Process;
using Caravel.Core.Resource;

namespace Caravel.Core.Entity
{
    public class Cv_ScriptComponent : Cv_EntityComponent
    {
        public string InitScriptResource
        {
            get; set;
        }

        public string ScriptResource
        {
            get; set;
        }

        public float Interval
        {
            get; set;
        }

        public bool ExecuteOnce
        {
            get; set;
        }

        public bool PauseExecution
        {
            get; set;
        }

        private bool m_bRanOnce = false;
        private Cv_TimerProcess m_Timer;

        public override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_ScriptComponent>());
            var initScript = componentDoc.CreateElement("InitScript");
            var script = componentDoc.CreateElement("Script");
            var interval = componentDoc.CreateElement("Interval");
            var executeOnce = componentDoc.CreateElement("ExecuteOnce");
            var paused = componentDoc.CreateElement("Paused");

            initScript.SetAttribute("resource", InitScriptResource);
            script.SetAttribute("resource", ScriptResource);
            interval.SetAttribute("value", Interval.ToString(CultureInfo.InvariantCulture));
            executeOnce.SetAttribute("value", ExecuteOnce.ToString(CultureInfo.InvariantCulture));
            paused.SetAttribute("value", PauseExecution.ToString(CultureInfo.InvariantCulture));

            componentData.AppendChild(initScript);
            componentData.AppendChild(script);
            componentData.AppendChild(interval);
            componentData.AppendChild(executeOnce);
            componentData.AppendChild(paused);
            
            return componentData;
        }

        public Cv_ScriptComponent()
        {
            ExecuteOnce = true;
            PauseExecution = true;
        }

        protected internal override bool VInitialize(XmlElement componentData)
        {
            var initScriptNode = componentData.SelectNodes("InitScript").Item(0);
            if (initScriptNode != null)
            {
                InitScriptResource = initScriptNode.Attributes["resource"].Value;
            }

            var scriptNode = componentData.SelectNodes("Script").Item(0);
            if (scriptNode != null)
            {
                ScriptResource = scriptNode.Attributes["resource"].Value;
            }

            var intervalNode = componentData.SelectNodes("Interval").Item(0);
            if (intervalNode != null)
            {
                Interval = float.Parse(intervalNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var executeOnceNode = componentData.SelectNodes("ExecuteOnce").Item(0);
            if (executeOnceNode != null)
            {
                ExecuteOnce = bool.Parse(executeOnceNode.Attributes["value"].Value);
            }

            var pausedNode = componentData.SelectNodes("Paused").Item(0);
            if (pausedNode != null)
            {
                PauseExecution = bool.Parse(pausedNode.Attributes["value"].Value);
            }

            return true;
        }

        protected internal override void VOnChanged()
        {
        }

        protected internal override void VOnDestroy()
        {
            if (m_Timer != null && m_Timer.IsAlive)
            {
                m_Timer.Fail();
            }
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            if (PauseExecution)
            {
                return;
            }

            if (!m_bRanOnce)
            {
                OnExecuteScriptTimeout();
            }
        }

        protected internal override bool VPostInitialize()
        {
            if (InitScriptResource != null && InitScriptResource != "")
            {
                Cv_ScriptResource scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(InitScriptResource, Owner.ResourceBundle);
                scriptRes.RunScript();
            }

            return true;
        }

        protected internal override void VPostLoad()
        {
        }

        private void OnExecuteScriptTimeout()
        {
            if (ScriptResource != null && ScriptResource != "")
            {
                Cv_ScriptResource scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(ScriptResource, Owner.ResourceBundle);
                scriptRes.RunScript();
            }

            m_bRanOnce = true;

            if (!ExecuteOnce)
            {
                m_Timer = new Cv_TimerProcess(Interval, OnExecuteScriptTimeout);
                Cv_ProcessManager.Instance.AttachProcess(m_Timer);
            }
        }
    }
}