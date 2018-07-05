using System.Collections.Generic;
using System.Xml;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_RigidBodyComponent : Cv_EntityComponent
    {
        public BodyType RigidBodyType
        {
            get
            {
                return m_RigidBodyType;
            }
            set
            {
                m_RigidBodyType = value;
                IsDirty = true;
            }
        }

        public bool FixedRotation
        {
            get
            {
                return m_bFixedRotation;
            }
            set
            {
                m_bFixedRotation = value;
                IsDirty = true;
            }
        }

        public float LinearDamping
        {
            get
            {
                return m_fLinearDamping;
            }
            set
            {
                m_fLinearDamping = value;
                IsDirty = true;
            }
        }

        public float AngularDamping
        {
            get
            {
                return m_fAngularDamping;
            }
            set
            {
                m_fAngularDamping = value;
                IsDirty = true;
            }
        }

        public float GravityScale
        {
            get
            {
                return m_fGravityScale;
            }
            set
            {
                m_fGravityScale = value;
                IsDirty = true;
            }
        }

        public bool UseEntityRotation { get; set; }

        public string Density
        {
            get
            {
                return m_sDensity;
            }

            private set
            {
                m_sDensity = value;
            }
        }

        public string Material
        {
            get
            {
                return m_sMaterial;
            }

            set
            {
                m_sMaterial = value;
                IsDirty = true;
            }
        }

        public Cv_CollisionShape[] CollisionShapes
        {
            get
            {
                return m_CollisionShapes.ToArray();
            }
        }

        #region Movement properties
        public float MaxVelocity { get; set; }
        public float MaxAngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }
        public Vector3 Impulse { get; set; }
        public float AngularVelocity { get; set; }
        public float AngularAcceleration { get; set; }
        public float AngularImpulse { get; set; }
        #endregion

        internal bool IsDirty { get; set; }

        private List<Cv_CollisionShape> m_CollisionShapes;

        #region Private physics members
        private BodyType m_RigidBodyType;
        private float m_fLinearDamping;
        private float m_fAngularDamping;
        private float m_fGravityScale;
        private bool m_bFixedRotation;
        private string m_sDensity;
        private string m_sMaterial;
        #endregion

        protected internal override bool VInit(XmlElement componentData)
        {
            return true;
        }

        protected internal override void VOnChanged()
        {
            throw new System.NotImplementedException();
        }

        protected internal override void VOnUpdate(float deltaTime)
        {
        }

        protected internal override bool VPostInit()
        {
            return true;
        }

        protected internal override void VPostLoad()
        {
        }

        protected internal override XmlElement VToXML()
        {
            throw new System.NotImplementedException();
        }
    }
}