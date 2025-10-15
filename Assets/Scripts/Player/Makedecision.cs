using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Characters;

public partial class Makedecision : MonoBehaviour
{
    [Header("UI References")]
    public GameObject decisionPanel;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI crimeText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;

    [Header("Button References - Assign in Inspector")]
    public Button decisionButton1;
    public Button decisionButton2;
    public Button decisionButton3;


    [Header("Button Text References")]
    public TextMeshProUGUI decisionText1;
    public TextMeshProUGUI decisionText2;
    public TextMeshProUGUI decisionText3;

    [Header("Game References")]
    public camera_mov playerCamera;

    [Header("Character Management")]
    public GameObject characterPrefab;
    public Transform spawnPoint;

    private System.Random random = new System.Random();
    private List<Button> decisionButtons;
    private List<TextMeshProUGUI> decisionTexts;

    void Start()
    {
        // Initialize decisions system
        Decisions.InitializeDecisions();

        // Setup button lists for easy access
        decisionButtons = new List<Button> { decisionButton1, decisionButton2, decisionButton3 };
        decisionTexts = new List<TextMeshProUGUI> { decisionText1, decisionText2, decisionText3 };

        // Setup button listeners
        for (int i = 0; i < decisionButtons.Count; i++)
        {
            int index = i; // Capture the index for the lambda
            if (decisionButtons[i] != null)
            {
                decisionButtons[i].onClick.AddListener(() => MakeDecisionAtIndex(index));
            }
        }

        // Spawn the first character
        Characters.SpawnNewCharacter(this, characterPrefab, spawnPoint);
        UpdateStatsUI();
    }

    public void ShowDecision()
    {
        // Create a random character for this decision
        Characters.currentCharacter = Characters.GenerateRandomCharacter();

        // Update UI with character info
        if (characterNameText != null)
            characterNameText.text = Characters.currentCharacter.characterName;

        if (crimeText != null)
            crimeText.text = $"Crime: {Characters.currentCharacter.crime}";

        if (descriptionText != null)
            descriptionText.text = Characters.currentCharacter.description;

        // Update decision buttons with current available decisions
        UpdateDecisionButtons();

        if (decisionPanel != null)
            decisionPanel.SetActive(true);

        if (playerCamera != null)
            playerCamera.DisableLook();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateDecisionButtons()
    {
        List<Decisions.DecisionType> currentDecisions = Decisions.GetCurrentActiveDecisions();

        // Enable and setup buttons based on available decisions
        for (int i = 0; i < decisionButtons.Count; i++)
        {
            if (decisionButtons[i] != null)
            {
                if (i < currentDecisions.Count)
                {
                    // Enable button and set text
                    decisionButtons[i].gameObject.SetActive(true);
                    if (decisionTexts[i] != null)
                    {
                        decisionTexts[i].text = Decisions.GetDecisionDisplayName(currentDecisions[i]);
                        // You could also add tooltips with the description
                    }
                }
                else
                {
                    // Disable button if no decision available for this slot
                    decisionButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void MakeDecisionAtIndex(int buttonIndex)
    {
        List<Decisions.DecisionType> currentDecisions = Decisions.GetCurrentActiveDecisions();

        if (buttonIndex < currentDecisions.Count)
        {
            Decisions.DecisionType selectedDecision = currentDecisions[buttonIndex];
            MakeDecision(selectedDecision);
        }
        else
        {
            Debug.LogError($"No decision available for button index {buttonIndex}");
        }
    }

    private void MakeDecision(Decisions.DecisionType decision)
    {
        Decisions.ExecuteDecision(Characters.currentCharacter, decision);
        CompleteDecision();
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

    private void CompleteDecision()
    {
        HideDecision();
        UpdateStatsUI();

        // Notify the current character that decision was made
        if (Characters.currentCharacterInstance != null)
        {
            character_mov charMov = Characters.currentCharacterInstance.GetComponent<character_mov>();
            if (charMov != null)
            {
                charMov.OnDecisionMade();
            }
        }

        // Start coroutine to spawn new character after a delay
        StartCoroutine(SpawnNewCharacterAfterDelay(2f));
    }

    private IEnumerator SpawnNewCharacterAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Characters.SpawnNewCharacter(this, characterPrefab, spawnPoint);
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
}



