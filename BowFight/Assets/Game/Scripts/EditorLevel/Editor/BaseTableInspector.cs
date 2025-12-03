using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;

[CustomEditor(typeof(BaseTable), true)]
public class BaseTableInspector : Editor
{
    private BaseTable baseTable;
    private Type dataType;
    private MethodInfo getDataCountMethod;
    private MethodInfo getAllDatasMethod;

    // UI 상태
    private bool mainFoldout = true;
    private List<bool> itemFoldouts = new List<bool>();
    private bool isDirty = false;

    private void OnEnable()
    {
        baseTable = (BaseTable)target;

        // 제네릭 타입 정보 추출
        Type baseType = baseTable.GetType();
        while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(BaseTable<>)))
        {
            baseType = baseType.BaseType;
        }

        if (baseType != null)
        {
            dataType = baseType.GetGenericArguments()[0];
            getDataCountMethod = baseType.GetMethod("GetDataCount");
            getAllDatasMethod = baseType.GetMethod("GetAllDatas");
        }

        RefreshItemFoldouts();
    }

    private void RefreshItemFoldouts()
    {
        int itemCount = GetItemCount();

        while (itemFoldouts.Count < itemCount)
            itemFoldouts.Add(false);

        while (itemFoldouts.Count > itemCount)
            itemFoldouts.RemoveAt(itemFoldouts.Count - 1);
    }

    private int GetItemCount()
    {
        if (getDataCountMethod != null)
            return (int)getDataCountMethod.Invoke(baseTable, null);
        return 0;
    }

    private System.Collections.IList GetAllItems()
    {
        if (getAllDatasMethod != null)
            return (System.Collections.IList)getAllDatasMethod.Invoke(baseTable, null);
        return null;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawHeader();
        DrawMainContent();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
            isDirty = false;
        }
    }

    private new void DrawHeader()
    {
        GUILayout.BeginVertical("box");
        {
            string tableName = dataType != null ? $"{dataType.Name} Table" : "Data Table";
            EditorGUILayout.LabelField($"📦 {tableName} Manager", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"Total Items: {GetItemCount()}");

                GUILayout.FlexibleSpace();

                // 새 아이템 생성 버튼
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("+ New Data", GUILayout.Width(80)))
                {
                    CreateNewItem();
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private void DrawMainContent()
    {
        var indices = GetIndices();

        GUILayout.BeginVertical("box");
        {
            mainFoldout = EditorGUILayout.Foldout(mainFoldout, $"📋 Items ({indices.Count}/{GetItemCount()})");

            if (mainFoldout)
            {
                if (indices.Count == 0)
                {
                    EditorGUILayout.HelpBox("No items match the current filter.", MessageType.Info);
                }
                else
                {
                    foreach (int index in indices)
                    {
                        DrawItemElement(index);
                    }
                }

                EditorGUILayout.Space(5);
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawItemElement(int index)
    {
        var items = GetAllItems();
        if (items == null || index >= items.Count) return;

        var item = items[index] as BaseTableData;

        // 색상 설정
        Color originalColor = GUI.backgroundColor;
        if (item == null)
            GUI.backgroundColor = Color.yellow;

        GUILayout.BeginVertical("window");
        {
            GUI.backgroundColor = originalColor;

            GUILayout.BeginHorizontal();
            {
                // 폴드아웃과 아이템 정보
                string itemName = GetItemDisplayName(item);
                string itemId = item != null ? $"[ID: {item.ID}]" : "[No ID]";
                string statusIcon = GetStatusIcon(item);

                itemFoldouts[index] = EditorGUILayout.Foldout(
                    itemFoldouts[index],
                    $"{statusIcon} {itemName} {itemId}");

                GUILayout.FlexibleSpace();

                // 삭제 버튼
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("✖", GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (EditorUtility.DisplayDialog("Delete Data",
                        $"Delete data '{itemName}'?", "Delete", "Cancel"))
                    {
                        DeleteItem(index);
                        return;
                    }
                }
                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();

            // 아이템 세부 정보
            if (itemFoldouts[index])
            {
                DrawItemDetails(index);
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawItemDetails(int index)
    {
        var items = GetAllItems();
        if (items == null || index >= items.Count) return;

        var item = items[index] as BaseTableData;
        if (item == null)
        {
            EditorGUILayout.HelpBox("Data is null. Delete this entry.", MessageType.Warning);
            return;
        }

        EditorGUI.indentLevel++;

        GUI.color = Color.white;
        bool enterChildren = true;
        bool modifyData = false;

        GUILayout.BeginHorizontal();
        {
            uint newId = (uint)EditorGUILayout.IntField("ID", (int)item.ID);

            if (newId != item.ID && newId != 0)
            {
                Undo.RecordObject(item, "Change Data ID");
                SetItemID(item, newId);
                modifyData = true;
                EditorUtility.SetDirty(item);
            }
        }
        GUILayout.EndHorizontal();
        GUI.color = Color.white;

        EditorGUILayout.Space(5);

        // 나머지 필드들은 기본 PropertyField로 표시
        EditorGUILayout.LabelField("Data Properties", EditorStyles.boldLabel);

        var serializedItem = new SerializedObject(item);
        var iterator = serializedItem.GetIterator();

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // ID 필드는 이미 위에서 처리했으므로 스킵
            if (iterator.name == "_id" || iterator.name == "m_Script")
                continue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(iterator, true);

            if (EditorGUI.EndChangeCheck())
            {
                modifyData = true;
            }
        }

        serializedItem.ApplyModifiedProperties();

        if (modifyData)
            SetItemName(item);

        EditorGUI.indentLevel--;
    }

    private string GetItemDisplayName(BaseTableData item)
    {
        if (item == null) return "Null Data";

        var field = dataType.GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(string))
        {
            var value = (string)field.GetValue(item);
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        return $"{dataType.Name}";
    }

    private string GetStatusIcon(BaseTableData item)
    {
        if (item == null) return "❌";
        return "✅";
    }

    private List<int> GetIndices()
    {
        var items = GetAllItems();
        if (items == null) return new List<int>();

        var indices = new List<int>();

        for (int i = 0; i < items.Count; i++)
        {
            indices.Add(i);
        }

        return indices;
    }

    private void CreateNewItem()
    {
        if (dataType == null) return;

        var items = GetAllItems();
        var newItem = CreateInstance(dataType) as BaseTableData;

        uint id = (uint)(items != null ? items.Count + 1 : 0);
        string name = $"{dataType.Name.Replace("TableData", "")}";

        newItem.SetID(id);
        newItem.SetName(name);

        // 이름 기본값 설정
        SetItemName(newItem, false);

        // Sub-Asset으로 추가
        AssetDatabase.AddObjectToAsset(newItem, baseTable);

        // 목록에 추가
        items?.Add(newItem);

        RefreshItemFoldouts();

        EditorUtility.SetDirty(baseTable);
        EditorUtility.SetDirty(newItem);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created new {dataType.Name}: {newItem.name}");
    }

    private void SetItemName(BaseTableData data, bool isSave = true)
    {
        if (data == null) return;

        string id = data.ID >= 100 ? data.ID.ToString()
        : data.ID >= 10 ? $"0{data.ID}"
        : $"00{data.ID}";
        string name = $"{id}_{data.Name}";

        data.name = name;

        if (isSave)
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
    }

    private void DeleteItem(int index)
    {
        var items = GetAllItems();
        if (items == null || index < 0 || index >= items.Count) return;

        var item = items[index] as BaseTableData;
        if (item != null)
        {
            DestroyImmediate(item, true);
        }

        items.RemoveAt(index);
        RefreshItemFoldouts();

        EditorUtility.SetDirty(baseTable);
        AssetDatabase.SaveAssets();
    }

    private void SetItemID(BaseTableData item, uint newId)
    {
        // BaseTableData의 protected id 필드를 리플렉션으로 설정
        var field = typeof(BaseTableData).GetField("_id",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(item, newId);
    }
}