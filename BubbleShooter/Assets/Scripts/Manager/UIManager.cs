using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;

    private Dictionary<int, MonoBehaviour> loadedUIs = new Dictionary<int, MonoBehaviour>();

    public Transform GetUIParent()
    {
        return uiCanvas ? uiCanvas.transform : transform;
    }

    public T GetUI<T>(UIType uiType) where T : MonoBehaviour
    {
        if (loadedUIs.TryGetValue((int)uiType, out var uiComponent))
        {
            if (!uiComponent)
            {
                loadedUIs.Remove((int)uiType);
                return null;
            }

            return uiComponent as T;
        }

        return null;
    }

    public T OpenUI<T>(UIType uiType) where T : MonoBehaviour
    {
        var ui = GetUI<T>(uiType);

        if (ui) return ui;

        string path = $"UI/{uiType}";
        var uiPrefab = Resources.Load<GameObject>(path);

        if (uiPrefab == null)
        {
            Debug.LogError($"Not found UI: {path}");
            return null;
        }

        var uiObj = Instantiate(uiPrefab, GetUIParent());
        var uiComponent = uiObj.GetComponent<T>();

        uiObj.transform.localPosition = Vector3.zero;

        loadedUIs[(int)uiType] = uiComponent;

        return uiComponent;
    }

    public void CloseUI(UIType uiType)
    {
        var ui = GetUI<MonoBehaviour>(uiType);

        if (!ui) return;

        loadedUIs.Remove((int)uiType);
        DestroyImmediate(ui.gameObject);
    }

    public void CloseAllUI()
    {
        var uiKeys = loadedUIs.Keys.ToList();

        foreach (var uiKey in uiKeys)
        {
            CloseUI((UIType)uiKey);
        }
    }
}
