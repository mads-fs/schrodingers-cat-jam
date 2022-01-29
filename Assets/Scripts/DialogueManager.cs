using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SC
{
    public class DialogueManager : MonoBehaviour
    {
        public GameObject DialogueCanvas;

        private List<GameObject> _triggers;

        private void Start()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            _triggers = new List<GameObject>();
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
                    // play dialogue
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
    }
}
