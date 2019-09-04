using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caravel.Core.Draw;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Entity
{
    public class Cv_ClickableComponent : Cv_EntityComponent
    {
        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

		public Action OnUnclick
		{
			get; set;
		}

        public string OnUnclickScript
        {
            get; set;
        }

		public Action OnClick
		{
			get; set;
		}

        public string OnClickScript
        {
            get; set;
        }

        public Vector2 AnchorPoint
        {
            get; set;
        }

        public bool Active
        {
            get; set;
        }

        public Cv_ClickAreaNode ClickAreaNode
        {
            get
            {
                if (m_AreaNode == null)
                {
                    m_AreaNode = new Cv_ClickAreaNode(Owner.ID, this);
                }

                return m_AreaNode;
            }
            
            protected set
            {
                m_AreaNode = value;
            }
        }


        private Cv_ClickAreaNode m_AreaNode;
        private bool m_bWasClicking = false;
        private bool m_bWasInArea = false;

        public override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_ClickableComponent>());
            var unclickScript = componentDoc.CreateElement("OnUnclick");
            var clickScript = componentDoc.CreateElement("OnClick");
            var size = componentDoc.CreateElement("Size");
            var anchor = componentDoc.CreateElement("Anchor");
            var active = componentDoc.CreateElement("Active");

            unclickScript.SetAttribute("resource", OnUnclickScript);
            clickScript.SetAttribute("resource", OnClickScript);
            size.SetAttribute("width", Width.ToString(CultureInfo.InvariantCulture));
            size.SetAttribute("height", Height.ToString(CultureInfo.InvariantCulture));
            anchor.SetAttribute("x", ((int) AnchorPoint.X).ToString(CultureInfo.InvariantCulture));
            anchor.SetAttribute("y", ((int) AnchorPoint.Y).ToString(CultureInfo.InvariantCulture));
            active.SetAttribute("status", Active.ToString(CultureInfo.InvariantCulture));

            componentData.AppendChild(unclickScript);
            componentData.AppendChild(clickScript);
            componentData.AppendChild(size);
            componentData.AppendChild(anchor);
            componentData.AppendChild(active);
            
            return componentData;
        }

        public override bool VInitialize(XmlElement componentData)
        {
            var onMouseUpNode = componentData.SelectNodes("OnUnclick").Item(0);
            if (onMouseUpNode != null)
            {
                OnUnclickScript = onMouseUpNode.Attributes["resource"].Value;
            }

            var mouseDownScript = componentData.SelectNodes("OnClick").Item(0);
            if (mouseDownScript != null)
            {
                OnClickScript = mouseDownScript.Attributes["resource"].Value;
            }

            var sizeNode = componentData.SelectNodes("Size").Item(0);
            if (sizeNode != null)
            {
                Width = int.Parse(sizeNode.Attributes["width"].Value);
                Height = int.Parse(sizeNode.Attributes["height"].Value);
            }

            var anchorNode = componentData.SelectNodes("Anchor").Item(0);
            if (anchorNode != null)
            {
                var x = int.Parse(anchorNode.Attributes["x"].Value);
                var y = int.Parse(anchorNode.Attributes["y"].Value);
                AnchorPoint = new Vector2(x, y);
            }

            var activeNode = componentData.SelectNodes("Active").Item(0);
            if (activeNode != null)
            {
                Active = bool.Parse(activeNode.Attributes["status"].Value);
            }

            return true;
        }

        public override void VOnChanged()
        {
        }

        public override void VOnDestroy()
        {
            Cv_ClickAreaNode clickAreaNode = this.ClickAreaNode;
            Cv_Event newEvent = new Cv_Event_DestroyClickableComponent(Owner.ID, clickAreaNode, this);
            Cv_EventManager.Instance.QueueEvent(newEvent, true);
        }

        
        public override bool VPostInitialize()
        {
            Cv_ClickAreaNode clickAreaNode = this.ClickAreaNode;
            Cv_Event newEvent = new Cv_Event_NewClickableComponent(Owner.ID, Owner.Parent, clickAreaNode, Width, Height, AnchorPoint, Active, this);
            Cv_EventManager.Instance.TriggerEvent(newEvent);
            return true;
        }

        public override void VPostLoad()
        {
        }

        public void Click(Vector2 pointer)
        {
            var playerViews = CaravelApp.Instance.Logic.GameViews.Where(gv => gv.Type == Cv_GameView.Cv_GameViewType.Player);

            if (!m_bWasClicking)
            {
                foreach (var view in playerViews)
                {
                    Cv_EntityID[] entities;
                    var playerView = view as Cv_PlayerView;
                    if (playerView.Pick<Cv_ClickAreaNode>(pointer, out entities) && entities.Contains(Owner.ID))
                    {
                        if (OnClickScript != null && OnClickScript != "")
                        {
                            var scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(OnClickScript, Owner.ResourceBundle);
                            scriptRes.RunScript();
                        }

                        if (OnClick != null)
                        {
                            OnClick();
                        }

                        m_bWasInArea = true;
                    }

                    m_bWasClicking = true;
                }
            }
        }

        public void Unclick(Vector2 pointer)
        {
            if (m_bWasClicking)
            {
                if (m_bWasInArea)
                {
					if (OnUnclickScript != null && OnUnclickScript != "")
					{
						var scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(OnUnclickScript, Owner.ResourceBundle);
						scriptRes.RunScript();
					}

					if (OnUnclick != null)
					{
						OnUnclick();
					}
                }

                m_bWasInArea = false;
                m_bWasClicking = false;
            }
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            if (!Active)
            {
                return;
            }

            var mouseState = Mouse.GetState();
            var playerViews = CaravelApp.Instance.Logic.GameViews.Where(gv => gv.Type == Cv_GameView.Cv_GameViewType.Player);
            var mousePos = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            Cv_EntityID[] entities;

            if (mouseState.LeftButton == ButtonState.Pressed && !m_bWasClicking)
            {
                foreach (var view in playerViews)
                {
                    var playerView = view as Cv_PlayerView;
                    if (playerView.Pick<Cv_ClickAreaNode>(mousePos, out entities) && entities.Contains(Owner.ID))
                    {
                        if (OnClickScript != null && OnClickScript != "")
                        {
                            var scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(OnClickScript, Owner.ResourceBundle);
                            scriptRes.RunScript();
                        }

                        if (OnClick != null)
                        {
                            OnClick();
                        }

                        m_bWasInArea = true;
                    }

                    m_bWasClicking = true;
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && m_bWasClicking)
            {
                if (m_bWasInArea)
                {
					if (OnUnclickScript != null && OnUnclickScript != "")
					{
						var scriptRes = Cv_ResourceManager.Instance.GetResource<Cv_ScriptResource>(OnUnclickScript, Owner.ResourceBundle);
						scriptRes.RunScript();
					}

					if (OnUnclick != null)
					{
						OnUnclick();
					}
                }

                m_bWasInArea = false;
                m_bWasClicking = false;
            }
        }
    }
}