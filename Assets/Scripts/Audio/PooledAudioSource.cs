using System.Collections;
using Pool;
using UnityEngine;
using Utilities;

namespace Audio
{
    public class PooledAudioSource : MonoBehaviour, IPoolable
    {
        private AudioSource _audioSource;
        private static bool _isMuted = false;
        private float _originalVolume;
        private bool _isLooping;


        private void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();
            // _originalVolume = _audioSource.volume;
            GameEvents.MuteSounds += MuteSounds;
            GameEvents.StopSoundByName += StopSoundByName;
        }

        private void OnDisable()
        {
            GameEvents.MuteSounds -= MuteSounds;
            GameEvents.StopSoundByName -= StopSoundByName;
        }

        private void StopSoundByName(string soundName)
        {
            if (_audioSource.clip.name == soundName)
            {
                StopAudio();
            }
        }

        private void MuteSounds()
        {
            _isMuted = !_isMuted;
            if (!_isMuted) SetVolume(0);
            else SetVolume(_originalVolume);
        }

        public void SetAudioClip(AudioClip audioClip)
        {
            _audioSource.clip = audioClip;
        }

        public void SetPitch(float pitch)
        {
            _audioSource.pitch = pitch;
        }

        public void SetVolume(float volume)
        {
            _originalVolume = volume;
            _audioSource.volume = volume;
        }

        public void SetLoop(bool loop)
        {
            _isLooping = loop;
            _audioSource.loop = loop;
        }

        public void SetSpatialBlend(float spatialBlend)
        {
            _audioSource.spatialBlend = spatialBlend;
        }

        private float GetClipLength()
        {
            return _audioSource.clip.length;
        }


        public void Play()
        {
            _audioSource.Play();
            StartCoroutine(ReturnToPoolWhenFinished());
        }

        public void StopAudio()
        {
            _audioSource.Stop();
            AudioSourcePool.Instance.Return(this);
        }

        private IEnumerator ReturnToPoolWhenFinished()
        {
            if (_isLooping) yield break;
            var clipLength = GetClipLength();
            if (clipLength < 1) yield return new WaitForSeconds(1f);
            else yield return new WaitWhile(() => _audioSource.isPlaying);
            AudioSourcePool.Instance.Return(this);
        }

        public void Reset()
        {
            _audioSource.Stop();
        }
    }
}