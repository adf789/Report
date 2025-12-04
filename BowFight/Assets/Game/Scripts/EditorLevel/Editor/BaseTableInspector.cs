using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;
using System.Collections;

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
    private List<Texture2D> viewTextures = new List<Texture2D>();

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
        InitialzeSprites();
    }

    private void RefreshItemFoldouts()
    {
        int itemCount = GetItemCount();

        while (itemFoldouts.Count < itemCount)
            itemFoldouts.Add(false);

        while (itemFoldouts.Count > itemCount)
            itemFoldouts.RemoveAt(itemFoldouts.Count - 1);
    }

    private void RefreshViewTextures()
    {
        int itemCount = GetItemCount();

        while (viewTextures.Count < itemCount)
            viewTextures.Add(null);

        while (viewTextures.Count > itemCount)
            viewTextures.RemoveAt(viewTextures.Count - 1);
    }

    private void InitialzeSprites()
    {
        foreach (var item in GetAllItems())
        {
            if (item is not BaseTableData data)
                break;

            var serializedItem = new SerializedObject(data);
            var thumbnailProperty = serializedItem.FindProperty("_thumbnail");
            var path = thumbnailProperty.stringValue;

            if (string.IsNullOrEmpty(path))
            {
                viewTextures.Add(null);
            }
            else
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Game/Resources/{path}.png");
                viewTextures.Add(texture);
            }
        }
    }

    private int GetItemCount()
    {
        if (getDataCountMethod != null)
            return (int)getDataCountMethod.Invoke(baseTable, null);
        return 0;
    }

    private IList GetAllItems()
    {
        if (getAllDatasMethod != null)
            return (IList)getAllDatasMethod.Invoke(baseTable, null);
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
        GUILayout.BeginVertical("box");
        {
            mainFoldout = EditorGUILayout.Foldout(mainFoldout, $"📋 Items ({GetItemCount()})");

            if (mainFoldout)
            {
                int index = 0;
                foreach (var item in GetAllItems())
                {
                    DrawItemElement(item as BaseTableData, index++);
                }

                EditorGUILayout.Space(5);
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawItemElement(BaseTableData data, int index)
    {
        // 색상 설정
        Color originalColor = GUI.backgroundColor;
        if (data == null)
            GUI.backgroundColor = Color.yellow;

        GUILayout.BeginVertical("window");
        {
            GUI.backgroundColor = originalColor;

            GUILayout.BeginHorizontal();
            {
                // 폴드아웃과 아이템 정보
                string itemName = GetItemDisplayName(data);
                string itemId = data != null ? $"[ID: {data.ID}]" : "[No ID]";
                string statusIcon = GetStatusIcon(data);

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
                DrawItemDetails(data, index);
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawItemDetails(BaseTableData data, int index)
    {
        if (data == null)
        {
            EditorGUILayout.HelpBox("Data is null. Delete this entry.", MessageType.Warning);
            return;
        }

        EditorGUI.indentLevel++;

        GUI.color = Color.white;
        bool enterChildren = true;
        bool modifyData = false;

        // 데이터 활성화 유무
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Active");
            bool isActive = EditorGUILayout.Toggle(data.IsActive);

            if (isActive != data.IsActive)
            {
                SetItemActive(data, isActive);
                modifyData = true;
            }
        }
        GUILayout.EndHorizontal();

        // ID 필드
        GUILayout.BeginHorizontal();
        {
            uint newId = (uint)EditorGUILayout.IntField("ID", (int)data.ID);

            if (newId != data.ID && newId != 0)
            {
                Undo.RecordObject(data, "Change Data ID");
                SetItemID(data, newId);
                modifyData = true;
            }
        }
        GUILayout.EndHorizontal();
        GUI.color = Color.white;

        EditorGUILayout.Space(5);

        // 썸네일 필드
        Texture2D texture = (Texture2D)EditorGUILayout.ObjectField("Thumbnail", viewTextures[index], typeof(Texture2D), false);
        if (texture != viewTextures[index])
        {
            SetThumbnail(data, texture);
            viewTextures[index] = texture;
            modifyData = true;
        }

        EditorGUILayout.Space(5);

        // 나머지 필드들은 기본 PropertyField로 표시
        EditorGUILayout.LabelField("Data Properties", EditorStyles.boldLabel);

        var serializedItem = new SerializedObject(data);
        var iterator = serializedItem.GetIterator();

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // ID 필드는 이미 위에서 처리했으므로 스킵
            if (iterator.name == "_id"
            || iterator.name == "m_Script"
            || iterator.name == "_thumbnail"
            || iterator.name == "_isActive")
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
            SetItemName(data);

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
        if (item == null || !item.IsActive) return "❌";
        return "✅";
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
        RefreshViewTextures();

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
        RefreshViewTextures();

        EditorUtility.SetDirty(baseTable);
        AssetDatabase.SaveAssets();
    }

    private void SetItemActive(BaseTableData item, bool isActive)
    {
        // BaseTableData의 protected id 필드를 리플렉션으로 설정
        var field = typeof(BaseTableData).GetField("_isActive",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(item, isActive);
    }

    private void SetItemID(BaseTableData item, uint newId)
    {
        // BaseTableData의 protected id 필드를 리플렉션으로 설정
        var field = typeof(BaseTableData).GetField("_id",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(item, newId);
    }

    private void SetThumbnail(BaseTableData item, Texture2D texture)
    {
        // BaseTableData의 protected id 필드를 리플렉션으로 설정
        var field = typeof(BaseTableData).GetField("_thumbnail",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var path = AssetDatabase.GetAssetPath(texture);

        if (!string.IsNullOrEmpty(path))
            path = path.Replace("Assets/Game/Resources/", "").Replace(".png", "");

        field?.SetValue(item, path);
    }
}