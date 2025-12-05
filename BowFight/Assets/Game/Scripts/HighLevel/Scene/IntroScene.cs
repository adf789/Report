using UnityEngine;

public class IntroScene : MonoBehaviour
{
    void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }

    public void MoveToBattleScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene($"Game/Scenes/BattleScene");
    }
}
