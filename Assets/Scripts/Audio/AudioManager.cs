// Purpose: To manage the audio in the game. This includes background music, sound effects, and audio pools.

using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [Header("Background Music")]
        [SerializeField, Range(0f, 1f)] private float backgroundVolume = 0.5f;
        [SerializeField] private AudioClip backgroundMusic;
        private float _backgroundVolume;
        private AudioSource _backgroundMusicSource;
        private bool _shouldBackgroundMusicPlay = true;

        
    
        [Header("Sound Effects")]
        [SerializeField] private AudioClip[] audioClips;
        private Dictionary<string, AudioClip> _audioClipDictionary;

    
        // [Header("Audio Pool")]
        // [SerializeField] private AudioSourcePool audioSourcePool;
        //
        private bool _isMuted = false;
        private static AudioManager _instance;


        private void Awake()
        {
            InitializeAudioClipDictionary();
            if(_instance == null)
            {
                _instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            GameEvents.PauseUnpauseBackgroundMusic += PauseUnpauseBackgroundMusic;
            GameEvents.MuteSounds += MuteSounds;
            _backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            _backgroundVolume = backgroundVolume; 
            if (backgroundMusic != null)
            {
                PlayBackgroundMusic(backgroundMusic);
            }
            else
            {
                Debug.LogWarning("No background music assigned.");
            }
        }

        private void OnDisable()
        {
            GameEvents.PauseUnpauseBackgroundMusic -= PauseUnpauseBackgroundMusic;
            GameEvents.MuteSounds -= MuteSounds;
            if (_backgroundMusicSource != null)
            {
                Destroy(_backgroundMusicSource);
            }
        }
        

        private void InitializeAudioClipDictionary()
        {
            _audioClipDictionary = new Dictionary<string, AudioClip>();
            foreach (var audioClip in audioClips)
            {
                _audioClipDictionary.TryAdd(audioClip.name, audioClip);
            }
        }
        

        private void MuteSounds()
        {
            _isMuted = !_isMuted;
            if (_isMuted)
            {
                SetBackgroundMusicVolume(0);
            }
            else
            {
                SetBackgroundMusicVolume(_backgroundVolume);
            }
        }


        private void PlayBackgroundMusic(AudioClip backgroundMusic)
        {
            _backgroundMusicSource.Stop();
            _backgroundMusicSource.clip = backgroundMusic;
            _backgroundMusicSource.volume = backgroundVolume;
            _backgroundMusicSource.loop = true;
            _backgroundMusicSource.Play();
            // _isBackgroundMusicPlaying = true;
        }
    
        private void SetBackgroundMusicVolume(float volume)
        {
            _backgroundMusicSource.volume = volume;
        }

        public PooledAudioSource PlaySound(Vector3 position, string soundName, float volume = 1f, float pitch = 1f, bool loop = false,
            float spatialBlend = 0f)
        {
            var audioSource = AudioSourcePool.Instance.Get();
            if (audioSource != null)
            {
                if(!GetAudioClip(soundName)) return null;
                audioSource.transform.position = position;
                audioSource.SetAudioClip(GetAudioClip(soundName));
                audioSource.SetVolume(volume);
                audioSource.SetPitch(pitch);
                audioSource.SetLoop(loop);
                audioSource.SetSpatialBlend(spatialBlend);
                audioSource.Play();
                // return audioSource;
            }

            return audioSource;
        }

        public void StopAllSoundWithName(string soundName)
        {
            GameEvents.StopSoundByName?.Invoke(soundName);
        }
        
        
        private AudioClip GetAudioClip(string soundName)
        {
            if (_audioClipDictionary.TryGetValue(soundName, out var audioClip))
            {
                return audioClip;
            }

            Debug.Log("Sound not found: " + soundName);
            return null;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_backgroundMusicSource == null) return;
            if (hasFocus)
            {
                if (_shouldBackgroundMusicPlay)
                {
                    _backgroundMusicSource.UnPause();
                }
            }
            else
            {
                _backgroundMusicSource.Pause();
            }
        }

        public void PauseUnpauseBackgroundMusic()
        {
                if (_backgroundMusicSource.isPlaying)
                {
                    _backgroundMusicSource.Pause();
                    _shouldBackgroundMusicPlay = false;
                }
                else
                {
                    _backgroundMusicSource.UnPause();
                    _shouldBackgroundMusicPlay = true;
                }

        }
        public void StopBackgroundMusicPlaying()
        {
            _backgroundMusicSource.Stop();
        }
        
    }
}