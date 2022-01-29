using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace JGDT.Audio.OneShot
{
    /// <summary>
    /// This component will play the sound in the <see cref="AudioSource"/> to completion as soon as it is initialised then destroy itself.
    /// Use this to make simple one-shot audio prefabs for 2D and 3D audio.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class OneShotAudio : MonoBehaviour
    {
        [Tooltip("The clip to play.")]
        public AudioClip Clip;
        [Tooltip("The Audio Mixer Group the AudioSource should use (if any).")]
        public AudioMixerGroup MixerGroup;

        // Private Fields
        private AudioSource _source;
        private bool _isPaused = false;

        private void Start()
        {
            if (Clip == null)
            {
                Debug.LogWarning($"{this} was given a null AudioClip. Didn't fire.");
                Destroy(gameObject);
            }
            else
            {
                _source = GetComponent<AudioSource>();
                _source.clip = Clip;
                if (MixerGroup) _source.outputAudioMixerGroup = MixerGroup;
                StartCoroutine(DoPlaySound());
            }
        }
        /// <summary>
        /// Use this to pause the <see cref="AudioSource"/> if needed.
        /// </summary>
        public void Pause() => _source.Stop();
        /// <summary>
        /// Use this to unpause the <see cref="AudioSource"/>.
        /// </summary>
        public void UnPause() => _source.Play();

        private IEnumerator DoPlaySound()
        {
            _source.Play();
            float time = 0f;
            float clipLength = Clip.length + 0.05f;
            while (time < clipLength)
            {
                if (_isPaused == false)
                {
                    time += Time.deltaTime;
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
