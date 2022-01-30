using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SCEditor
{
    [CustomEditor(typeof(JGDT.Audio.Crossfade.Crossfade))]
    public class CrossfadeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Play")) ((JGDT.Audio.Crossfade.Crossfade)target).Play();
                if (GUILayout.Button("Pause")) ((JGDT.Audio.Crossfade.Crossfade)target).Pause();
                if (GUILayout.Button("Crossfade")) ((JGDT.Audio.Crossfade.Crossfade)target).Fade();
            }
        }
    }
}
