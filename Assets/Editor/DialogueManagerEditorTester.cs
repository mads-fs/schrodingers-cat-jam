using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SCEditor
{
    [CustomEditor(typeof(SC.DialogueManager))]
    public class DialogueManagerEditorTester : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Test Play")) ((SC.DialogueManager)target).PlayTest();
                if (GUILayout.Button("Replay Last")) ((SC.DialogueManager)target).ReplayLast();
            }
        }
    }
}