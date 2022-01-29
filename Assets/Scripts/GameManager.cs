using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SC
{
    public class GameManager : MonoBehaviour
    {
        [Min(0), Tooltip("How many times the cat can go to the spirit realm and back. 0 for infinite.")]
        public int Lives = 0;
        public ControllerType @ControllerType = ControllerType.Keyboard;
        public readonly int WorldState = 0;

        private int _maxLives = 0;
        private CatController _player;
        private DialogueManager _dialogueManager;

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
            if (!_player)
            {
                Debug.LogError($"No {typeof(CatController)} found. Abort.");
                Destroy(this);
            }
            else
            {
                Instance._maxLives = Lives;
                SubscribeToPlayer();
            }
        }

        private void Update()
        {
            switch (@ControllerType)
            {
                case ControllerType.Keyboard:
                    HandleKeyboardInput();
                    break;
                case ControllerType.XboxOne:
                    HandleXboxOneInput();
                    break;
            }
        }

        private void SubscribeToPlayer()
        {
            Instance._player.OnAlived += Player_OnAlived;
            Instance._player.OnUnalived += Player_OnUnAlived;
        }

        /// <summary>
        /// Plays Dialogue using worldstate
        /// </summary>
        public void PlayDialogue(GameObject trigger) => _dialogueManager.PlayDialogue(trigger, -1);
        /// <summary>
        /// Plays Dialogue using provided state
        /// </summary>
        public void PlayDialogue(GameObject trigger, int state) => _dialogueManager.PlayDialogue(trigger, state);

        #region Controller Handling
        private void HandleKeyboardInput()
        {

        }

        private void HandleXboxOneInput()
        {

        }
        #endregion

        #region OnAlived
        private void Player_OnAlived(object sender, System.EventArgs e)
        {
            Debug.Log("Alived!");
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
                    TransitionToDeathRealm();
                }
            }
            TransitionToDeathRealm();
        }

        private void GameOver()
        {
            Debug.LogWarning("Game Over!");
        }

        private void TransitionToDeathRealm()
        {
            Debug.Log("Transition To Death Realm.");
        }
        #endregion
    }
}