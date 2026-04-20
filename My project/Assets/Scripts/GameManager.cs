using TMPro;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField, Min(0f)] private float playDuration = 60f;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text endGameText;
    [SerializeField] private bool freezeTimeOnEnd = true;

    private float remainingTime;
    private bool isGameEnded;

    public bool IsGameEnded => isGameEnded;

    private void Awake()
    {
        Time.timeScale = 1f;
        remainingTime = playDuration;

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
            EndGame();
        }
    }

    private void EndGame()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;
        remainingTime = 0f;
        UpdateCountdownText();

        if (endGameText != null)
        {
            endGameText.text = "End Game";
            endGameText.gameObject.SetActive(true);
        }

        if (freezeTimeOnEnd)
        {
            Time.timeScale = 0f;
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            countdownText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
        }
    }
}
