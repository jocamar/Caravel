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

        private SpriteBatch m_SpriteBatch;
		private Cv_SceneNode m_Root;

		private List<Cv_Transform> m_TransformStack;
		private Dictionary<Cv_EntityID, Cv_SceneNode> m_EntitiesMap;

        public Cv_SceneElement(SpriteBatch sb)
        {
            m_SpriteBatch = sb;
        }

        public override void VOnRender(float time, float timeElapsed)
        {
            var res = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>("profile.png");
            var tex = res.GetTexture();
            
            m_SpriteBatch.Draw(tex.Texture, Vector2.Zero, Color.White);
        }

        public override void VOnUpdate(float time, float timeElapsed)
        {
        }

		public Cv_SceneNode GetEntityNode(Cv_EntityID entityID)
		{
			Cv_SceneNode node = null;
			m_EntitiesMap.TryGetValue(entityID, out node);

			return node;
		}

		public bool AddNode(Cv_EntityID entityID, Cv_SceneNode node)
		{
			m_EntitiesMap.Add(entityID, node);
			//TODO(JM): Add node to root

			return true;
		}

		public bool RemoveNode(Cv_EntityID entityID)
		{
			m_EntitiesMap.Remove(entityID);
			//TODO(JM): Remove node from root

			return true;
		}

		public void OnNewRenderComponent(Cv_Event eventData)
		{

		}

		public void OnModifiedRenderComponent(Cv_Event eventData)
		{

		}

		public void OnDestroyEntity(Cv_Event eventData)
		{

		}

		public void OnMoveEntity(Cv_Event eventData)
		{

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
			//m_Renderer.Transform = m_TransformStack[m_TransformStack.Count-1];
		}

		/*void PopMatrix() 
		{
			// Scene::PopMatrix - Chapter 16, page 541
			m_MatrixStack->Pop(); 
			Mat4x4 mat = GetTopMatrix();
			m_Renderer->VSetWorldTransform(&mat);
		}

		const Mat4x4 GetTopMatrix() 
		{ 
			// Scene::GetTopMatrix - Chapter 16, page 541
			return static_cast<const Mat4x4>(*m_MatrixStack->GetTop()); 
		}

		LightManager *GetLightManager() { return m_LightManager; }

		void AddAlphaSceneNode(AlphaSceneNode *asn) { m_AlphaSceneNodes.push_back(asn); }

		HRESULT Pick(RayCast *pRayCast) { return m_Root->VPick(this, pRayCast); }

		shared_ptr<IRenderer> GetRenderer() { return m_Renderer; }*/
    }
}