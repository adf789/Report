using UnityEngine;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject dim;

    public LevelManager LevelManager => levelManager;
    public UIManager UIManager => uiManager;

    private BubbleGrid bubbleGrid;
    private int currentScore;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.runInBackground = true;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void SetupGame()
    {
        Debug.Log("BubblePuzzle Game Started");

        // Reset score
        currentScore = 0;

        // Setup
        BubblePoolManager.Instance?.InitializePool();
        levelManager.StartGeneration();

        var gameUI = uiManager.OpenUI<GameUI>(UIType.GameUI);

        gameUI.SetEventWin(OnWinResult);
        gameUI.SetBossHp(levelManager.BossHp);
    }

    public void SetBubbleGrid(BubbleGrid bubbleGrid)
    {
        this.bubbleGrid = bubbleGrid;

        levelManager?.SetBubbleGrid(bubbleGrid);
    }

    public void SetActiveDim(bool isActive)
    {
        dim.SetActive(isActive);
    }

    /// <summary>
    /// Handle match scored
    /// </summary>
    public void OnMatchScored(int bubbleCount)
    {
        int matchScore = bubbleCount * IntDefine.BASE_MATCH_SCORE;
        currentScore += matchScore;

        var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
        gameUI?.UpdateScore(currentScore);
    }

    /// <summary>
    /// Handle bubbles fallen
    /// </summary>
    public void OnBubblesFallen(int bubbleCount)
    {
        int matchScore = bubbleCount * IntDefine.FALL_BONUS;
        currentScore += matchScore;

        var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
        gameUI?.UpdateScore(currentScore);
    }

    public void OnDamagedBoss(int damage)
    {
        bool isDeath = levelManager.BossHp.Damage(damage);
        var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);

        if (gameUI)
        {
            gameUI.UpdateBossHp(levelManager.BossHp);
        }
        else if (isDeath)
        {
            OnWinResult();
        }
    }

    public void OnDefeatResult()
    {
        OnShowResultPopup(false);
    }

    public void OnWinResult()
    {
        OnShowResultPopup(true);
    }

    private void OnShowResultPopup(bool isWin)
    {
        var resultPopup = uiManager.OpenUI<ResultPopup>(UIType.ResultPopup);

        if (resultPopup)
        {
            resultPopup.SetScore(currentScore);
            resultPopup.SetResult(isWin);
            resultPopup.SetEventResetGame(OnResetGame);
        }
    }

    private void OnResetGame()
    {
        foreach (var bubble in bubbleGrid.GetAllBubbles())
        {
            bubble.ReturnToPool();
        }

        SetBubbleGrid(null);
        SetActiveDim(true);

        uiManager.CloseAllUI();

        UnityEngine.SceneManagement.SceneManager.LoadScene($"Scenes/IntroScene");
    }
}
