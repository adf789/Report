using System.Collections;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    [SerializeField] private BubbleGrid bubbleGrid;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private DestructionHandler destructionHandler;

    void Start()
    {
        GameManager.Instance.SetBubbleGrid(bubbleGrid);
        GameManager.Instance.SetupGame();
        GameManager.Instance.SetActiveDim(false);

        destructionHandler.SetEventDamagedBoss(OnDamagedBoss);

        bubbleShooter.SetEventSacrificeBubble(OnSacrificeBubble);
        bubbleShooter.SetEventAfterShootCoroutine(ProcessGameLogic);
        bubbleShooter.Initialize(GameManager.Instance.LevelManager.BubbleShotCount);
    }

    /// <summary>
    /// Process game logic after bubble placement
    /// </summary>
    private IEnumerator ProcessGameLogic(Bubble placedBubble)
    {
        // Step 1: Check for matches
        var matches = GameLogic.MatchDetector.FindMatchingCluster(placedBubble, bubbleGrid);
        int matchCount = matches != null ? matches.Count : 0;

        if (matchCount > 0)
        {
            // Notify GameManager of match
            GameManager.Instance.OnMatchScored(matchCount);

            // Step 2: Destroy matched bubbles
            yield return destructionHandler.DestroyBubbles(matches);

            // Step 2-1: Check game end conditions
            if (GameManager.Instance.LevelManager.BossHp.IsDeath)
            {
                bubbleShooter.SetLock(true);
                yield break;
            }
            else if (bubbleShooter.RemainShotCount == 0)
            {
                GameManager.Instance.OnDefeatResult();
                bubbleShooter.SetLock(true);
                yield break;
            }

            // Step 3: Check for disconnected bubbles
            var disconnected = GameLogic.Gravity.GetDisconnectedBubbles(bubbleGrid);
            if (disconnected != null && disconnected.Count > 0)
            {
                // Step 4: Make them fall
                StartCoroutine(destructionHandler.MakeBubblesFall(disconnected, () =>
                {
                    GameManager.Instance?.OnBubblesFallen(disconnected.Count);
                }));
            }

            yield return GameManager.Instance.LevelManager.RegenerateIfNeeded();
        }
    }

    private void OnSacrificeBubble()
    {
        if (bubbleShooter.RemainShotCount == 0)
        {
            GameManager.Instance.OnDefeatResult();
            bubbleShooter.SetLock(true);
        }
    }

    private void OnDamagedBoss()
    {
        GameManager.Instance.OnDamagedBoss(1);
    }
}
