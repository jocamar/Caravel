using System;
using System.Collections.Generic;
using Caravel.Core.Entity;
using Caravel.Core.Resource;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using static Caravel.Core.Entity.Cv_Entity;

namespace Caravel.Core.Sound
{
    public class Cv_SoundManager
    {
        public static Cv_SoundManager Instance
        {
            get; private set;
        }

        public float GlobalSoundVolume
        {
            get; set;
        }

        public float DistanceFallOff
        {
            get; set;
        }

        public class Cv_SoundInstanceData
        {
            public string Resource;
            public Cv_EntityID EntityId;
            public SoundEffectInstance Instance;
            public float FadeRemainingTime;
            public float FinalVolume;
            public bool Paused;
            public bool Fading;

            internal LinkedListNode<Cv_SoundInstanceData> ListNode;
        }

        private readonly int MAX_INSTANCES_PER_SOUND = 5;

        private Dictionary<string, LinkedList<Cv_SoundInstanceData>> m_SoundInstances;
        private Dictionary<Cv_EntityID, LinkedList<Cv_SoundInstanceData>> m_EntityToInstancesMap;
        private Dictionary<SoundEffectInstance, Cv_SoundInstanceData> m_SoundToDataMap;
        private LinkedList<Cv_SoundInstanceData> m_SoundInstancesList;
        private List<Cv_SoundInstanceData> m_SoundInstancesListCopy;
        private AudioEmitter m_Emitter;
        private AudioListener m_Listener;

        public Cv_SoundInstanceData PlaySound(string soundResource, Cv_Entity entity, bool looping = false,
																	float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || entity == null)
            {
                Cv_Debug.Error("Error - No sound or entity defined when trying to play sound.");
                return null;
            }

            var playSoundData = GetNewSoundInstance(soundResource, entity, volume, pan, pitch, looping);
            AddSoundToManager(playSoundData);

			try
			{
            	playSoundData.Instance.Play();
			}
			catch (Exception e)
			{
				Cv_Debug.Error("Unable to play sound: " + soundResource + "\n" + e.ToString());
			}
            return playSoundData;
        }

        public Cv_SoundInstanceData PlaySound2D(string soundResource, Cv_Entity entity, Vector2 listener, Vector2 emitter,
                                                                        bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || entity == null)
            {
                Cv_Debug.Error("Error - No sound or entity defined when trying to play sound.");
                return null;
            }

            var playSoundData = GetNewSoundInstance(soundResource, entity, volume, pan, pitch, looping);
            AddSoundToManager(playSoundData);

            m_Emitter.Position = new Vector3(emitter, 0) * DistanceFallOff;
            m_Listener.Forward = new Vector3(0, 0, -1);
            m_Listener.Up = new Vector3(0,1,0);
            m_Listener.Position = new Vector3(listener, 0) * DistanceFallOff;

			try
			{
				playSoundData.Instance.Apply3D(m_Listener, m_Emitter);
				playSoundData.Instance.Play();
				playSoundData.Instance.Apply3D(m_Listener, m_Emitter);  // This is necessary due to a bug in monogame where 3D sounds do
																		//not work correctly unless Apply3D is called both before and after Play
			}
			catch (Exception e)
			{
				Cv_Debug.Error("Unable to play sound: " + soundResource + "\n" + e.ToString());
			}

            return playSoundData;
        }

        public Cv_SoundInstanceData FadeInSound(string soundResource, Cv_Entity entity, float interval,
														bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || entity == null)
            {
                Cv_Debug.Error("Error - No sound orentity defined when trying to play sound.");
                return null;
            }

            var fadeSoundData = PlaySound(soundResource, entity, looping, 0, pan, pitch);

            FadeSound(fadeSoundData, volume, interval);

            return fadeSoundData;
        }

        public Cv_SoundInstanceData FadeInSound2D(string soundResource, Cv_Entity entity, Vector2 listener, Vector2 emitter,
                                                        float interval, bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || entity == null)
            {
                Cv_Debug.Error("Error - No sound or entity defined when trying to play sound.");
                return null;
            }

            var fadeSoundData = PlaySound2D(soundResource, entity, listener, emitter, looping, 0, pan, pitch);

            FadeSound(fadeSoundData, volume, interval);

            return fadeSoundData;
        }

        public void FadeOutSound(string soundResource, float interval, Cv_Entity entity = null)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (entity != null) //If an entity is specified fades the first instance of that sound belonging to the entity
            {
                if (!m_EntityToInstancesMap.ContainsKey(entity.ID))
                {
                    return;
                }

                var instances = m_EntityToInstancesMap[entity.ID];

                Cv_SoundInstanceData soundToFade = null;
                foreach (var s in instances)
                {
                    if (s.Resource == soundResource)
                    {
                        soundToFade = s;
                        break;
                    }
                }

                if (soundToFade != null)
                {
                    FadeSound(soundToFade, 0, interval);
                }
            }
            else //Fades all instances of that sound
            {
                if (!m_SoundInstances.ContainsKey(soundResource))
                {
                    return;
                }

                var instances = m_SoundInstances[soundResource];
                foreach (var s in instances)
                {
                    FadeSound(s, 0, interval);
                }
            }
        }

        public void FadeOutSound(Cv_SoundInstanceData instance, float interval)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (m_SoundToDataMap.ContainsKey(instance.Instance))
            {
                FadeSound(instance, 0, interval);
            }
        }

        public void StopSound(string soundResource, Cv_Entity entity = null, bool immediate = false)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (entity != null) //If an entity is specified removes the first instance of that sound belonging to the entity
            {
                if (!m_EntityToInstancesMap.ContainsKey(entity.ID))
                {
                    return;
                }

                var instances = m_EntityToInstancesMap[entity.ID];

                Cv_SoundInstanceData soundToStop = null;
                foreach (var s in instances)
                {
                    if (s.Resource == soundResource)
                    {
                        soundToStop = s;
                        break;
                    }
                }

                if (soundToStop != null)
                {
                    StopSound(soundToStop);
                }
            }
            else //Removes all instances of that sound
            {
                if (!m_SoundInstances.ContainsKey(soundResource))
                {
                    return;
                }
                    
                m_SoundInstancesListCopy.Clear();
                m_SoundInstancesListCopy.AddRange(m_SoundInstances[soundResource]);
                foreach (var soundToStop in m_SoundInstancesListCopy)
                {
                    StopSound(soundToStop);
                }
            }
        }

        public void StopSound(Cv_SoundInstanceData instance, bool immediate = false)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (m_SoundToDataMap.ContainsKey(instance.Instance))
            {
                StopSound(instance);
            }
        }

        public void PauseSound(string soundResource, Cv_Entity entity = null)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to pause sound.");
                return;
            }

            if (entity != null) //If an entity is specified pauses the first instance of that sound belonging to the entity
            {
                if (!m_EntityToInstancesMap.ContainsKey(entity.ID))
                {
                    return;
                }

                var instances = m_EntityToInstancesMap[entity.ID];

                Cv_SoundInstanceData soundToPause = null;
                foreach (var s in instances)
                {
                    if (s.Resource == soundResource)
                    {
                        soundToPause = s;
                        break;
                    }
                }

                if (soundToPause != null)
                {
                    soundToPause.Instance.Pause();
                    soundToPause.Paused = true;
                }
            }
            else //Pauses all instances of that sound
            {
                if (!m_SoundInstances.ContainsKey(soundResource))
                {
                    return;
                }

                foreach (var s in m_SoundInstances[soundResource])
                {
                    s.Instance.Pause();
                    s.Paused = true;
                }
            }
        }

        public void PauseSound(Cv_SoundInstanceData instance)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to pause sound.");
                return;
            }

            if (m_SoundToDataMap.ContainsKey(instance.Instance))
            {
                instance.Instance.Pause();
                instance.Paused = true;
            }
        }

        public void ResumeSound(string soundResource, Cv_Entity entity = null)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to resume sound.");
                return;
            }

            if (entity != null) //If an entity is specified resumes the first instance of that sound belonging to the entity
            {
                if (!m_EntityToInstancesMap.ContainsKey(entity.ID))
                {
                    return;
                }

                var instances = m_EntityToInstancesMap[entity.ID];

                Cv_SoundInstanceData soundToResume = null;
                foreach (var s in instances)
                {
                    if (s.Resource == soundResource)
                    {
                        soundToResume = s;
                        break;
                    }
                }

                if (soundToResume != null)
                {
                    soundToResume.Instance.Resume();
                    soundToResume.Paused = false;
                }
            }
            else //Resumes all instances of that sound
            {
                if (!m_SoundInstances.ContainsKey(soundResource))
                {
                    return;
                }

                foreach (var s in m_SoundInstances[soundResource])
                {
                    s.Instance.Resume();
                    s.Paused = false;
                }
            }
        }

        public void ResumeSound(Cv_SoundInstanceData instance)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to resume sound.");
                return;
            }

            if (m_SoundToDataMap.ContainsKey(instance.Instance))
            {
                instance.Instance.Resume();
                instance.Paused = false;
            }
        }

        public void StopAllSounds(bool immediate = false)
        {
            m_SoundInstancesListCopy.Clear();
            m_SoundInstancesListCopy.AddRange(m_SoundInstancesList);

            foreach (var s in m_SoundInstancesListCopy)
            {
                StopSound(s, immediate);
            }
        }

        public void PauseAllSounds()
        {
            foreach (var s in m_SoundInstancesList)
            {
                PauseSound(s);
            }
        }

        public void ResumeAllSounds()
        {
            foreach (var s in m_SoundInstancesList)
            {
                ResumeSound(s);
            }
        }

        public bool SoundIsPlaying(string soundResource, Cv_Entity entity = null)
        {
            if (entity != null)
            {
                if (m_EntityToInstancesMap.ContainsKey(entity.ID))
                {
                    var instances = m_EntityToInstancesMap[entity.ID];

                    foreach (var s in instances)
                    {
                        if (s.Instance.State == SoundState.Playing && s.Resource == soundResource)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            if (m_SoundInstances.ContainsKey(soundResource))
            {
                var instances = m_SoundInstances[soundResource];

                foreach (var s in instances)
                {
                    if (s.Instance.State == SoundState.Playing)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool SoundIsPlaying(Cv_SoundInstanceData instanceData)
        {
            return m_SoundToDataMap.ContainsKey(instanceData.Instance) && instanceData.Instance.State == SoundState.Playing;
        }
        
        internal Cv_SoundManager()
        {
            GlobalSoundVolume = 1f;
            DistanceFallOff = 1f;
            m_SoundInstances = new Dictionary<string, LinkedList<Cv_SoundInstanceData>>();
            m_SoundInstancesList = new LinkedList<Cv_SoundInstanceData>();
            m_SoundInstancesListCopy = new List<Cv_SoundInstanceData>();
            m_SoundToDataMap = new Dictionary<SoundEffectInstance, Cv_SoundInstanceData>();
            m_EntityToInstancesMap = new Dictionary<Cv_EntityID, LinkedList<Cv_SoundInstanceData>>();
            m_Emitter = new AudioEmitter();
            m_Listener = new AudioListener();
            Instance = this;
        }

        internal bool Initialize()
        {
            return true;
        }

        internal void OnUpdate(float time, float elapsedTime)
        {
            m_SoundInstancesListCopy.Clear();
            m_SoundInstancesListCopy.AddRange(m_SoundInstancesList);

            foreach (var s in m_SoundInstancesListCopy)
            {
                if (s.Fading)
                {
                    if (s.Paused)
                    {
                        continue;
                    }

                    var volumeDiff = s.FinalVolume - s.Instance.Volume;

                    if (volumeDiff == 0 || s.FadeRemainingTime <= 0)
                    {
                        if (s.FinalVolume <= 0)
                        {
                            StopSound(s);
                        }
                        else
                        {
                            s.Fading = false;
                        }
                        continue;
                    }

                    var volumeIncrement = volumeDiff / s.FadeRemainingTime;
                    volumeIncrement *= elapsedTime;
                    var newVolume = s.Instance.Volume + volumeIncrement;

                    if (volumeIncrement <= 0 && newVolume < s.FinalVolume)
                    {
                        newVolume = s.FinalVolume;
                    }

                    if (volumeIncrement > 0 && newVolume > s.FinalVolume)
                    {
                        newVolume = s.FinalVolume;
                    }

                    s.Instance.Volume = newVolume;
                    s.FadeRemainingTime -= elapsedTime;
                }
                else
                {
                    if (s.Instance.State == SoundState.Stopped)
                    {
                        StopSound(s); //this removes the sound from the manager
                    }
                }
            }
        }

        private void FadeSound(Cv_SoundInstanceData soundToFade, float volume, float interval)
        {
            soundToFade.FadeRemainingTime = interval;
            soundToFade.FinalVolume = volume;
            soundToFade.Paused = false;
            soundToFade.Fading = true;
        }

        private void StopSound(Cv_SoundInstanceData soundToStop)
        {
            soundToStop.Instance.Stop(true);
            m_SoundToDataMap.Remove(soundToStop.Instance);
            m_SoundInstancesList.Remove(soundToStop.ListNode);
            m_EntityToInstancesMap[soundToStop.EntityId].Remove(soundToStop);
            m_SoundInstances[soundToStop.Resource].Remove(soundToStop);
            soundToStop.Instance.Dispose();
        }

        private void AddSoundToManager(Cv_SoundInstanceData soundToAdd)
        {
            LinkedList<Cv_SoundInstanceData> soundInstances;
            if (m_SoundInstances.TryGetValue(soundToAdd.Resource, out soundInstances))
            {
                if (soundInstances.Count >= MAX_INSTANCES_PER_SOUND)
                {
                    StopSound(soundInstances.First.Value);
                }

                soundInstances.AddLast(soundToAdd);
            }
            else
            {
                var soundList = new LinkedList<Cv_SoundInstanceData>();
                soundList.AddLast(soundToAdd);
                m_SoundInstances.Add(soundToAdd.Resource, soundList);
            }

            if (m_EntityToInstancesMap.TryGetValue(soundToAdd.EntityId, out soundInstances))
            {
                soundInstances.AddLast(soundToAdd);
            }
            else
            {
                var soundList = new LinkedList<Cv_SoundInstanceData>();
                soundList.AddLast(soundToAdd);
                m_EntityToInstancesMap.Add(soundToAdd.EntityId, soundList);
            }

            soundToAdd.ListNode = m_SoundInstancesList.AddLast(soundToAdd);
            m_SoundToDataMap.Add(soundToAdd.Instance, soundToAdd);
        }

        private Cv_SoundInstanceData GetNewSoundInstance(string soundResource, Cv_Entity entity, float volume, float pan, float pitch, bool looping)
        {
             Cv_SoundResource sound = Cv_ResourceManager.Instance.GetResource<Cv_SoundResource>(soundResource, entity.ResourceBundle);

            var soundData = sound.GetSoundData();
            var soundInstance = soundData.Sound.CreateInstance();

            var playSoundData = new Cv_SoundInstanceData();
            playSoundData.Resource = soundResource;
            playSoundData.FadeRemainingTime = 0;
            playSoundData.FinalVolume = volume;
            playSoundData.Paused = false;
            playSoundData.Instance = soundInstance;
            playSoundData.Fading = false;
            playSoundData.EntityId = entity.ID;

            soundInstance.Volume = volume * GlobalSoundVolume;
            soundInstance.Pan = pan;
            soundInstance.Pitch = pitch;
			soundInstance.IsLooped = looping;

            return playSoundData;
        }
    }
}