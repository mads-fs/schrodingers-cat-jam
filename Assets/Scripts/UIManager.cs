using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SC
{
    public class UIManager : MonoBehaviour
    {
        public GameObject UIParent;
        [Header("Audio")]
        public float LowestVolume = -50;
        public float HighestVolume = 0f;
        public float HighestSFXVolume = 5f;
        public float HighestMusicVolume = -10f;
        [Header("Sliders")]
        public Slider MasterSlider;
        public Slider SFXSlider;
        public Slider MusicSlider;
        [Header("Buttons")]
        public Button CloseButton;
        public Button ExitButton;

        private string _masterVolumeKey = "masterVolume";
        private string _sfxVolumeKey = "sfxVolume";
        private string _musicVolumeKey = "musicVolume";

        private void Start()
        {
            UIParent.SetActive(false);
            if (!PlayerPrefs.HasKey(_masterVolumeKey)) PlayerPrefs.SetFloat(_masterVolumeKey, 1f);
            if (!PlayerPrefs.HasKey(_sfxVolumeKey)) PlayerPrefs.SetFloat(_sfxVolumeKey, 1f);
            if (!PlayerPrefs.HasKey(_musicVolumeKey)) PlayerPrefs.SetFloat(_musicVolumeKey, 1f);

            MasterSlider.value = PlayerPrefs.GetFloat(_masterVolumeKey);
            SFXSlider.value = PlayerPrefs.GetFloat(_sfxVolumeKey);
            MusicSlider.value = PlayerPrefs.GetFloat(_musicVolumeKey);

            SetAudioLevel("Master", MasterSlider.value);
            SetAudioLevel("SFX", SFXSlider.value, LowestVolume, HighestSFXVolume);
            SetAudioLevel("Music", MusicSlider.value, LowestVolume, HighestMusicVolume);
            SetAudioLevel("MusicSpirit", MusicSlider.value, LowestVolume, HighestMusicVolume);

            MasterSlider.onValueChanged.AddListener(newVal =>
            {
                PlayerPrefs.SetFloat(_masterVolumeKey, newVal);
                SetAudioLevel("Master", newVal);
            });

            SFXSlider.onValueChanged.AddListener(newVal =>
            {
                PlayerPrefs.SetFloat(_sfxVolumeKey, newVal);
                SetAudioLevel("SFX", newVal, LowestVolume, HighestSFXVolume);
            });

            MusicSlider.onValueChanged.AddListener(newVal =>
            {
                PlayerPrefs.SetFloat(_musicVolumeKey, newVal);
                SetAudioLevel("Music", newVal, LowestVolume, HighestMusicVolume);
                SetAudioLevel("MusicSpirit", newVal, LowestVolume, HighestMusicVolume);
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleUIParent();
            }
        }

        private void ToggleUIParent()
        {
            if (UIParent.activeInHierarchy == true)
            {
                Close();
            }
            else
            {
                UIParent.SetActive(true);
            }
        }

        public void Close() => UIParent.SetActive(false);

        public void Exit() => Application.Quit();

        private void SetAudioLevel(string mixerGroupName, float newValue, float overrideLowest = float.MinValue, float overrideHighest = float.MaxValue)
        {
            float lowest = (overrideLowest == float.MinValue ? LowestVolume : overrideLowest);
            float highest = (overrideHighest == float.MaxValue ? HighestVolume : overrideHighest);
            float adjustedScale = MapValueToNewScale(newValue, 0f, 1f, lowest, highest);
            GameManager.Instance.MainMixer.SetFloat(mixerGroupName, adjustedScale);
        }

        /// <summary>
        /// Utility function to map a value from one range onto another.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="min">The minimum of the value's current range.</param>
        /// <param name="max">The maximum of the value's current range.</param>
        /// <param name="targetMin">The minimum of the value's target range.</param>
        /// <param name="targetMax">The maximum of the value's target range.</param>
        /// <returns>The value mapped onto the target range.</returns>
        public static float MapValueToNewScale(float value, float min, float max, float targetMin, float targetMax)
            => (value - min) * ((targetMax - targetMin) / (max - min)) + targetMin;
    }
}