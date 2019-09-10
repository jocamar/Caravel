using System;
using Microsoft.Xna.Framework;

namespace Caravel.Core
{
    public struct Cv_Player : IEquatable<Cv_Player>
    {
        public PlayerIndex PlayerIndex
        {
            get
            {
                return m_Player;
            }
        }

        private readonly PlayerIndex m_Player;
        private readonly int m_PlayerIdx;
        
        public static Cv_Player One = new Cv_Player(1);
        public static Cv_Player Two = new Cv_Player(2);
        public static Cv_Player Three = new Cv_Player(3);
        public static Cv_Player Four = new Cv_Player(4);

        public Cv_Player(int pIdx)
        {
            switch (pIdx)
            {
                case 2: m_Player = PlayerIndex.Two; break;
                case 3: m_Player = PlayerIndex.Three; break;
                case 4: m_Player = PlayerIndex.Four; break;
                default:
                    m_Player = PlayerIndex.One;
                    break;
            }

            m_PlayerIdx = pIdx;
        }

        public static implicit operator int (Cv_Player x)
        {
            return x.m_PlayerIdx;
        }

        public static implicit operator Cv_Player(int x)
        {
            return new Cv_Player(x);
        }

        public bool Equals(Cv_Player other)
        {
            return m_Player == other.m_Player;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Cv_Player) obj);
        }

        public override int GetHashCode()
        {
            return m_Player.GetHashCode();
        }

        public static bool operator ==(Cv_Player first, Cv_Player second) 
        {
            return first.Equals(second);
        }

        public static bool operator !=(Cv_Player first, Cv_Player second) 
        {
            return !(first == second);
        }
    }
}