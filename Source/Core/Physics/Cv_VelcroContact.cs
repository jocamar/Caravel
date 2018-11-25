using VelcroPhysics.Collision.ContactSystem;

namespace Caravel.Core.Physics
{
    public class Cv_VelcroContact : Cv_Contact
    {
        public override float Friction {
            get {
                return m_Contact.Friction;
            }

            set {
                m_Contact.Friction = value;
            }
        }

        public override float Restitution {
            get {
                return m_Contact.Restitution;
            }

            set {
                m_Contact.Restitution = value;
            }
        }

        public override bool Enabled {
            get {
                return m_Contact.Enabled;
            }

            set {
                m_Contact.Enabled = value;
            }
        }

        private Contact m_Contact;

        public Cv_VelcroContact(Contact contact)
        {
            m_Contact = contact;
        }

        public override void ResetFriction()
        {
            m_Contact.ResetFriction();
        }

        public override void ResetRestitution()
        {
            m_Contact.ResetRestitution();
        }

        public override bool Equals(Cv_Contact other)
        {
            if (other is Cv_VelcroContact)
            {
                var velcroContact = other as Cv_VelcroContact;

                if (velcroContact.m_Contact == m_Contact) {
                    return true;
                }
            }
            
            return false;
        }
    }
}