using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Keycard Security")]
    // Shows as a clean dropdown in the Unity Inspector
    public KeycardLevel CurrentHighestCard = KeycardLevel.None;

    [Header("Special Items")]
    [SerializeField] private int simpleKeyCount = 0; // Tracks how many simple keys you have

    // --- NEW: Sound Effect Fields ---
    [Header("Audio Clips")]
    [Tooltip("Sound played when the player discovers and upgrades a security keycard")]
    [SerializeField] private AudioClip keycardFoundSound;
    [Tooltip("Sound played when the player finds a consumable simple key")]
    [SerializeField] private AudioClip simpleKeyFoundSound;

    public void UpgradeKeycard(KeycardLevel newLevel)
    {
        if (newLevel > CurrentHighestCard)
        {
            CurrentHighestCard = newLevel;
            Debug.Log($"Inventory Updated! Highest card is now: {CurrentHighestCard}");
            
            // --- NEW: Play the keycard discovery sound at the player's position ---
            PlayDiscoverySound(keycardFoundSound);

            // UI Hooks: Send text notification and refresh icon color
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"Security Access Upgraded: Level {CurrentHighestCard}!");
                GameUIManager.Instance.UpdateKeycardUI(CurrentHighestCard, null);
            }
        }
    }

    // Call this when the player searches the cabinet or washing machine
    public void AddSimpleKey()
    {
        simpleKeyCount++;
        Debug.Log($"Collected a Simple Key! Total keys: {simpleKeyCount}");

        // --- NEW: Play the simple key discovery sound at the player's position ---
        PlayDiscoverySound(simpleKeyFoundSound);

        // UI Hooks: Light up icon and display top-left text log
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateSimpleKeyUI(true);
            GameUIManager.Instance.DisplayNotification("Collected a Simple Key!");
        }
    }

    // --- NEW: Audio helper function ---
    private void PlayDiscoverySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    // Call this to check if the player can open a simple lock
    public bool HasSimpleKey()
    {
        return simpleKeyCount > 0;
    }

    // Call this immediately after successfully unlocking a simple lock
    public void ConsumeSimpleKey()
    {
        if (simpleKeyCount > 0)
        {
            simpleKeyCount--;
            Debug.Log($"Used a Simple Key. Keys remaining: {simpleKeyCount}");

            // UI Hooks: Dim icon if out of keys, send use alert text
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateSimpleKeyUI(simpleKeyCount > 0);
                GameUIManager.Instance.DisplayNotification("Used a Simple Key.");
            }
        }
    }
}