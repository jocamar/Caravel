using System;
using System.Collections.Generic;
using Caravel.Core.Events;
using Caravel.Core.Resource;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

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
            public SoundEffectInstance Instance;
            public float FadeRemainingTime;
            public float FinalVolume;
            public bool Paused;
            public bool Fading;

            internal LinkedListNode<Cv_SoundInstanceData> ListNode;
        }

        private readonly int MAX_INSTANCES_PER_SOUND = 5;

        private Dictionary<string, LinkedList<Cv_SoundInstanceData>> m_SoundInstances;
        private Dictionary<SoundEffectInstance, Cv_SoundInstanceData> m_SoundToDataMap;
        private LinkedList<Cv_SoundInstanceData> m_SoundInstancesList;
        private List<Cv_SoundInstanceData> m_SoundInstancesListCopy;
        private AudioEmitter m_Emitter;
        private AudioListener m_Listener;

        public Cv_SoundInstanceData PlaySound(string soundResource, string resourceBundle, bool looping = false,
																	float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || resourceBundle == null || resourceBundle == "")
            {
                Cv_Debug.Error("Error - No sound or resource bundle defined when trying to play sound.");
                return null;
            }

            Cv_SoundResource sound = Cv_ResourceManager.Instance.GetResource<Cv_SoundResource>(soundResource, resourceBundle);

            var soundData = sound.GetSoundData();
            var soundInstance = soundData.Sound.CreateInstance();

            var playSoundData = new Cv_SoundInstanceData();
            playSoundData.Resource = soundResource;
            playSoundData.FadeRemainingTime = 0;
            playSoundData.FinalVolume = volume;
            playSoundData.Paused = false;
            playSoundData.Instance = soundInstance;
            playSoundData.Fading = false;

            LinkedList<Cv_SoundInstanceData> soundInstances;
            if (m_SoundInstances.TryGetValue(soundResource, out soundInstances))
            {
                if (soundInstances.Count >= MAX_INSTANCES_PER_SOUND)
                {
                    var soundToStop = soundInstances.First.Value;
                    soundToStop.Instance.Stop(true);
                    m_SoundToDataMap.Remove(soundToStop.Instance);
                    m_SoundInstancesList.Remove(soundToStop.ListNode);
                    soundInstances.RemoveFirst();
                    soundToStop.Instance.Dispose();
                }

                soundInstances.AddLast(playSoundData);
            }
            else
            {
                var soundList = new LinkedList<Cv_SoundInstanceData>();
                soundList.AddLast(playSoundData);
                m_SoundInstances.Add(soundResource, soundList);
            }

            playSoundData.ListNode = m_SoundInstancesList.AddLast(playSoundData);
            m_SoundToDataMap.Add(soundInstance, playSoundData);

            soundInstance.Volume = volume * GlobalSoundVolume;
            soundInstance.Pan = pan;
            soundInstance.Pitch = pitch;
			soundInstance.IsLooped = looping;
            soundInstance.Play();
            return playSoundData;
        }

        public Cv_SoundInstanceData PlaySound2D(string soundResource, string resourceBundle, Vector2 listener, Vector2 emitter,
                                                                        bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || resourceBundle == null || resourceBundle == "")
            {
                Cv_Debug.Error("Error - No sound or resource bundle defined when trying to play sound.");
                return null;
            }

            Cv_SoundResource sound = Cv_ResourceManager.Instance.GetResource<Cv_SoundResource>(soundResource, resourceBundle);

            var soundData = sound.GetSoundData();
            var soundInstance = soundData.Sound.CreateInstance();

            var playSoundData = new Cv_SoundInstanceData();
            playSoundData.Resource = soundResource;
            playSoundData.FadeRemainingTime = 0;
            playSoundData.FinalVolume = volume;
            playSoundData.Paused = false;
            playSoundData.Instance = soundInstance;
            playSoundData.Fading = false;

            LinkedList<Cv_SoundInstanceData> soundInstances;
            if (m_SoundInstances.TryGetValue(soundResource, out soundInstances))
            {
                if (soundInstances.Count >= MAX_INSTANCES_PER_SOUND)
                {
                    var soundToStop = soundInstances.First.Value;
                    soundToStop.Instance.Stop(true);
                    m_SoundToDataMap.Remove(soundToStop.Instance);
                    m_SoundInstancesList.Remove(soundToStop.ListNode);
                    soundInstances.RemoveFirst();
                    soundToStop.Instance.Dispose();
                }

                soundInstances.AddLast(playSoundData);
            }
            else
            {
                var soundList = new LinkedList<Cv_SoundInstanceData>();
                soundList.AddLast(playSoundData);
                m_SoundInstances.Add(soundResource, soundList);
            }

            playSoundData.ListNode = m_SoundInstancesList.AddLast(playSoundData);
            m_SoundToDataMap.Add(soundInstance, playSoundData);

            m_Emitter.Position = new Vector3(emitter, 0) * DistanceFallOff;
            m_Listener.Forward = new Vector3(0, 0, -1);
            m_Listener.Up = new Vector3(0,1,0);
            m_Listener.Position = new Vector3(listener, 0) * DistanceFallOff;

            soundInstance.Volume = volume * GlobalSoundVolume;
            soundInstance.Pan = pan;
            soundInstance.Pitch = pitch;
			soundInstance.IsLooped = looping;
            soundInstance.Apply3D(m_Listener, m_Emitter);
            soundInstance.Play();
            soundInstance.Apply3D(m_Listener, m_Emitter);   // This is necessary due to a bug in monogame where 3D sounds do
                                                            //not work correctly unless Apply3D is called both before and after Play
            return playSoundData;
        }

        public Cv_SoundInstanceData FadeInSound(string soundResource, string resourceBundle, float interval,
														bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || resourceBundle == null || resourceBundle == "")
            {
                Cv_Debug.Error("Error - No sound or resource bundle defined when trying to play sound.");
                return null;
            }

            var fadeSoundData = PlaySound(soundResource, resourceBundle, looping, 0, pan, pitch);

            fadeSoundData.FadeRemainingTime = interval;
            fadeSoundData.FinalVolume = volume;
            fadeSoundData.Fading = true;

            return fadeSoundData;
        }

        public Cv_SoundInstanceData FadeInSound2D(string soundResource, string resourceBundle, Vector2 listener, Vector2 emitter,
                                                        float interval, bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || resourceBundle == null || resourceBundle == "")
            {
                Cv_Debug.Error("Error - No sound or resource bundle defined when trying to play sound.");
                return null;
            }

            var fadeSoundData = PlaySound2D(soundResource, resourceBundle, listener, emitter, looping, 0, pan, pitch);

            fadeSoundData.FadeRemainingTime = interval;
            fadeSoundData.FinalVolume = volume;
            fadeSoundData.Fading = true;

            return fadeSoundData;
        }

        public void FadeOutSound(string soundResource, float interval)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (m_SoundInstances.ContainsKey(soundResource))
            {
                foreach (var s in m_SoundInstances[soundResource])
                {
                    s.FadeRemainingTime = interval;
                    s.FinalVolume = 0;
                    s.Paused = false;
                    s.Fading = true;
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
                instance.FadeRemainingTime = interval;
                instance.FinalVolume = 0;
                instance.Paused = false;
                instance.Fading = true;
            }
        }

        public void StopSound(string soundResource, bool immediate = false)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (m_SoundInstances.ContainsKey(soundResource))
            {
                foreach (var soundToStop in m_SoundInstances[soundResource])
                {
                    soundToStop.Instance.Stop(immediate);
                    m_SoundToDataMap.Remove(soundToStop.Instance);
                    m_SoundInstancesList.Remove(soundToStop.ListNode);
                    soundToStop.Instance.Dispose();
                }
                m_SoundInstances[soundResource].Clear();
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
                instance.Instance.Stop(immediate);
                m_SoundToDataMap.Remove(instance.Instance);
                m_SoundInstancesList.Remove(instance.ListNode);
                m_SoundInstances[instance.Resource].Remove(instance);
                instance.Instance.Dispose();
            }
        }

        public void PauseSound(string soundResource)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to pause sound.");
                return;
            }

            if (m_SoundInstances.ContainsKey(soundResource))
            {
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

        public void ResumeSound(string soundResource)
        {
            if (soundResource == null || soundResource == "")
            {
                Cv_Debug.Error("Error - No sound defined when trying to resume sound.");
                return;
            }

            if (m_SoundInstances.ContainsKey(soundResource))
            {
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

        public bool SoundIsPlaying(string soundResource)
        {
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
            m_Emitter = new AudioEmitter();
            m_Listener = new AudioListener();
            Instance = this;
        }

        internal bool Initialize()
        {
            return true;
        }

        internal void OnUpdate(float time, float timeElapsed)
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
                    volumeIncrement *= timeElapsed;
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
                    s.FadeRemainingTime -= timeElapsed;
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
    }
}