using UnityEngine;

public class IntroScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.SetActiveDim(false);

        var introUI = GameManager.Instance.UIManager.OpenUI<IntroUI>(UIType.IntroUI);
        introUI.SetEventLoadScene(OnEventLoadScene);
    }

    private void OnEventLoadScene()
    {
        var introUI = GameManager.Instance.UIManager.GetUI<IntroUI>(UIType.IntroUI);

        if (introUI)
            introUI.SetEventLoadScene(null);

        GameManager.Instance.UIManager.CloseAllUI();
    }
}
