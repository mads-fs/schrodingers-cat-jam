using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SC
{
    public class DialogueManager : MonoBehaviour
    {
        public event EventHandler OnDialogueStart;
        public event EventHandler OnDialogueAdvance;
        public event EventHandler OnDialogueEnd;

        [Tooltip("Drag a chain here before pressing 'Test Play' to test this chain.")]
        public DialogueChain TestChain;
        public GameObject DialogueCanvas;
        public GameObject DialogueParent;
        public TMP_Text TextBox;

        public bool IsPlayingDialogue { get; private set; } = false;

        private List<GameObject> _triggers;
        private DialogueChain _currentChain = null;
        private int _chainIndex = 0;

        private void Start()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            _triggers = new List<GameObject>();
            DialogueParent.SetActive(false);
            _triggers.Clear();
            DialogueTrigger[] triggers = FindObjectsOfType<DialogueTrigger>();
            foreach (DialogueTrigger trigger in triggers) _triggers.Add(trigger.gameObject);
        }

        private void Update()
        {
            if (IsPlayingDialogue == true)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    Next();
                }
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _triggers.Clear();
            DialogueTrigger[] triggers = FindObjectsOfType<DialogueTrigger>();
            foreach (DialogueTrigger trigger in triggers) _triggers.Add(trigger.gameObject);
        }

        public void PlayDialogue(GameObject trigger, int worldState)
        {
            if (_triggers.Contains(trigger))
            {
                DialogueChain chain = trigger.GetComponent<DialogueTrigger>().GetDialogue(worldState);
                if (chain != null)
                {
                    _currentChain = chain;
                    StartDialogue(chain.NewWorldState);
                }
                else
                {
                    Debug.LogWarning($"GetDialogue() was called, but no chain matches worldstate: {worldState}");
                }
            }
            else
            {
                Debug.LogError($"Invalid {typeof(DialogueTrigger)} given in {typeof(DialogueManager)}.PlayDialogue() ({trigger.name})");
            }
        }

        public void ReplayLast()
        {
            if (_currentChain != null)
            {
                StartDialogue(-1);
            }
            else
            {
                Debug.LogWarning("No chain has been played yet.");
            }
        }

        public void PlayTest()
        {
            if (TestChain != null)
            {
                _currentChain = TestChain;
                StartDialogue(-1);
            }
            else
            {
                Debug.LogWarning("Please provide a chain in the TestChain field before pressing Test Play");

            }
        }

        private void StartDialogue(int newWorldState)
        {
            _chainIndex = 0;
            TextBox.text = "";
            IsPlayingDialogue = true;
            DialogueParent.SetActive(true);
            OnDialogueStart?.Invoke(this, null);
            if(newWorldState != -1) GameManager.Instance.AdvanceWorldState(newWorldState);
            Next();
        }

        private void Next()
        {
            if (_chainIndex >= _currentChain.DialogueList.Count)
            {
                StopDialogue();
            }
            else
            {
                string text = _currentChain.DialogueList[_chainIndex].Content;
                TextBox.text = text;
                _chainIndex += 1;
                OnDialogueAdvance?.Invoke(this, null);
            }
        }

        public void StopDialogue()
        {
            IsPlayingDialogue = false;
            OnDialogueEnd?.Invoke(this, null);
            DialogueParent.SetActive(false);
        }
    }
}
