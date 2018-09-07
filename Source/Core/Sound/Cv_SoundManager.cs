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

        private class FadeSoundData
        {
            public SoundEffectInstance Instance;
            public float FadeRemainingTime;
            public float FinalVolume;
            public bool Paused;
        }

        private Dictionary<string, List<SoundEffectInstance>> m_SoundInstances;
        private Dictionary<SoundEffectInstance, string> m_InstancesToResourceMap;
        private List<SoundEffectInstance> m_SoundInstancesList;
        private List<SoundEffectInstance> m_SoundInstancesListCopy;
        private List<FadeSoundData> m_FadeSoundInstanceList;
        private List<FadeSoundData> m_FadeSoundInstanceListCopy;
        private AudioEmitter m_Emitter;
        private AudioListener m_Listener;

        public SoundEffectInstance PlaySound(string soundResource, string resourceBundle, bool looping = false,
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

            if (m_SoundInstances.ContainsKey(soundResource))
            {
                m_SoundInstances[soundResource].Add(soundInstance);
            }
            else
            {
                var soundList = new List<SoundEffectInstance>();
                soundList.Add(soundInstance);
                m_SoundInstances.Add(soundResource, soundList);
            }

            m_SoundInstancesList.Add(soundInstance);
            m_InstancesToResourceMap.Add(soundInstance, soundResource);

            soundInstance.Volume = volume * GlobalSoundVolume;
            soundInstance.Pan = pan;
            soundInstance.Pitch = pitch;
			soundInstance.IsLooped = looping;
            soundInstance.Play();
            return soundInstance;
        }

        public SoundEffectInstance PlaySound2D(string soundResource, string resourceBundle, Vector2 listener, Vector2 emitter,
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

            if (m_SoundInstances.ContainsKey(soundResource))
            {
                m_SoundInstances[soundResource].Add(soundInstance);
            }
            else
            {
                var soundList = new List<SoundEffectInstance>();
                soundList.Add(soundInstance);
                m_SoundInstances.Add(soundResource, soundList);
            }

            m_SoundInstancesList.Add(soundInstance);
            m_InstancesToResourceMap.Add(soundInstance, soundResource);

            m_Emitter.Position = new Vector3(emitter, 0);
            m_Listener.Position = new Vector3(listener, 0);
            soundInstance.Apply3D(m_Listener, m_Emitter);

            soundInstance.Volume = volume * GlobalSoundVolume;
            soundInstance.Pan = pan;
            soundInstance.Pitch = pitch;
			soundInstance.IsLooped = looping;
            soundInstance.Play();
            return soundInstance;
        }

        public SoundEffectInstance FadeInSound(string soundResource, string resourceBundle, float interval,
														bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || resourceBundle == null || resourceBundle == "")
            {
                Cv_Debug.Error("Error - No sound or resource bundle defined when trying to play sound.");
                return null;
            }

            var fadeSoundData = new FadeSoundData();

            fadeSoundData.FadeRemainingTime = interval;
            fadeSoundData.FinalVolume = volume;
            fadeSoundData.Paused = false;
            fadeSoundData.Instance = PlaySound(soundResource, resourceBundle, looping, 0, pan, pitch);

            m_FadeSoundInstanceList.Add(fadeSoundData);
            return fadeSoundData.Instance;
        }

        public SoundEffectInstance FadeInSound2D(string soundResource, string resourceBundle, Vector2 listener, Vector2 emitter,
                                                        float interval, bool looping = false, float volume = 1f, float pan = 0f, float pitch = 0f)
        {
            if (soundResource == null || soundResource == "" || resourceBundle == null || resourceBundle == "")
            {
                Cv_Debug.Error("Error - No sound or resource bundle defined when trying to play sound.");
                return null;
            }

            var fadeSoundData = new FadeSoundData();

            fadeSoundData.FadeRemainingTime = interval;
            fadeSoundData.FinalVolume = volume;
            fadeSoundData.Paused = false;
            fadeSoundData.Instance = PlaySound2D(soundResource, resourceBundle, listener, emitter, looping, 0, pan, pitch);

            m_FadeSoundInstanceList.Add(fadeSoundData);
            return fadeSoundData.Instance;
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
                    var fadeSoundData = new FadeSoundData();

                    fadeSoundData.FadeRemainingTime = interval;
                    fadeSoundData.FinalVolume = 0;
                    fadeSoundData.Paused = false;
                    fadeSoundData.Instance = s;

                    m_FadeSoundInstanceList.Add(fadeSoundData);
                }
            }
        }

        public void FadeOutSound(SoundEffectInstance instance, float interval)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (m_InstancesToResourceMap.ContainsKey(instance))
            {
                var fadeSoundData = new FadeSoundData();

                fadeSoundData.FadeRemainingTime = interval;
                fadeSoundData.FinalVolume = 0;
                fadeSoundData.Paused = false;
                fadeSoundData.Instance = instance;

                m_FadeSoundInstanceList.Add(fadeSoundData);
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
                foreach (var s in m_SoundInstances[soundResource])
                {
                    s.Stop(immediate);
                    m_SoundInstancesList.Remove(s);
                    m_InstancesToResourceMap.Remove(s);
                    RemoveFadeInstance(s);
                }
                m_SoundInstances[soundResource].Clear();
            }
        }

        public void StopSound(SoundEffectInstance instance, bool immediate = false)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to stop sound.");
                return;
            }

            if (m_InstancesToResourceMap.ContainsKey(instance))
            {
                var resource = m_InstancesToResourceMap[instance];

                instance.Stop(immediate);

                m_SoundInstances[resource].Remove(instance);
                m_SoundInstancesList.Remove(instance);
                m_InstancesToResourceMap.Remove(instance);
                RemoveFadeInstance(instance);
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
                    s.Pause();
                    PauseFadeInstance(s);
                }
            }
        }

        public void PauseSound(SoundEffectInstance instance)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to pause sound.");
                return;
            }

            if (m_InstancesToResourceMap.ContainsKey(instance))
            {
                instance.Pause();
                PauseFadeInstance(instance);
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
                    s.Resume();
                    ResumeFadeInstance(s);
                }
            }
        }

        public void ResumeSound(SoundEffectInstance instance)
        {
            if (instance == null)
            {
                Cv_Debug.Error("Error - No sound defined when trying to resume sound.");
                return;
            }

            if (m_InstancesToResourceMap.ContainsKey(instance))
            {
                instance.Resume();
                ResumeFadeInstance(instance);
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
                    if (s.State == SoundState.Playing)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool SoundIsPlaying(SoundEffectInstance instance)
        {
            return m_InstancesToResourceMap.ContainsKey(instance) && instance.State == SoundState.Playing;
        }
        
        internal Cv_SoundManager()
        {
            m_SoundInstances = new Dictionary<string, List<SoundEffectInstance>>();
            m_SoundInstancesList = new List<SoundEffectInstance>();
            m_SoundInstancesListCopy = new List<SoundEffectInstance>();
            m_InstancesToResourceMap = new Dictionary<SoundEffectInstance, string>();
            m_FadeSoundInstanceList = new List<FadeSoundData>();
            m_FadeSoundInstanceListCopy = new List<FadeSoundData>();
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
                if (s.State == SoundState.Stopped)
                {
                    StopSound(s); //this removes the sound from the manager
                }
            }

            m_FadeSoundInstanceListCopy.Clear();
            m_FadeSoundInstanceListCopy.AddRange(m_FadeSoundInstanceList);
            foreach (var fadeData in m_FadeSoundInstanceListCopy)
            {
                if (fadeData.Paused)
                {
                    continue;
                }

                var volumeDiff = fadeData.FinalVolume - fadeData.Instance.Volume;

                if (volumeDiff == 0 || fadeData.FadeRemainingTime <= 0)
                {
                    if (fadeData.FinalVolume <= 0)
                    {
                        StopSound(fadeData.Instance);
                    }
                    else
                    {
                        RemoveFadeInstance(fadeData.Instance);
                    }
                    continue;
                }

                var volumeIncrement = volumeDiff / fadeData.FadeRemainingTime;
                volumeIncrement *= timeElapsed;
                var newVolume = fadeData.Instance.Volume + volumeIncrement;

                if (volumeIncrement <= 0 && newVolume < fadeData.FinalVolume)
                {
                    newVolume = fadeData.FinalVolume;
                }

                if (volumeIncrement > 0 && newVolume > fadeData.FinalVolume)
                {
                    newVolume = fadeData.FinalVolume;
                }

                fadeData.Instance.Volume = newVolume;
                fadeData.FadeRemainingTime -= timeElapsed;
            }
        }

        private void RemoveFadeInstance(SoundEffectInstance instance)
        {
            FadeSoundData toRemove = null;
            foreach (var fadeData in m_FadeSoundInstanceList)
            {
                if (fadeData.Instance == instance)
                {
                    toRemove = fadeData;
                    break;
                }
            }

            if (toRemove != null)
            {
                m_FadeSoundInstanceList.Remove(toRemove);
            }
        }

        private void PauseFadeInstance(SoundEffectInstance instance)
        {
            foreach (var fadeData in m_FadeSoundInstanceList)
            {
                if (fadeData.Instance == instance)
                {
                    fadeData.Paused = true;
                    return;
                }
            }
        }

        private void ResumeFadeInstance(SoundEffectInstance instance)
        {
            foreach (var fadeData in m_FadeSoundInstanceList)
            {
                if (fadeData.Instance == instance)
                {
                    fadeData.Paused = false;
                    return;
                }
            }
        }
    }
}