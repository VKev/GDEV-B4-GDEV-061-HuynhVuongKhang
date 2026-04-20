using TMPro;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 10f;
    [SerializeField, Min(0f)] private float damagePerSecondWhileInDangerZone = 1f;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Collider2D playerCollider;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthText();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Awake()
    {
        ResolveReferences();
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthText();
    }

    private void Update()
    {
        if (isDead || (gameManager != null && gameManager.IsGameEnded))
        {
            return;
        }

        if (IsStandingInDangerZone())
        {
            TakeDamage(damagePerSecondWhileInDangerZone * Time.deltaTime);
        }
    }

    private bool IsStandingInDangerZone()
    {
        foreach (DangerZone dangerZone in DangerZone.ActiveZones)
        {
            if (dangerZone != null && dangerZone.Contains(transform, playerCollider))
            {
                return true;
            }
        }

        return false;
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (gameManager != null)
        {
            gameManager.NotifyPlayerDeath();
        }
    }

    private void ResolveReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (playerCollider == null)
        {
            TryGetComponent(out playerCollider);
        }

        if (healthText == null)
        {
            healthText = FindTextByGameObjectName("HpText");
        }

        if (healthText == null)
        {
            healthText = CreateHealthText();
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

    private static TMP_Text CreateHealthText()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            return null;
        }

        GameObject healthObject = new GameObject(
            "HpText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        healthObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = healthObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-24f, -24f);
        rectTransform.sizeDelta = new Vector2(180f, 50f);

        TextMeshProUGUI text = healthObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.TopRight;
        text.color = Color.white;
        text.fontSize = 36f;
        text.raycastTarget = false;

        return text;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }
}
