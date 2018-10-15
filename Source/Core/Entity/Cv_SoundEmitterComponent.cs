using System;
using System.Globalization;
using System.Xml;
using Caravel.Core.Events;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public class Cv_SoundEmitterComponent : Cv_EntityComponent
    {
        public string SoundResource
        {
            get; set;
        }

        public float Volume
        {
            get
            {
                return m_fVolume;
            }

            set
            {
                m_fVolume = Math.Max(0, Math.Min(value, 1));
            }
        }

        public float Pan
        {
            get
            {
                return m_fPan;
            }

            set
            {
                m_fPan = Math.Max(0, Math.Min(value, 1));
            }
        }

        public float Pitch
        {
            get
            {
                return m_fPitch;
            }

            set
            {
                m_fPitch = Math.Max(0, Math.Min(value, 1));
            }
        }

        public bool Looping
        {
            get; set;
        }

        public bool IsPositional
        {
            get; set;
        }

        public bool AutoPlay
        {
            get; set;
        }

        private float m_fVolume;
        private float m_fPan;
        private float m_fPitch;

        private bool m_bPlayed = false;

        public override XmlElement VToXML()
        {
            var componentDoc = new XmlDocument();
            var componentData = componentDoc.CreateElement(GetComponentName<Cv_SoundEmitterComponent>());
            var sound = componentDoc.CreateElement("SoundEffect");
            var volume = componentDoc.CreateElement("Volume");
            var pan = componentDoc.CreateElement("Pan");
            var pitch = componentDoc.CreateElement("Pitch");
            var looping = componentDoc.CreateElement("Looping");
            var positional = componentDoc.CreateElement("IsPositional");
            var autoPlay = componentDoc.CreateElement("AutoPlay");

            sound.SetAttribute("resource", SoundResource);
            volume.SetAttribute("value", Volume.ToString(CultureInfo.InvariantCulture));
            pan.SetAttribute("value", Pan.ToString(CultureInfo.InvariantCulture));
            pitch.SetAttribute("value", Pitch.ToString(CultureInfo.InvariantCulture));
            looping.SetAttribute("status", Looping.ToString(CultureInfo.InvariantCulture));
            positional.SetAttribute("status", IsPositional.ToString(CultureInfo.InvariantCulture));
            autoPlay.SetAttribute("status", AutoPlay.ToString(CultureInfo.InvariantCulture));

            componentData.AppendChild(sound);
            componentData.AppendChild(volume);
            componentData.AppendChild(pan);
            componentData.AppendChild(pitch);
            componentData.AppendChild(looping);
            componentData.AppendChild(positional);
            componentData.AppendChild(autoPlay);
            
            return componentData;
        }

        public void PlaySound()
        {
            var emitter = Vector2.Zero;
            var listener = Vector2.Zero;
            if (IsPositional)
            {
                var tranform = Owner.GetComponent<Cv_TransformComponent>();

                if (tranform != null)
                {
                    emitter = new Vector2(tranform.Position.X, tranform.Position.Y);
                }

                var playerView = CaravelApp.Instance.GetPlayerView(PlayerIndex.One);

                if (playerView != null && playerView.ListenerEntity != null)
                {
                    var listenerTransform = playerView.ListenerEntity.GetComponent<Cv_TransformComponent>();

                    if (listenerTransform != null)
                    {
                        listener = new Vector2(listenerTransform.Position.X, listenerTransform.Position.Y);
                    }
                }
            }

            Cv_Event_PlaySound playEvt = new Cv_Event_PlaySound(Owner.ID, this, SoundResource, Looping, Volume, Pan,
                                                                        Pitch, false, 0, IsPositional, emitter, listener);

            Cv_EventManager.Instance.QueueEvent(playEvt);
        }

        public void PlayOneShotSound(string soundResource, float volume, float pan, float pitch)
        {
            var emitter = Vector2.Zero;
            var listener = Vector2.Zero;
            if (IsPositional)
            {
                var tranform = Owner.GetComponent<Cv_TransformComponent>();

                if (tranform != null)
                {
                    emitter = new Vector2(tranform.Position.X, tranform.Position.Y);
                }

                var playerView = CaravelApp.Instance.GetPlayerView(PlayerIndex.One);

                if (playerView != null && playerView.ListenerEntity != null)
                {
                    var listenerTransform = playerView.ListenerEntity.GetComponent<Cv_TransformComponent>();

                    if (listenerTransform != null)
                    {
                        listener = new Vector2(listenerTransform.Position.X, listenerTransform.Position.Y);
                    }
                }
            }

            Cv_Event_PlaySound playEvt = new Cv_Event_PlaySound(Owner.ID, this, soundResource, false, volume, pan,
                                                                    pitch, false, 0, IsPositional, emitter, listener);

            Cv_EventManager.Instance.QueueEvent(playEvt);
        }

        public void FadeInSound(float interval)
        {
            var emitter = Vector2.Zero;
            var listener = Vector2.Zero;

            if (IsPositional)
            {
                var tranform = Owner.GetComponent<Cv_TransformComponent>();

                if (tranform != null)
                {
                    emitter = new Vector2(tranform.Position.X, tranform.Position.Y);
                }

                var playerView = CaravelApp.Instance.GetPlayerView(PlayerIndex.One);

                if (playerView != null && playerView.ListenerEntity != null)
                {
                    var listenerTransform = playerView.ListenerEntity.GetComponent<Cv_TransformComponent>();

                    if (listenerTransform != null)
                    {
                        listener = new Vector2(listenerTransform.Position.X, listenerTransform.Position.Y);
                    }
                }
            }

            Cv_Event_PlaySound playEvt = new Cv_Event_PlaySound(Owner.ID, this, SoundResource, Looping, Volume, Pan,
                                                                    Pitch, true, interval, IsPositional, emitter, listener);

            Cv_EventManager.Instance.QueueEvent(playEvt);
        }

        public void StopSound()
        {
            Cv_Event_StopSound stopEvent = new Cv_Event_StopSound(Owner.ID, SoundResource, this);
            Cv_EventManager.Instance.QueueEvent(stopEvent);
        }

        public void PauseSound()
        {
            Cv_Event_PauseSound pauseEvent = new Cv_Event_PauseSound(Owner.ID, SoundResource, this);
            Cv_EventManager.Instance.QueueEvent(pauseEvent);
        }

        public void ResumeSound()
        {
            Cv_Event_ResumeSound resumeEvent = new Cv_Event_ResumeSound(Owner.ID, SoundResource, this);
            Cv_EventManager.Instance.QueueEvent(resumeEvent);
        }

        public void FadeOutSound(float interval)
        {
            var emitter = Vector2.Zero;
            var listener = Vector2.Zero;

            if (IsPositional)
            {
                var tranform = Owner.GetComponent<Cv_TransformComponent>();

                if (tranform != null)
                {
                    emitter = new Vector2(tranform.Position.X, tranform.Position.Y);
                }

                var playerView = CaravelApp.Instance.GetPlayerView(PlayerIndex.One);

                if (playerView != null && playerView.ListenerEntity != null)
                {
                    var listenerTransform = playerView.ListenerEntity.GetComponent<Cv_TransformComponent>();

                    if (listenerTransform != null)
                    {
                        listener = new Vector2(listenerTransform.Position.X, listenerTransform.Position.Y);
                    }
                }
            }

            Cv_Event_PlaySound fadeEvent = new Cv_Event_PlaySound(Owner.ID, this, SoundResource, Looping, 0, Pan,
                                                                    Pitch, true, interval, IsPositional, emitter, listener);
            Cv_EventManager.Instance.QueueEvent(fadeEvent);
        }

        public override bool VInitialize(XmlElement componentData)
        {
            var soundNode = componentData.SelectNodes("SoundEffect").Item(0);
            if (soundNode != null)
            {
                SoundResource = soundNode.Attributes["resource"].Value;
            }

            var volumeNode = componentData.SelectNodes("Volume").Item(0);
            if (volumeNode != null)
            {
                Volume = float.Parse(volumeNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var panNode = componentData.SelectNodes("Pan").Item(0);
            if (panNode != null)
            {
                Pan = float.Parse(panNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var pitchNode = componentData.SelectNodes("Pitch").Item(0);
            if (pitchNode != null)
            {
                Pitch = float.Parse(pitchNode.Attributes["value"].Value, CultureInfo.InvariantCulture);
            }

            var loopingNode = componentData.SelectNodes("Looping").Item(0);
            if (loopingNode != null)
            {
                Looping = bool.Parse(loopingNode.Attributes["status"].Value);
            }

            var positionalNode = componentData.SelectNodes("IsPositional").Item(0);
            if (positionalNode != null)
            {
                IsPositional = bool.Parse(positionalNode.Attributes["status"].Value);
            }

            var autoPlayNode = componentData.SelectNodes("AutoPlay").Item(0);
            if (autoPlayNode != null)
            {
                AutoPlay = bool.Parse(autoPlayNode.Attributes["status"].Value);
            }

            return true;
        }

        public override void VOnChanged()
        {
        }

        public override void VOnDestroy()
        {
        }

        public override bool VPostInitialize()
        {
            return true;
        }

        public override void VPostLoad()
        {
        }

        protected internal override void VOnUpdate(float elapsedTime)
        {
            if (!m_bPlayed && AutoPlay && !CaravelApp.Instance.EditorRunning)
            {
                m_bPlayed = true;
                PlaySound();
            }
        }
    }
}