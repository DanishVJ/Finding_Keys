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

    // --- FIXED: Labeled exactly by card color ---
    [Header("Keycard Graphics Assets")]
    [SerializeField] private Sprite blueCardSprite;
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private Sprite purpleCardSprite;
    [SerializeField] private Sprite silverCardSprite;
    [SerializeField] private Sprite goldCardSprite;
    
    [Header("Top Left Notifications")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3f;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HidePrompt();
        UpdateSimpleKeyUI(false);
    }

    private void Start()
    {
        DisplayNotification("Finding Keys: Trap the alien in the last room!");
    }

    public void ShowPrompt(string message)
    {
        interactionPromptText.gameObject.SetActive(true);
        interactionPromptText.text = message;
    }

    public void HidePrompt()
    {
        interactionPromptText.gameObject.SetActive(false);
    }

    public void UpdateSimpleKeyUI(bool hasKey)
    {
        simpleKeyIcon.color = hasKey ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
    }

    // --- FIXED: Switch statement matches your actual enum color names ---
    public void UpdateKeycardUI(KeycardLevel level, Sprite fallbackSprite)
    {
        Sprite targetSprite = null;

        switch (level)
        {
            case KeycardLevel.None:
                keycardIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                keycardIcon.sprite = null;
                return; // Exit out early since we don't have a card

            case KeycardLevel.Blue:
                targetSprite = blueCardSprite;
                break;
            case KeycardLevel.Red:
                targetSprite = redCardSprite;
                break;
            case KeycardLevel.Purple:
                targetSprite = purpleCardSprite;
                break;
            case KeycardLevel.Silver:
                targetSprite = silverCardSprite;
                break;
            case KeycardLevel.Gold:
                targetSprite = goldCardSprite;
                break;
        }

        // Apply the chosen graphic to the single UI slot on your Canvas
        if (targetSprite != null)
        {
            keycardIcon.color = Color.white;
            keycardIcon.sprite = targetSprite; 
        }
        else
        {
            Debug.LogWarning($"GameUIManager: Sprite field for {level} is unassigned in the inspector!");
        }
    }

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