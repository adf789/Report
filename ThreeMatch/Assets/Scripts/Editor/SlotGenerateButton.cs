using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SlotGenerator))]
public class SlotGenerateButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SlotGenerator generator = (SlotGenerator)target;
        if(GUILayout.Button("슬롯 생성_위"))
        {
            generator.Generate(Slot.Direction.Up);
        }
        if (GUILayout.Button("슬롯 생성_오른쪽_위"))
        {
            generator.Generate(Slot.Direction.Up_Right);
        }
        if (GUILayout.Button("슬롯 생성_오른쪽_아래"))
        {
            generator.Generate(Slot.Direction.Down_Right);
        }
        if (GUILayout.Button("슬롯 생성_아래"))
        {
            generator.Generate(Slot.Direction.Down);
        }
        if (GUILayout.Button("슬롯 생성_왼쪽_아래"))
        {
            generator.Generate(Slot.Direction.Down_Left);
        }
        if (GUILayout.Button("슬롯 생성_왼쪽_위"))
        {
            generator.Generate(Slot.Direction.Up_Left);
        }
    }
}
