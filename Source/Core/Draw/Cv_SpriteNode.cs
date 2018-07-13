using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Draw
{
    public class Cv_SpriteNode : Cv_SceneNode
    {
        public override float Radius
        {
            get
            {
                if (Properties.Radius < 0)
                {
                    var transf = Parent.Transform;
                    var originFactorX = Math.Abs(transf.Origin.X - 0.5) + 0.5;
                    var originFactorY = Math.Abs(transf.Origin.Y - 0.5) + 0.5;
                    var originFactor = (float) Math.Max(originFactorX, originFactorY);

                    var comp = ((Cv_SpriteComponent) m_Component);
                    Properties.Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height) * originFactor;
                    Properties.Radius *= Math.Max(transf.Scale.X, transf.Scale.Y);
                }

                return Properties.Radius;
            }
        }

		private readonly float MaxLayers = 255;

        public Cv_SpriteNode(Cv_Entity.Cv_EntityID entityID, Cv_RenderComponent renderComponent, Cv_Transform to, Cv_Transform from = null) : base(entityID, renderComponent, to, from)
        {
            var comp = ((Cv_SpriteComponent) renderComponent);
        }

        public override void VRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            var spriteComponent = (Cv_SpriteComponent) m_Component;

            Cv_RawTextureResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>(spriteComponent.Texture, spriteComponent.Owner.ResourceBundle);
			

            var tex = resource.GetTexture().Texture;
            var pos = scene.Transform.Position;
            var rot = scene.Transform.Rotation;
            var scale = scene.Transform.Scale;

            var frameW = tex.Width / spriteComponent.FrameX;
			var frameH = tex.Height / spriteComponent.FrameY;
			var x = (spriteComponent.CurrentFrame % spriteComponent.FrameX) * frameW;
			var y = (spriteComponent.CurrentFrame / spriteComponent.FrameX) * frameH;

            renderer.Draw(tex, new Rectangle((int) pos.X,
                                                    (int)pos.Y,
                                                    (int)(spriteComponent.Width * scale.X),
                                                    (int)(spriteComponent.Height * scale.Y)),
                                    new Rectangle(x,y, frameW, frameH),
                                    spriteComponent.Color,
                                    rot,
                                    new Vector2(frameW * scene.Transform.Origin.X, frameH * scene.Transform.Origin.Y),
                                    SpriteEffects.None,
                                    pos.Z / MaxLayers);
        }

        public override bool VOnChanged(Cv_SceneElement scene)
        {
            var comp = ((Cv_SpriteComponent) m_Component);
            Radius = (float) Math.Sqrt(comp.Width*comp.Width + comp.Height*comp.Height)/2;
            return true;
        }

        public override void VPreRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
        }

        public override bool VIsVisible(Cv_SceneElement scene, Cv_Renderer renderer)
        {
            return true;
        }

        public override bool VPick(Cv_SceneElement scene, Cv_Renderer renderer, Vector2 screenPosition, List<Cv_EntityID> entities)
        {
            var camMatrix = renderer.CamMatrix;
            var worldTransform = Parent.WorldTransform;
            var pos = new Vector2(worldTransform.Position.X, worldTransform.Position.Y);
            var rot = worldTransform.Rotation;
            var scale = worldTransform.Scale;

            var spriteComponent = (Cv_SpriteComponent) m_Component;

			Cv_RawTextureResource resource;
			resource = Cv_ResourceManager.Instance.GetResource<Cv_RawTextureResource>(spriteComponent.Texture, spriteComponent.Owner.ResourceBundle);

            var tex = resource.GetTexture().Texture;
            var frameW = tex.Width / spriteComponent.FrameX;
			var frameH = tex.Height / spriteComponent.FrameY;

            var transformedVertices = new List<Vector2>();
            var point1 = new Vector2(-(worldTransform.Origin.X * spriteComponent.Width * scale.X),
                                     -(worldTransform.Origin.Y * spriteComponent.Height * scale.Y));

            var point2 = new Vector2(point1.X + (spriteComponent.Width * scale.X),
                                     point1.Y);

            var point3 = new Vector2(point2.X,
                                     point1.Y + (spriteComponent.Height * scale.Y));

            var point4 = new Vector2(point1.X,
                                     point3.Y);

            Matrix rotMat = Matrix.CreateRotationZ(rot);
            point1 = Vector2.Transform(point1, rotMat);
            point2 = Vector2.Transform(point2, rotMat);
            point3 = Vector2.Transform(point3, rotMat);
            point4 = Vector2.Transform(point4, rotMat);

            point1 += pos;
            point2 += pos;
            point3 += pos;
            point4 += pos;

            transformedVertices.Add(point1);
            transformedVertices.Add(point2);
            transformedVertices.Add(point3);
            transformedVertices.Add(point4);

            var invertedTransform = Matrix.Invert(camMatrix);
            var worldPoint = Vector2.Transform(screenPosition, invertedTransform);
            if (Cv_DrawUtils.PointInPolygon(worldPoint, transformedVertices))
            {
                entities.Add(Properties.EntityID);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void VPostRender(Cv_SceneElement scene, Cv_Renderer renderer)
        {
        }
    }
}