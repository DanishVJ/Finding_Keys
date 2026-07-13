using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Center Interaction Prompt")]
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    [Header("Top Right Inventory Icons")]
    [SerializeField] private Image keycardIcon;
    [SerializeField] private Image simpleKeyIcon;
    
    [Header("Top Left Notifications")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3f;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        // Simple singleton pattern so any script can easily find the UI Manager
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Hide prompt and key icon at startup
        HidePrompt();
        UpdateSimpleKeyUI(false);
    }

    // --- 1. INTERACTION PROMPT FUNCTIONS ---
    public void ShowPrompt(string message)
    {
        interactionPromptText.gameObject.SetActive(true);
        interactionPromptText.text = message;
    }

    public void HidePrompt()
    {
        interactionPromptText.gameObject.SetActive(false);
    }

    // --- 2. INVENTORY DISPLAY FUNCTIONS ---
    public void UpdateSimpleKeyUI(bool hasKey)
    {
        // If the player doesn't have it, make it semi-transparent gray. If they do, make it solid white.
        simpleKeyIcon.color = hasKey ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
    }

    public void UpdateKeycardUI(KeycardLevel level, Sprite cardSprite)
    {
        if (level == KeycardLevel.None)
        {
            keycardIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            keycardIcon.sprite = null;
        }
        else
        {
            keycardIcon.color = Color.white;
            keycardIcon.sprite = cardSprite; // Your partner can swap these sprites later!
        }
    }

    // --- 3. TOP LEFT NOTIFICATION LOG ---
    public void DisplayNotification(string message)
    {
        if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
        notificationCoroutine = StartCoroutine(RunNotification(message));
    }

    private IEnumerator RunNotification(string message)
    {
        notificationText.text = message;
        yield return new WaitForSeconds(notificationDuration);
        notificationText.text = "";
    }
}