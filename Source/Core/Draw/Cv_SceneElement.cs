using System;
using System.Collections.Generic;
using System.Linq;
using Caravel.Core.Entity;
using Caravel.Core.Events;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
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

		public CaravelApp Caravel
		{
			get; private set;
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
		private Dictionary<Cv_EntityID, Cv_HolderNode> m_HolderNodes;

        public Cv_SceneElement(CaravelApp app)
        {
			Caravel = app;
			EditorSelectedEntity = Cv_EntityID.INVALID_ENTITY;
            m_EntitiesMap = new Dictionary<Cv_EntityID, List<Cv_SceneNode>>();
			m_HolderNodes = new Dictionary<Cv_EntityID, Cv_HolderNode>();
            m_TransformStack = new List<Cv_Transform>();
			m_Root = new Cv_HolderNode(Cv_EntityID.INVALID_ENTITY);

			Cv_EventManager.Instance.AddListener<Cv_Event_NewRenderComponent>(OnNewRenderComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_NewClickableComponent>(OnNewClickableComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
			Cv_EventManager.Instance.AddListener<Cv_Event_DestroyCameraComponent>(OnDestroyCameraComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_DestroyClickableComponent>(OnDestroyClickableComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_DestroyRenderComponent>(OnDestroyRenderComponent);
			Cv_EventManager.Instance.AddListener<Cv_Event_TransformEntity>(OnMoveEntity);
			Cv_EventManager.Instance.AddListener<Cv_Event_ModifiedRenderComponent>(OnModifiedRenderComponent);
        }

        ~Cv_SceneElement()
		{
			Cv_EventManager.Instance.RemoveListener<Cv_Event_NewRenderComponent>(OnNewRenderComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_NewCameraComponent>(OnNewCameraComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_NewClickableComponent>(OnNewClickableComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyEntity>(OnDestroyEntity);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyCameraComponent>(OnDestroyCameraComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyClickableComponent>(OnDestroyClickableComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_DestroyRenderComponent>(OnDestroyRenderComponent);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_TransformEntity>(OnMoveEntity);
			Cv_EventManager.Instance.RemoveListener<Cv_Event_ModifiedRenderComponent>(OnModifiedRenderComponent);
		}

        public override void VOnRender(float time, float elapsedTime, Cv_Renderer renderer)
        {
			if (m_Root != null && Camera != null)
			{
				m_TransformStack.Clear();
				renderer.BeginDraw(Camera);
					m_Root.VPreRender(renderer);
					m_Root.VRender(renderer);
					m_Root.VRenderChildren(renderer);
					m_Root.VPostRender(renderer);
				renderer.EndDraw();
			}
        }

		public override void VOnPostRender(Cv_Renderer renderer)
		{
			if (m_Root != null)
			{
				m_Root.VFinishedRender(renderer);
			}
		}

        public override void VOnUpdate(float time, float elapsedTime)
        {
			m_Root.VOnUpdate(time, elapsedTime);
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
				Cv_HolderNode holderNode = null;
				if (m_HolderNodes.TryGetValue(entityID, out holderNode))
				{
					if (holderNode.Parent.Properties.EntityID != Cv_EntityID.INVALID_ENTITY) //holderNode is not direct child of root
					{
						Cv_Debug.Error("Cannot have two nodes belonging to the same entity with different parents.");
						return false;
					}

					m_EntitiesMap[entityID].Add(node);
				}

				if (holderNode == null)
				{
					var nodes = new List<Cv_SceneNode>();
					nodes.Add(node);
					m_EntitiesMap.Add(entityID, nodes);

					holderNode = new Cv_HolderNode(entityID);
					m_Root.AddChild(holderNode);
					m_HolderNodes.Add(entityID, holderNode);
				}

				SetNodeTransform(holderNode);

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
				Cv_HolderNode ancestorNode = null;

				if (parentEntity == Cv_EntityID.INVALID_ENTITY)
				{
					return AddNode(entityID, node);
				}
				else if (!m_HolderNodes.ContainsKey(parentEntity))
				{
					var currEntity = Caravel.Logic.GetEntity(parentEntity);
					var entityStack = new Stack<Cv_Entity>();

                    if (currEntity == null)
                    {
                        return false;
                    }

					//Rebuild path to root
					entityStack.Push(currEntity);
					while (currEntity.Parent != Cv_EntityID.INVALID_ENTITY)
					{
						currEntity = Caravel.Logic.GetEntity(currEntity.Parent);

						if (m_HolderNodes.ContainsKey(currEntity.ID))
						{
							ancestorNode = m_HolderNodes[currEntity.ID];
							break;
						}

						entityStack.Push(currEntity);
					}

					//Add all the nodes starting with the closest to the root
					while (entityStack.Count > 0)
					{
						currEntity = entityStack.Pop();

						var newNodes = new List<Cv_SceneNode>();
						m_EntitiesMap.Add(currEntity.ID, newNodes);

						var hNode = new Cv_HolderNode(currEntity.ID);
						m_HolderNodes.Add(currEntity.ID, hNode);
						
						if (ancestorNode != null)
						{
							ancestorNode.AddChild(hNode);
						}
						else
						{
							m_Root.AddChild(hNode);
						}

						ancestorNode = hNode;

                        SetNodeTransform(hNode);
					}
				}
				else
				{
					ancestorNode = m_HolderNodes[parentEntity];
				}

				if (ancestorNode == null)
				{
					Cv_Debug.Error("Error while trying to find a parent for the new node.");
					return false;
				}

				Cv_HolderNode holderNode = null;
				List<Cv_SceneNode> nodes = null;
				if (m_HolderNodes.TryGetValue(entityID, out holderNode))
				{
					if (holderNode.Parent.Properties.EntityID != parentEntity)
					{
						Cv_Debug.Error("Cannot have two nodes belonging to the same entity with different parents.");
						return false;
					}

                    nodes = m_EntitiesMap[entityID];
                }
				else
				{
					holderNode = new Cv_HolderNode(entityID);
					ancestorNode.AddChild(holderNode);

					SetNodeTransform(holderNode);

					nodes = new List<Cv_SceneNode>();
					m_EntitiesMap.Add(entityID, nodes);
					m_HolderNodes.Add(entityID, holderNode);
				}
                
				nodes.Add(node);
				return holderNode.AddChild(node);
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
				m_HolderNodes.Remove(entityID);
			}

			return m_Root.RemoveChild(entityID);
		}

		public void OnNewRenderComponent(Cv_Event eventData)
		{
			var castEventData = (Cv_Event_NewRenderComponent) eventData;
			var entityId = castEventData.EntityID;
			var sceneNode = castEventData.SceneNode;
			var parentId = castEventData.ParentID;

            if (CaravelApp.Instance.Logic.GetEntity(entityId) == null)
            {
                return;
            }

			AddNodeAsChild(parentId, entityId, sceneNode);
		}

		public void OnNewCameraComponent(Cv_Event eventData)
		{
			var castEventData = (Cv_Event_NewCameraComponent) eventData;
			var entityId = castEventData.EntityID;
			var cameraNode = castEventData.CameraNode;
			var parentId = castEventData.ParentID;

            if (CaravelApp.Instance.Logic.GetEntity(entityId) == null)
            {
                return;
            }

            AddNodeAsChild(parentId, entityId, cameraNode);
		}

		
        private void OnNewClickableComponent(Cv_Event eventData)
        {
            var castEventData = (Cv_Event_NewClickableComponent) eventData;
			var entityId = castEventData.EntityID;
			var clickAreaNode = castEventData.ClickAreaNode;
			var parentId = castEventData.ParentID;

            if (CaravelApp.Instance.Logic.GetEntity(entityId) == null)
            {
                return;
            }

            AddNodeAsChild(parentId, entityId, clickAreaNode);
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

			if (Caravel.Logic.State == Cv_GameState.LoadingScene)
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
					if (!n.VOnChanged())
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

		public void OnDestroyCameraComponent(Cv_Event eventData)
		{
			Cv_Event_DestroyCameraComponent destroyCamera = (Cv_Event_DestroyCameraComponent) eventData;
			RemoveNode(destroyCamera.CameraNode);
		}

		public void OnDestroyClickableComponent(Cv_Event eventData)
		{
			Cv_Event_DestroyClickableComponent destroyCamera = (Cv_Event_DestroyClickableComponent) eventData;
			RemoveNode(destroyCamera.ClickAreaNode);
		}

		public void OnDestroyRenderComponent(Cv_Event eventData)
		{
			Cv_Event_DestroyRenderComponent destroyRender = (Cv_Event_DestroyRenderComponent) eventData;
			RemoveNode(destroyRender.SceneNode);
		}

		public void OnMoveEntity(Cv_Event eventData)
		{
			Cv_HolderNode holderNode = null;
			if (m_HolderNodes.TryGetValue(eventData.EntityID, out holderNode))
			{
				SetNodeTransform(holderNode);
			}
		}

		public void PushAndSetTransform(Cv_Transform toWorld)
		{
			Cv_Transform currTransform = Cv_Transform.Identity;

			if (m_TransformStack.Count > 0)
			{
				currTransform = m_TransformStack[m_TransformStack.Count-1];
			}
			
			m_TransformStack.Add(Cv_Transform.Multiply(currTransform, toWorld));
		}

		public void PopTransform() 
		{
			m_TransformStack.RemoveAt(m_TransformStack.Count-1);
		}

		public void PrintTree()
		{
			m_Root.PrintTree(0);
		}

		internal bool Pick(Vector2 screenPosition, out Cv_EntityID[] entities, Cv_Renderer renderer)
		{
			var entityList = new List<Cv_EntityID>();
			var scaledPosition = renderer.ScaleScreenToViewCoordinates(screenPosition);
			var result = false;
			
			if (scaledPosition.X >= 0 && scaledPosition.X <= renderer.Viewport.Width
					&& scaledPosition.Y >= 0 && scaledPosition.Y <= renderer.Viewport.Height)
			{
				m_TransformStack.Clear();
				result = m_Root.VPick(renderer, scaledPosition, entityList);
			}
			entities = entityList.ToArray();
			entities = entities.OrderBy(e => Caravel.Logic.GetEntity(e).GetComponent<Cv_TransformComponent>() != null ? 1 : 2)
					.ThenByDescending(e => Caravel.Logic.GetEntity(e).GetComponent<Cv_TransformComponent>() != null ?
											Caravel.Logic.GetEntity(e).GetComponent<Cv_TransformComponent>().Position.Z : 0).ToArray();
            return result;
        }

		internal bool Pick<NodeType>(Vector2 screenPosition, out Cv_EntityID[] entities, Cv_Renderer renderer) where NodeType : Cv_SceneNode
		{
			var entityList = new List<Cv_EntityID>();
			var scaledPosition = renderer.ScaleScreenToViewCoordinates(screenPosition);
			var result = false;
			
			if (scaledPosition.X >= 0 && scaledPosition.X <= renderer.Viewport.Width
					&& scaledPosition.Y >= 0 && scaledPosition.Y <= renderer.Viewport.Height)
			{
				m_TransformStack.Clear();
				result = m_Root.Pick<NodeType>(renderer, scaledPosition, entityList);
			}
			entities = entityList.ToArray();
			entities = entities.OrderBy(e => Caravel.Logic.GetEntity(e).GetComponent<Cv_TransformComponent>() != null ? 1 : 2)
					.ThenByDescending(e => Caravel.Logic.GetEntity(e).GetComponent<Cv_TransformComponent>() != null ?
											Caravel.Logic.GetEntity(e).GetComponent<Cv_TransformComponent>().Position.Z : 0).ToArray();
            return result;
        }

		private bool RemoveNode(Cv_SceneNode node)
		{
			if (m_Root.RemoveChild(node))
			{
				m_EntitiesMap[node.Properties.EntityID].Remove(node);
				if (m_EntitiesMap[node.Properties.EntityID].Count <= 0)
				{
					m_EntitiesMap.Remove(node.Properties.EntityID);
					
					if (m_HolderNodes.ContainsKey(node.Properties.EntityID))
					{
						var holderNode = m_HolderNodes[node.Properties.EntityID];
						m_Root.RemoveChild(holderNode);
						m_HolderNodes.Remove(node.Properties.EntityID);
					}
				}

				return true;
			}

			return false;
		}

		private void SetNodeTransform(Cv_SceneNode node)
		{
			if (node.Properties.EntityID == Cv_EntityID.INVALID_ENTITY)
			{
				return;
			}

			var entity = Caravel.Logic.GetEntity(node.Properties.EntityID);

			var transformComponent = entity.GetComponent<Cv_TransformComponent>();
			if (transformComponent != null)
			{
				node.Position = transformComponent.Position;
				node.Scale = transformComponent.Scale;
				node.Origin = transformComponent.Origin;
				node.Rotation = transformComponent.Rotation;
			}
		}
    }
}