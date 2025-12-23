using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PuzzleCreator))]
public class PuzzleGenerateButton : Editor
{
    float obstacleRate;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PuzzleCreator creator = (PuzzleCreator)target;
        if(GUILayout.Button("½½·Ô »ö±ò ·£´ý"))
        {
            creator.AllSlotsRandomColor();
        }
        obstacleRate = EditorGUILayout.Slider(obstacleRate, 0, 1);
        if(GUILayout.Button("Àå¾Ö¹° »ý¼º"))
        {
            creator.PlaceRandomObstacle(obstacleRate);
        }
    }
}
