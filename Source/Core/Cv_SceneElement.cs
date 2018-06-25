using System.Collections.Generic;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core
{
    public class Cv_SceneElement : Cv_ScreenElement
    {
		public Cv_CameraNode Camera
		{
			get; set;
		}

		internal Cv_Renderer Renderer;

		private Cv_SceneNode m_Root;
		private List<Cv_Transform> m_TransformStack;
		private Dictionary<Cv_EntityID, Cv_SceneNode> m_EntitiesMap;
        private Cv_Transform m_Transform;

        public Cv_SceneElement(Cv_Renderer renderer)
        {
            Renderer = renderer;
            m_EntitiesMap = new Dictionary<Cv_EntityID, Cv_SceneNode>();
            m_TransformStack = new List<Cv_Transform>();
			m_Root = new Cv_SceneNode(Cv_EntityID.INVALID_ENTITY, null, new Cv_Transform());

			//Cv_EventManager.Instance.AddListener<Cv_Event_NewRenderComponent>(OnNewRenderComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
			Cv_EventManager.Instance.AddListener<Cv_Event_TransformEntity>(OnMoveEntity);
			//Cv_EventManager.Instance.AddListener<Cv_Event_ModifiedRenderComponent>(OnModifiedRenderComponent);
        }

		~Cv_SceneElement()
		{
			//Cv_EventManager.Instance.RemoveListener<Cv_Event_NewRenderComponent>(OnNewRenderComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_TransformEntity>(OnMoveEntity);
			//Cv_EventManager.Instance.RemoveListener<Cv_Event_ModifiedRenderComponent>(OnModifiedRenderComponent);
		}

        public override void VOnRender(float time, float timeElapsed)
        {
            var res = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>("profile.png");
            var tex = res.GetTexture();
            
            Renderer.Draw(tex.Texture, new Rectangle(-250, -250, 500, 500), Color.White);

			if (m_Root != null && Camera != null)
			{
				m_Root.VPreRender(this);
				m_Root.VRender(this);
				m_Root.VRenderChildren(this);
				m_Root.VPostRender(this);
			}
        }

        public override void VOnUpdate(float time, float timeElapsed)
        {
			m_Root.VOnUpdate(time, timeElapsed, this);
        }

		public Cv_SceneNode GetEntityNode(Cv_EntityID entityID)
		{
			Cv_SceneNode node = null;
			m_EntitiesMap.TryGetValue(entityID, out node);

			return node;
		}

		public bool AddNode(Cv_EntityID entityID, Cv_SceneNode node)
		{
			if (entityID != Cv_EntityID.INVALID_ENTITY)
			{
				m_EntitiesMap.Add(entityID, node);
			}

			return m_Root.AddChild(node);
		}

		public bool RemoveNode(Cv_EntityID entityID)
		{
			if (entityID == Cv_EntityID.INVALID_ENTITY)
			{
				return false;
			}

			m_EntitiesMap.Remove(entityID);

			return m_Root.RemoveChild(entityID);
		}

		public void OnNewRenderComponent(Cv_Event eventData)
		{

		}

		public void OnModifiedRenderComponent(Cv_Event eventData)
		{

		}

		public void OnDestroyEntity(Cv_Event eventData)
		{
			Cv_Event_DestroyEntity destroyEntity = (Cv_Event_DestroyEntity) eventData;
			RemoveNode(destroyEntity.EntityID);
		}

		public void OnMoveEntity(Cv_Event eventData)
		{
			Cv_Event_TransformEntity transformEntity = (Cv_Event_TransformEntity) eventData;
			
			Cv_SceneNode node;
			if (m_EntitiesMap.TryGetValue(transformEntity.EntityID, out node))
			{
				node.Transform = transformEntity.Transform;
			}
		}

		public void PushAndSetTransform(Cv_Transform toWorld)
		{
			Cv_Transform currTransform = null;

			if (m_TransformStack.Count > 0)
			{
				currTransform = new Cv_Transform();
			}
			else
			{
				currTransform = m_TransformStack[m_TransformStack.Count-1];
			}
			
			m_TransformStack.Add(Cv_Transform.Multiply(currTransform, toWorld));
		}

		public void PopTransform() 
		{
			m_TransformStack.RemoveAt(m_TransformStack.Count-1);
			var transf = m_TransformStack[m_TransformStack.Count-1];
		}

		bool Pick(Vector2 screenPosition) {
            return m_Root.VPick(this, screenPosition);
        }
    }
}