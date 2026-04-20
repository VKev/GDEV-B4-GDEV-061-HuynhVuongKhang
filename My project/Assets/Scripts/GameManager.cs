using TMPro;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField, Min(0f)] private float playDuration = 60f;
    [SerializeField, Min(1)] private int maxSoldierDeaths = 3;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private TMP_Text endGameText;
    [SerializeField] private string winMessage = "YOU WIN";
    [SerializeField] private string loseMessage = "YOU LOSE";
    [SerializeField] private bool freezeTimeOnEnd = true;

    private float remainingTime;
    private int soldierDeaths;
    private bool isGameEnded;

    public bool IsGameEnded => isGameEnded;

    private void Awake()
    {
        Time.timeScale = 1f;
        remainingTime = playDuration;
        soldierDeaths = 0;
        isGameEnded = false;

        ResolveReferences();

        if (winUI != null)
        {
            winUI.SetActive(false);
        }

        if (loseUI != null)
        {
            loseUI.SetActive(false);
        }

        if (endGameText != null)
        {
            endGameText.gameObject.SetActive(false);
        }

        UpdateCountdownText();
    }

    private void Update()
    {
        if (isGameEnded)
        {
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        UpdateCountdownText();

        if (remainingTime <= 0f)
        {
            Win();
        }
    }

    public void NotifySoldierDeath()
    {
        if (isGameEnded)
        {
            return;
        }

        soldierDeaths++;

        if (soldierDeaths >= maxSoldierDeaths)
        {
            Lose();
        }
    }

    public void NotifyPlayerDeath()
    {
        Lose();
    }

    private void Win()
    {
        EndGame(winUI, winMessage, true);
    }

    private void Lose()
    {
        EndGame(loseUI, loseMessage, false);
    }

    private void EndGame(GameObject stateUI, string message, bool timerCompleted)
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;
        remainingTime = timerCompleted ? 0f : Mathf.Max(0f, remainingTime);
        UpdateCountdownText();

        if (endGameText != null)
        {
            endGameText.text = message;
            endGameText.gameObject.SetActive(true);
        }

        if (stateUI != null)
        {
            stateUI.SetActive(true);
        }

        if (freezeTimeOnEnd)
        {
            Time.timeScale = 0f;
        }
    }

    private void ResolveReferences()
    {
        if (countdownText == null)
        {
            countdownText = FindTextByGameObjectName("GameDuration");
        }

        if (endGameText == null)
        {
            endGameText = FindTextByGameObjectName("EndGameText");
        }
    }

    private static TMP_Text FindTextByGameObjectName(string gameObjectName)
    {
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name == gameObjectName)
            {
                return text;
            }
        }

        return null;
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            countdownText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
        }
    }
}
