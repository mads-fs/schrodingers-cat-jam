using JGDT.Audio.Crossfade;
using JGDT.Audio.OneShot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace SC
{
    public class GameManager : MonoBehaviour
    {
        [Min(0), Tooltip("How many times the cat can go to the spirit realm and back. 0 for infinite.")]
        public int Lives = 0;
        public float TransitionLerpTime = 3f;
        public int WorldState { get; private set; } = 0;
        [Header("Camera")]
        public float CameraLerpTime = 1.5f;
        public Volume GlobalVolume;
        public VolumeProfile RealWorldProfile;
        public VolumeProfile SpiritWorldProfile;
        [Header("Audio")]
        public AudioMixer MainMixer;
        public GameObject TransitionAudioPrefab;
        public GameObject TransitionAudioPrefabReverse;
        [Header("Spirit Realm")]
        public GameObject SpiritCat;

        private int _maxLives = 0;
        private CatController _player;
        private DialogueManager _dialogueManager;
        private UIManager _uiManager;
        private Camera _cam;
        private float _camDefaultFOV;
        private Vector3 _camDefaultPosition;
        private float _camSpiritCatFOV;
        private Vector3 _camSpiritCatPosition;
        private float _camBoxFOV;
        private Vector3 _camBoxPosition;

        private Coroutine _cameraMoveHandle;
        private bool _playerIsTransitioning = false;

        #region Singleton
        public static GameManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Only one object of type {GetType()} may exist. Destroying {gameObject}.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Instance._cam = Camera.main;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion

        private void Start()
        {
            Instance._player = FindObjectOfType<CatController>();
            Instance._dialogueManager = FindObjectOfType<DialogueManager>();
            Instance._uiManager = FindObjectOfType<UIManager>();
            if (!_player)
            {
                Debug.LogError($"No {typeof(CatController)} found. Abort.");
                Destroy(this);
            }
            else
            {
                Instance._maxLives = Lives;
                Instance._camDefaultPosition = _cam.transform.position;
                Instance._camDefaultFOV = _cam.fieldOfView;
                Instance._camSpiritCatFOV = 15f;
                Instance._camSpiritCatPosition = FindObjectsOfType<GameObject>().First(pred => pred.name == "SpiritCat").transform.position;
                // adjust for pivot location
                Instance._camSpiritCatPosition = new Vector3(Instance._camSpiritCatPosition.x, Instance._camSpiritCatPosition.y + 2f, _camDefaultPosition.z);
                Instance._camBoxFOV = 5f;
                Instance._camBoxPosition = FindObjectsOfType<GameObject>().First(pred => pred.name == "Box").transform.position;
                Instance._camBoxPosition = new Vector3(Instance._camBoxPosition.x, Instance._camBoxPosition.y + 0.25f, _camDefaultPosition.z);

                SubscribeToPlayer();
                SubscribeToDialogueManager();
                HideSpiritRealm();
            }
        }

        private void SubscribeToPlayer()
        {
            Instance._player.OnAlived += Player_OnAlived;
            Instance._player.OnUnalived += Player_OnUnAlived;
        }

        private void SubscribeToDialogueManager()
        {
            Instance._dialogueManager.OnDialogueEnd += DialogueManager_OnDialogueEnd;
        }

        private void HideSpiritRealm()
        {
            SpiritCat.SetActive(false);
        }

        public void AdvanceWorldState() => Instance.WorldState += 1;

        public void MoveCameraToDefault()
        {
            if (Instance._cam.transform.position != _camDefaultPosition)
            {
                if (_cameraMoveHandle != null) StopCoroutine(_cameraMoveHandle);
                _cameraMoveHandle =
                    StartCoroutine(DoCameraMove(Instance._cam.transform.position, Instance._camDefaultPosition, Instance._cam.fieldOfView, Instance._camDefaultFOV));
            }
        }

        public void MoveCameraToSpiritCat()
        {
            if (Instance._cam.transform.position != _camSpiritCatPosition)
            {
                if (_cameraMoveHandle != null) StopCoroutine(_cameraMoveHandle);
                _cameraMoveHandle =
                    StartCoroutine(DoCameraMove(Instance._cam.transform.position, Instance._camSpiritCatPosition, Instance._cam.fieldOfView, Instance._camSpiritCatFOV));
            }
        }

        public void MoveCameraToBox()
        {
            if (Instance._cam.transform.position != _camBoxPosition)
            {
                if (_cameraMoveHandle != null) StopCoroutine(_cameraMoveHandle);
                _cameraMoveHandle =
                    StartCoroutine(DoCameraMove(Instance._cam.transform.position, Instance._camBoxPosition, Instance._cam.fieldOfView, Instance._camBoxFOV, 2f));
            }
        }

        private IEnumerator DoCameraMove(Vector3 startPos, Vector3 endPos, float startFOV, float endFOV, float overrideLerpTime = 0f)
        {
            float lerpTime = (overrideLerpTime > 0 ? overrideLerpTime : CameraLerpTime);
            float time = 0f;
            while (time < lerpTime)
            {
                time += Time.deltaTime;
                float alpha = time / CameraLerpTime;
                Instance._cam.transform.position = Vector3.Slerp(startPos, endPos, alpha);
                Instance._cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, alpha);
                yield return null;
            }
            Instance._cam.transform.position = endPos;
            Instance._cam.fieldOfView = endFOV;
        }

        /// <summary>
        /// Plays Dialogue using worldstate
        /// </summary>
        public void PlayDialogue(GameObject trigger) => _dialogueManager.PlayDialogue(trigger, -1);
        /// <summary>
        /// Plays Dialogue using provided state
        /// </summary>
        public void PlayDialogue(GameObject trigger, int state) => _dialogueManager.PlayDialogue(trigger, state);

        #region OnAlived
        private void Player_OnAlived(object sender, System.EventArgs e)
        {
            TransitionToLivingRealm();
        }

        private void TransitionToLivingRealm()
        {
            _player.CanMove = false;
            _player.CanInteract = false;
            StartCoroutine(DoLivingTransition());
        }

        private IEnumerator DoLivingTransition()
        {
            _player.gameObject.SetActive(false);
            Crossfade crossFade = FindObjectOfType<Crossfade>();
            crossFade.Pause();
            Instantiate(TransitionAudioPrefabReverse, _camBoxPosition, Quaternion.identity);
            Vector3 startPos = Instance._cam.transform.position;
            Vector3 endPos = Instance._camBoxPosition;
            float startFOV = Instance._cam.fieldOfView;
            float endFOV = Instance._camBoxFOV;
            float time = 0f;

            while (time < Instance.TransitionLerpTime)
            {
                time += Time.deltaTime;
                float alpha = time / Instance.TransitionLerpTime;
                Instance._cam.transform.position = Vector3.Lerp(startPos, endPos, alpha);
                Instance._cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, alpha);
                yield return null;
            }

            Instance._cam.transform.position = endPos;
            Instance._cam.fieldOfView = endFOV;
            startPos = endPos;
            endPos = Instance._camDefaultPosition;
            startFOV = endFOV;
            endFOV = Instance._camDefaultFOV;
            time = 0f;

            crossFade.Play();
            crossFade.Fade();
            ShowLivingRealm();
            yield return new WaitForSeconds(2f);

            while (time < Instance.CameraLerpTime)
            {
                time += Time.deltaTime;
                float alpha = time / Instance.CameraLerpTime;
                Instance._cam.transform.position = Vector3.Lerp(startPos, endPos, alpha);
                Instance._cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, alpha);
                yield return null;
            }
            _player.gameObject.SetActive(true);
            _playerIsTransitioning = false;
            _player.CanMove = true;
            _player.CanInteract = true;
        }

        private void ShowLivingRealm()
        {
            SpiritCat.SetActive(false);
            GlobalVolume.profile = RealWorldProfile;
        }
        #endregion

        #region OnUnalived
        private void Player_OnUnAlived(object sender, System.EventArgs e)
        {
            // if MaxLives start at 0 then it can be done infinitely.
            if (Instance._maxLives != 0)
            {
                Instance.Lives -= 1;
                if (Instance.Lives == 0)
                {
                    GameOver();
                }
                else
                {
                    TransitionToSpiritRealm();
                }
            }
            TransitionToSpiritRealm();
        }

        private void TransitionToSpiritRealm()
        {
            _player.CanMove = false;
            _player.CanInteract = false;
            _playerIsTransitioning = true;
            StartCoroutine(DoSpiritTransition());
        }

        private IEnumerator DoSpiritTransition()
        {
            _player.gameObject.SetActive(false);
            Crossfade crossFade = FindObjectOfType<Crossfade>();
            crossFade.Pause();
            GameObject oneshotAudio = Instantiate(TransitionAudioPrefab, _camBoxPosition, Quaternion.identity);
            Vector3 startPos = Instance._cam.transform.position;
            Vector3 endPos = Instance._camBoxPosition;
            float startFOV = Instance._cam.fieldOfView;
            float endFOV = Instance._camBoxFOV;
            float time = 0f;

            while (time < Instance.TransitionLerpTime)
            {
                time += Time.deltaTime;
                float alpha = time / Instance.TransitionLerpTime;
                Instance._cam.transform.position = Vector3.Lerp(startPos, endPos, alpha);
                Instance._cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, alpha);
                yield return null;
            }

            Instance._cam.transform.position = endPos;
            Instance._cam.fieldOfView = endFOV;
            startPos = endPos;
            endPos = Instance._camDefaultPosition;
            startFOV = endFOV;
            endFOV = Instance._camDefaultFOV;
            time = 0f;
            
            crossFade.Play();
            crossFade.Fade();
            ShowSpiritRealm();
            yield return new WaitForSeconds(2f);

            while (time < Instance.CameraLerpTime)
            {
                time += Time.deltaTime;
                float alpha = time / Instance.CameraLerpTime;
                Instance._cam.transform.position = Vector3.Lerp(startPos, endPos, alpha);
                Instance._cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, alpha);
                yield return null;
            }
            _player.gameObject.SetActive(true);
            _playerIsTransitioning = false;
            _player.CanMove = true;
            _player.CanInteract = true;
        }

        private void ShowSpiritRealm()
        {
            GlobalVolume.profile = SpiritWorldProfile;
            SpiritCat.SetActive(true);
        }

        private void GameOver()
        {
            Debug.LogWarning("Game Over!");
        }
        #endregion

        #region DialogueManager Events
        private void DialogueManager_OnDialogueEnd(object sender, System.EventArgs e) => MoveCameraToDefault();
        #endregion
    }
}