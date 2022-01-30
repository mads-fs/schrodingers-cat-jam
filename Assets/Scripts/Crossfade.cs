using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JGDT.Audio.Crossfade
{
    /// <summary>
    /// This component will fade sound between two <see cref="AudioSource"/>s.
    /// The component is designed to be used programmatically and assumes that both sound clips are the exact same length.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Crossfade : MonoBehaviour
    {
        /// <summary>
        /// Fires when a Fade is started.
        /// </summary>
        public event EventHandler OnFadeStart;
        /// <summary>
        /// Fires when a Fade has ended.
        /// </summary>
        public event EventHandler OnFadeEnd;

        [Tooltip("The amount of time it takes to fade the sound in seconds.")]
        public float FadeTime = 1f;
        [Tooltip("The desired volume when fading in.")]
        public float VolumeFadeIn = 1f;
        [Tooltip("The desired volume when fading out.")]
        public float VolumeFadeOut = 0f;
        [Tooltip("The curve used to fade the sound.")]
        public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1f, 1f) });
        [Space(20), Header("Read Only")]
        [SerializeField]
        private bool _isPaused = false; // This bool will be used if the fading needs to be paused.
        [SerializeField]
        private bool _isFading = false; // This bool will be used to determine if the source is currently fading or not.
        /// <summary>
        /// One of two <see cref="AudioSource"/>s that this component will use. Required and auto-added.
        /// </summary>
        public AudioSource SourceA;
        /// <summary>
        /// One of two <see cref="AudioSource"/>s that this component will use. Required and auto-added.
        /// </summary>
        public AudioSource SourceB;

        // Private Fields
        /// <summary>
        /// A queue in case the Fade function is called multiple times in succession.
        /// </summary>
        private Queue<float> _queue;

        private string _activeSource = "SourceA";

        private void Start()
        {
            if (SourceA == null || SourceB == null)
            {
                AudioSource[] sources = GetComponents<AudioSource>();
                if (sources.Length != 2)
                {
                    Debug.LogError($"{this} could not find two AudioSources. The Component will disable.");
                    enabled = false;
                }
                else
                {
                    SourceA = sources[0];
                    SourceB = sources[1];
                    _queue = new Queue<float>();
                    SourceB.Stop();
                    SourceA.volume = VolumeFadeIn;
                    SourceB.volume = VolumeFadeOut;
                    SourceB.playOnAwake = false;
                }
            }
            else
            {
                _queue = new Queue<float>();
                SourceB.Stop();
                SourceA.volume = VolumeFadeIn;
                SourceB.volume = VolumeFadeOut;
                SourceB.playOnAwake = false;
            }
        }

        #region Public Methods
        public void Pause()
        {
            SourceA.Pause(); 
            SourceB.Pause();
        }

        public void Play()
        {
            SourceA.Play();
            SourceB.Play();
        }
        /// <summary>
        /// Will pause fading whehter it's currently running or not. Will not pause audio.
        /// </summary>
        public void PauseFade() => _isPaused = true;
        /// <summary>
        /// Will unpause fading whehter it's currently running or not. Will not pause audio.
        /// </summary>
        public void UnPauseFade() => _isPaused = false;
        /// <summary>
        /// Will fade in the sound using the component <see cref="FadeTime"/>.
        /// </summary>
        public void Fade() => QueueFade(FadeTime);
        /// <summary>
        /// An override version of <see cref="Fade"/> that uses the specified time rather than the components time.
        /// </summary>
        /// <param name="fadeTime">The time to use to fade the sound.</param>
        public void Fade(float fadeTime) => QueueFade(fadeTime);
        #endregion

        #region Private Methods
        /// <summary>
        /// Will either queue a fade if fading is already taking place or play it immediately.
        /// </summary>
        /// <param name="time">The time it should take to fade.</param>
        private void QueueFade(float time)
        {
            if (_isFading == false)
            {
                _isFading = true;
                StartCoroutine(DoFade(time));
            }
            else
            {
                _queue.Enqueue(time);
            }
        }

        /// <summary>
        /// Called by the Fade coroutines to see if more fades should be played after the previous one.
        /// </summary>
        private void TryPlayNextFade()
        {
            if (_queue.Count > 0)
            {
                float time = _queue.Dequeue();
                StartCoroutine(DoFade(time));
            }
            else
            {
                _isFading = false;
            }
        }

        private IEnumerator DoFade(float fadeTime)
        {
            OnFadeStart?.Invoke(this, null);
            float fadeInTime = 0f;
            float fadeOutTime = fadeTime;

            AudioSource active = (_activeSource == "SourceA" ? SourceA : SourceB);
            AudioSource inactive = (_activeSource == "SourceA" ? SourceB : SourceA);
            _activeSource = (_activeSource == "SourceA" ? "SourceB" : "SourceA");
            float activeVolumeStart = active.volume;
            float inactiveVolumeStart = inactive.volume;
            inactive.time = active.time;
            inactive.Play();

            while (fadeInTime < fadeTime)
            {
                if (_isPaused == false)
                {
                    fadeInTime += Time.deltaTime;
                    fadeOutTime -= Time.deltaTime;
                    float curveAlphaIn = FadeCurve.Evaluate(fadeInTime / fadeTime);
                    float curveAlphaOut = FadeCurve.Evaluate(fadeOutTime / fadeTime);
                    active.volume = Mathf.Lerp(VolumeFadeOut, activeVolumeStart, curveAlphaOut);
                    inactive.volume = Mathf.Lerp(inactiveVolumeStart, VolumeFadeIn, curveAlphaIn);
                }
                yield return null;
            }
            active.volume = VolumeFadeOut;
            inactive.volume = VolumeFadeIn;
            active.Stop();
            OnFadeEnd?.Invoke(this, null);
            TryPlayNextFade();
        }
        #endregion

        private void OnValidate()
        {
            if (SourceA == null)
            {
                AudioSource source = GetComponent<AudioSource>();
                if (source) SourceA = source;
            }
            if (SourceB == null)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                SourceB = source;
            }
        }
    }
}
