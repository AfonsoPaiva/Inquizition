using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Makedecision : MonoBehaviour
{
    [Header("UI References")]
    public GameObject decisionPanel;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI crimeText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    [Header("Button References - Assign in Inspector")]
    public Button executeButton;
    public Button exileButton;
    public Button forgiveButton;

    [Header("Character Management")]
    public GameObject characterPrefab;
    public Transform spawnPoint;

    [Header("Game References")]
    public camera_mov playerCamera;

    private Character currentCharacter;
    private GameObject currentCharacterInstance;
    private System.Random random = new System.Random();

    void Start()
    {
        // Setup button listeners in code (more scalable)
        if (executeButton != null)
            executeButton.onClick.AddListener(() => MakeDecision(DecisionType.Execute));
        if (exileButton != null)
            exileButton.onClick.AddListener(() => MakeDecision(DecisionType.Exile));
        if (forgiveButton != null)
            forgiveButton.onClick.AddListener(() => MakeDecision(DecisionType.Forgive));

        // Spawn the first character
        SpawnNewCharacter();
        UpdateStatsUI();
    }

    public void ShowDecision()
    {
        // Create a random character for this decision
        currentCharacter = GenerateRandomCharacter();

        // Update UI with character info
        if (characterNameText != null)
            characterNameText.text = currentCharacter.characterName;

        if (crimeText != null)
            crimeText.text = $"Crime: {currentCharacter.crime}";

        if (descriptionText != null)
            descriptionText.text = currentCharacter.description;

        if (decisionPanel != null)
            decisionPanel.SetActive(true);

        if (playerCamera != null)
            playerCamera.DisableLook();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideDecision()
    {
        if (decisionPanel != null)
            decisionPanel.SetActive(false);

        if (playerCamera != null)
            playerCamera.EnableLook();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void MakeDecision(DecisionType decision)
    {
        switch (decision)
        {
            case DecisionType.Execute:
                ExecuteDecision();
                break;
            case DecisionType.Exile:
                ExileDecision();
                break;
            case DecisionType.Forgive:
                ForgiveDecision();
                break;
        }

        CompleteDecision();
    }

    private void CompleteDecision()
    {
        HideDecision();
        UpdateStatsUI();

        // Notify the current character that decision was made
        if (currentCharacterInstance != null)
        {
            character_mov charMov = currentCharacterInstance.GetComponent<character_mov>();
            if (charMov != null)
            {
                charMov.OnDecisionMade();
            }
        }

        // Start coroutine to spawn new character after a delay
        StartCoroutine(SpawnNewCharacterAfterDelay(2f));
    }

    private System.Collections.IEnumerator SpawnNewCharacterAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnNewCharacter();
    }

    private void SpawnNewCharacter()
    {
        if (characterPrefab != null && spawnPoint != null)
        {
            currentCharacterInstance = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

            // Set up the new character's reference to this Makedecision
            character_mov newCharMov = currentCharacterInstance.GetComponent<character_mov>();
            if (newCharMov != null)
            {
                newCharMov.makedecision = this;
            }
        }
        else
        {
            Debug.LogError("Character prefab or spawn point not assigned!");
        }
    }

    private void ExecuteDecision()
    {
        GameState stats = GameState.Instance;
        bool isGuilty = currentCharacter.isGuilty;

        int popChange = -random.Next(10, 21);
        int fearChange = random.Next(15, 26);
        int divineChange = isGuilty ? random.Next(5, 16) : -random.Next(20, 31);
        int karmaChange = -random.Next(10, 21);

        stats.currentStats.population += popChange;
        stats.currentStats.fear += fearChange;
        stats.currentStats.divineFavor += divineChange;
        stats.currentStats.karma += karmaChange;

        stats.currentStats.ClampValues();

        Debug.Log($"Executed {currentCharacter.characterName}");
    }

    private void ExileDecision()
    {
        GameState stats = GameState.Instance;

        int popChange = -random.Next(5, 11);
        int fearChange = random.Next(5, 11);
        int divineChange = random.Next(2, 9);
        int karmaChange = -random.Next(2, 6);

        stats.currentStats.population += popChange;
        stats.currentStats.fear += fearChange;
        stats.currentStats.divineFavor += divineChange;
        stats.currentStats.karma += karmaChange;

        stats.currentStats.ClampValues();

        Debug.Log($"Exiled {currentCharacter.characterName}");
    }

    private void ForgiveDecision()
    {
        GameState stats = GameState.Instance;

        int popChange = random.Next(5, 11);
        int fearChange = -random.Next(10, 16);
        int divineChange = random.Next(-5, 11);
        int karmaChange = random.Next(10, 21);

        stats.currentStats.population += popChange;
        stats.currentStats.fear += fearChange;
        stats.currentStats.divineFavor += divineChange;
        stats.currentStats.karma += karmaChange;

        stats.currentStats.ClampValues();

        Debug.Log($"Forgave {currentCharacter.characterName}");
    }

    private void UpdateStatsUI()
    {
        if (statsText != null && GameState.Instance != null)
        {
            GameState stats = GameState.Instance;
            statsText.text = $"Population: {stats.currentStats.population}\n" +
                           $"Fear: {stats.currentStats.fear}\n" +
                           $"Divine Favor: {stats.currentStats.divineFavor}\n" +
                           $"Karma: {stats.currentStats.karma}";
        }
    }

    private Character GenerateRandomCharacter()
    {
        string[] names = { "Thomas the Farmer", "Lady Eleanor", "Blacksmith Gregor", "Mysterious Stranger" };
        string[] crimes = { "Stealing bread", "Heresy", "Assault", "Conspiracy" };
        string[] descriptions = {
            "I swear I'm innocent!",
            "It was an accident, I promise!",
            "They forced me to do it!",
            "I had no choice..."
        };

        return new Character(
            names[random.Next(names.Length)],
            crimes[random.Next(crimes.Length)],
            descriptions[random.Next(descriptions.Length)],
            random.Next(0, 2) == 1
        );
    }
}

public enum DecisionType
{
    Execute,
    Exile,
    Forgive
}

[System.Serializable]
public class Character
{
    public string characterName;
    public string crime;
    public string description;
    public bool isGuilty;

    public Character(string name, string crimeDesc, string desc, bool guilty)
    {
        characterName = name;
        crime = crimeDesc;
        description = desc;
        isGuilty = guilty;
    }
}