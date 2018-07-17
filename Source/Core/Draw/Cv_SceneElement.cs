using System.Collections.Generic;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Cv_GameLogic;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_SceneElement : Cv_ScreenElement
    {
		public Cv_EntityID EditorSelectedEntity
		{
			get; set;
		}

		public Cv_CameraNode Camera
		{
			get; set;
		}

		public Cv_Transform Transform
		{
			get
			{
				return m_TransformStack[m_TransformStack.Count-1];
			}
		}

		private Cv_SceneNode m_Root;
		private List<Cv_Transform> m_TransformStack;
		private Dictionary<Cv_EntityID, List<Cv_SceneNode>> m_EntitiesMap;

        public Cv_SceneElement()
        {
			EditorSelectedEntity = Cv_EntityID.INVALID_ENTITY;
            m_EntitiesMap = new Dictionary<Cv_EntityID, List<Cv_SceneNode>>();
            m_TransformStack = new List<Cv_Transform>();
			m_Root = new Cv_HolderNode(Cv_EntityID.INVALID_ENTITY);

			Cv_EventManager.Instance.AddListener<Cv_Event_NewRenderComponent>(OnNewRenderComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
			Cv_EventManager.Instance.AddListener<Cv_Event_TransformEntity>(OnMoveEntity);
			Cv_EventManager.Instance.AddListener<Cv_Event_ModifiedRenderComponent>(OnModifiedRenderComponent);
        }

		~Cv_SceneElement()
		{
			Cv_EventManager.Instance.RemoveListener<Cv_Event_NewRenderComponent>(OnNewRenderComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_TransformEntity>(OnMoveEntity);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_ModifiedRenderComponent>(OnModifiedRenderComponent);
		}

        public override void VOnRender(float time, float timeElapsed, Cv_Renderer renderer)
        {
			if (m_Root != null && Camera != null)
			{
				renderer.BeginDraw(Camera);
					m_Root.VPreRender(this, renderer);
					m_Root.VRender(this, renderer);
					m_Root.VRenderChildren(this, renderer);
					m_Root.VPostRender(this, renderer);
				renderer.EndDraw();
			}
        }

		public override void VOnPostRender(Cv_Renderer renderer)
		{
			if (m_Root != null)
			{
				m_Root.VFinishedRender(this, renderer);
			}
		}

        public override void VOnUpdate(float time, float timeElapsed)
        {
			m_Root.VOnUpdate(time, timeElapsed, this);
        }

		public Cv_SceneNode[] GetEntityNodes(Cv_EntityID entityID)
		{
			List<Cv_SceneNode> nodes = null;
			m_EntitiesMap.TryGetValue(entityID, out nodes);

			return nodes.ToArray();
		}

		public bool AddNode(Cv_EntityID entityID, Cv_SceneNode node)
		{
			if (node == null)
			{
				Cv_Debug.Error("Trying to add nonexistant node to scene.");
				return false;
			}

			if (entityID != Cv_EntityID.INVALID_ENTITY)
			{
				bool holderNodeExists = false;
				Cv_SceneNode holderNode = null;
				List<Cv_SceneNode> nodes;
				if (m_EntitiesMap.TryGetValue(entityID, out nodes))
				{
					var siblingNode = nodes.Find(n => n.Parent.Parent.Properties.EntityID != Cv_EntityID.INVALID_ENTITY);
					if (siblingNode != null)
					{
						Cv_Debug.Error("Cannot have two nodes belonging to the same entity with different parents.");
						return false;
					}

					if (nodes.Count > 0)
					{
						siblingNode = nodes[0];
						holderNodeExists = true;
						holderNode = siblingNode.Parent;
					}

					nodes.Add(node);
				}
				else
				{
					nodes = new List<Cv_SceneNode>();
					nodes.Add(node);
					m_EntitiesMap.Add(entityID, nodes);
				}

				if (!holderNodeExists)
				{
					holderNode = new Cv_HolderNode(entityID);
					m_Root.AddChild(holderNode);
				}

				return holderNode.AddChild(node);
			}

			return m_Root.AddChild(node);
		}

		public bool AddNodeAsChild(Cv_EntityID parentEntity, Cv_EntityID entityID, Cv_SceneNode node)
		{
			if (node == null)
			{
				Cv_Debug.Error("Trying to add nonexistant node to scene.");
				return false;
			}

			if (entityID != Cv_EntityID.INVALID_ENTITY)
			{
				if (parentEntity == Cv_EntityID.INVALID_ENTITY)
				{
					return AddNode(entityID, node);
				}
				else if (!m_EntitiesMap.ContainsKey(parentEntity)
							|| m_EntitiesMap[parentEntity].Count <= 0
							|| m_EntitiesMap[parentEntity][0].Parent.Properties.EntityID != parentEntity)
				{
					Cv_Debug.Error("Parent does not exist on the scene graph.");
					return false;
				}
				else
				{
					Cv_SceneNode holderNode = null;
					List<Cv_SceneNode> nodes = null;
					if (m_EntitiesMap.TryGetValue(entityID, out nodes))
					{
						var siblingNode = nodes.Find(n => n.Parent.Parent.Properties.EntityID != parentEntity);
						if (siblingNode != null)
						{
							Cv_Debug.Error("Cannot have two nodes belonging to the same entity with different parents.");
							return false;
						}
						else if (nodes[0].Parent.Properties.EntityID == entityID)
						{
							holderNode = nodes[0].Parent;
						}
						else
						{
							Cv_Debug.Error("Invalid holder node for existing entity nodes.");
							return false;
						}
					}
					else
					{
						holderNode = new Cv_HolderNode(entityID);
						m_EntitiesMap[parentEntity][0].Parent.AddChild(holderNode);

						nodes = new List<Cv_SceneNode>();
						m_EntitiesMap.Add(entityID, nodes);
					}

					nodes.Add(node);
					return holderNode.AddChild(node);
				}
			}
			else
			{
				if (parentEntity != Cv_EntityID.INVALID_ENTITY)
				{
					Cv_Debug.Error("Trying to attach a node without entity to a node with an entity.");
					return false;
				}
				
				return m_Root.AddChild(node);
			}
		}

		public bool RemoveNode(Cv_EntityID entityID)
		{
			if (entityID != Cv_EntityID.INVALID_ENTITY)
			{
				m_EntitiesMap.Remove(entityID);
			}

			return m_Root.RemoveChild(entityID);
		}

		public void OnNewRenderComponent(Cv_Event eventData)
		{
			var castEventData = (Cv_Event_NewRenderComponent) eventData;
			var entityId = castEventData.EntityID;
			var sceneNode = castEventData.SceneNode;
			var parentId = castEventData.ParentID;

			AddNodeAsChild(parentId, entityId, sceneNode);
		}

		public void OnNewCameraComponent(Cv_Event eventData)
		{
			var castEventData = (Cv_Event_NewCameraComponent) eventData;
			var entityId = castEventData.EntityID;
			var cameraNode = castEventData.CameraNode;
			var parentId = castEventData.ParentID;

			AddNodeAsChild(parentId, entityId, cameraNode);
		}

		public void OnModifiedRenderComponent(Cv_Event eventData)
		{
			var castEventData = (Cv_Event_ModifiedRenderComponent) eventData;

			Cv_EntityID entityId = castEventData.EntityID;
			if (entityId == Cv_EntityID.INVALID_ENTITY)
			{
				Cv_Debug.Error("OnModifiedRenderComponent - Unknown entity ID!");
				return;
			}

			if (CaravelApp.Instance.GameLogic.State == Cv_GameState.LoadingGameEnvironment)
			{
				return;
			}

			var sceneNodes = GetEntityNodes(entityId);

			if (sceneNodes == null || sceneNodes.Length <= 0)
			{
				Cv_Debug.Error("Failed to apply changes to scene node for entity " + entityId);
			}
			else
			{
				foreach (var n in sceneNodes)
				{
					if (!n.VOnChanged(this))
					{
						Cv_Debug.Error("Error applying changes to scene node for entity " + entityId);
					}
				}
			}
		}

		public void OnDestroyEntity(Cv_Event eventData)
		{
			Cv_Event_DestroyEntity destroyEntity = (Cv_Event_DestroyEntity) eventData;
			RemoveNode(destroyEntity.EntityID);
		}

		public void OnMoveEntity(Cv_Event eventData)
		{
			List<Cv_SceneNode> nodes = null;
			if (m_EntitiesMap.TryGetValue(eventData.EntityID, out nodes))
			{
				Cv_Event_TransformEntity transformEntity = (Cv_Event_TransformEntity) eventData;
				if (nodes.Count > 0)
				{
					nodes[0].Parent.Position = transformEntity.NewPosition;
					nodes[0].Parent.Scale = transformEntity.NewScale;
					nodes[0].Parent.Origin = transformEntity.NewOrigin;
					nodes[0].Parent.Rotation = transformEntity.NewRotation;
				}
			}
		}

		public void PushAndSetTransform(Cv_Transform toWorld)
		{
			Cv_Transform currTransform = null;

			if (m_TransformStack.Count <= 0)
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
		}

		public bool Pick(Vector2 mousePosition, out Cv_EntityID[] entities, Cv_Renderer renderer) {
			var entityList = new List<Cv_EntityID>();
			var screenPosition = renderer.ScaleMouseToScreenCoordinates(mousePosition);
			var result = m_Root.VPick(this, renderer, screenPosition, entityList);
			entities = entityList.ToArray();
            return result;
        }

		public void PrintTree()
		{
			m_Root.PrintTree(0);
		}
    }
}