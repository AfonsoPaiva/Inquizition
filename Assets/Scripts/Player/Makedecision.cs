using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public TextMeshProUGUI postDecisionText;
    public GameObject postDecisionPanel;
    public GameObject storePanel;

    [Header("Button References")]
    public Button decisionButton1;
    public Button decisionButton2;
    public Button decisionButton3;

    [Header("Button Text References")]
    public TextMeshProUGUI decisionText1;
    public TextMeshProUGUI decisionText2;
    public TextMeshProUGUI decisionText3;

    [Header("Tooltip Panels - One for each button")]
    public GameObject tooltipPanel1; // For button 1
    public GameObject tooltipPanel2; // For button 2
    public GameObject tooltipPanel3; // For button 3

    [Header("Tooltip Text References")]
    public TextMeshProUGUI tooltipTitle1;
    public TextMeshProUGUI tooltipEffects1;
    public TextMeshProUGUI tooltipTitle2;
    public TextMeshProUGUI tooltipEffects2;
    public TextMeshProUGUI tooltipTitle3;
    public TextMeshProUGUI tooltipEffects3;

    [Header("Game References")]
    public camera_mov playerCamera;
    public Transform cameraFocusPoint;

    [Header("Character Management")]
    public GameObject characterPrefab;
    public Transform spawnPoint;
    public Transform judgmentPoint;
    public Transform pointGood;
    public Transform pointBad;

    [Header("Queue Settings")]
    public int queueSize = 5;
    public float spacing = 2f;

    [Header("Camera Points")]
    public Transform cameraGoodPoint;
    public Transform cameraBadPoint;

    [Header("Store Items")]
    public Button buyPopulationButton;
    public Button buyDivineFavorButton;
    public Button buyKarmaButton;
    public TextMeshProUGUI storeMessageText;

    private Dictionary<Decisions.DecisionType, string> decisionEffectsCache = new Dictionary<Decisions.DecisionType, string>();
    private Dictionary<Button, GameObject> buttonToTooltipMap = new Dictionary<Button, GameObject>();
    private Dictionary<Button, (TextMeshProUGUI title, TextMeshProUGUI effects)> buttonToTextMap = new Dictionary<Button, (TextMeshProUGUI, TextMeshProUGUI)>();

    private System.Random random = new System.Random();
    private List<Button> decisionButtons;
    private List<TextMeshProUGUI> decisionTexts;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;

    private Queue<GameObject> characterQueue = new Queue<GameObject>();
    private bool isProcessingDecision = false;
    private bool godGuidanceRevealed = false;
    private bool revealedGuiltStatus = false;

    void Start()
    {
        Decisions.InitializeDecisions();

        decisionButtons = new List<Button> { decisionButton1, decisionButton2, decisionButton3 };
        decisionTexts = new List<TextMeshProUGUI> { decisionText1, decisionText2, decisionText3 };

        for (int i = 0; i < decisionButtons.Count; i++)
        {
            int index = i;
            if (decisionButtons[i] != null)
            {
                decisionButtons[i].onClick.AddListener(() => MakeDecisionAtIndex(index));
            }
        }


        // Initialize store item buttons
        if (buyPopulationButton != null)
        {
            buyPopulationButton.onClick.AddListener(() => BuyStoreItem(StoreItemType.Population));
        }
        if (buyDivineFavorButton != null)
        {
            buyDivineFavorButton.onClick.AddListener(() => BuyStoreItem(StoreItemType.DivineFavor));
        }
        if (buyKarmaButton != null)
        {
            buyKarmaButton.onClick.AddListener(() => BuyStoreItem(StoreItemType.Karma));
        }

        // Initialize tooltip system
        InitializeTooltipSystem();

        if (postDecisionPanel != null)
            postDecisionPanel.SetActive(false);

        if (storePanel != null)
            storePanel.SetActive(false);

        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.position;
            originalCameraRotation = playerCamera.transform.rotation;
            originalCameraParent = playerCamera.transform.parent;
        }

        // Create initial queue
        CreateInitialQueue();

        // Move first character to judgment point
        StartCoroutine(MoveToJudgmentPosition());
    }

    private enum StoreItemType
    {
        Population,
        DivineFavor,
        Karma
    }

    private void ToggleStore()
    {
        if (storePanel != null)
        {
            bool newState = !storePanel.activeSelf;
            storePanel.SetActive(newState);

            if (newState)
            {

                HideDecision();
            }
            else
            {
                ShowDecision();
            }
        }
    }



    private void BuyStoreItem(StoreItemType itemType)
    {
        if (GameState.Instance == null) return;

        int cost = 0;
        int amount = 0;
        string message = "";

        switch (itemType)
        {
            case StoreItemType.Population:
                cost = 30;
                amount = 15;
                message = "Hired new settlers to increase population";
                break;
            case StoreItemType.DivineFavor:
                cost = 25;
                amount = 20;
                message = "Made offerings to the gods";
                break;
            case StoreItemType.Karma:
                cost = 20;
                amount = 15;
                message = "Performed charitable acts";
                break;
        }

        if (GameState.Instance.currentStats.gold >= cost)
        {
            GameState.Instance.currentStats.gold -= cost;

            switch (itemType)
            {
                case StoreItemType.Population:
                    GameState.Instance.currentStats.population += amount;
                    break;
                case StoreItemType.DivineFavor:
                    GameState.Instance.currentStats.divineFavor += amount;
                    break;
                case StoreItemType.Karma:
                    GameState.Instance.currentStats.karma += amount;
                    break;
            }

            GameState.Instance.currentStats.ClampValues();


            if (storeMessageText != null)
            {
                storeMessageText.text = $"Purchase successful! {message}";
                StartCoroutine(ClearStoreMessage());
            }
        }
        else
        {
            if (storeMessageText != null)
            {
                storeMessageText.text = "Not enough gold!";
                StartCoroutine(ClearStoreMessage());
            }
        }
    }

    private IEnumerator ClearStoreMessage()
    {
        yield return new WaitForSeconds(2f);
        if (storeMessageText != null)
        {
            storeMessageText.text = "";
        }
    }

    private void InitializeTooltipSystem()
    {
        // Create cache of decision effects for quick access
        decisionEffectsCache.Clear();
        buttonToTooltipMap.Clear();
        buttonToTextMap.Clear();

        // Map buttons to their respective tooltip panels and text components
        if (decisionButton1 != null && tooltipPanel1 != null)
        {
            buttonToTooltipMap[decisionButton1] = tooltipPanel1;
            buttonToTextMap[decisionButton1] = (tooltipTitle1, tooltipEffects1);
            SetupButtonHoverEvents(decisionButton1, tooltipPanel1);
        }

        if (decisionButton2 != null && tooltipPanel2 != null)
        {
            buttonToTooltipMap[decisionButton2] = tooltipPanel2;
            buttonToTextMap[decisionButton2] = (tooltipTitle2, tooltipEffects2);
            SetupButtonHoverEvents(decisionButton2, tooltipPanel2);
        }

        if (decisionButton3 != null && tooltipPanel3 != null)
        {
            buttonToTooltipMap[decisionButton3] = tooltipPanel3;
            buttonToTextMap[decisionButton3] = (tooltipTitle3, tooltipEffects3);
            SetupButtonHoverEvents(decisionButton3, tooltipPanel3);
        }

        // Cache all decision effects
        foreach (Decisions.DecisionType decisionType in System.Enum.GetValues(typeof(Decisions.DecisionType)))
        {
            decisionEffectsCache[decisionType] = GenerateDecisionEffectsText(decisionType);
        }


        // Hide all tooltips initially
        HideAllTooltips();
    }

    private void SetupButtonHoverEvents(Button button, GameObject tooltipPanel)
    {
        if (button == null || tooltipPanel == null)
        {
            return;
        }


        // Remove existing EventTrigger if any
        var existingEventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (existingEventTrigger != null)
        {
            Destroy(existingEventTrigger);
        }

        // Add new EventTrigger
        var eventTrigger = button.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter event - show THIS button's tooltip
        var pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(true);
            }

        });
        eventTrigger.triggers.Add(pointerEnter);

        // Pointer Exit event - hide THIS button's tooltip
        var pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        });
        eventTrigger.triggers.Add(pointerExit);

        // Additional: Pointer Click event to ensure tooltips work
        var pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener((data) => {
        });
        eventTrigger.triggers.Add(pointerClick);

    }

    private void HideAllTooltips()
    {
        if (tooltipPanel1 != null)
        {
            tooltipPanel1.SetActive(false);
        }
        if (tooltipPanel2 != null)
        {
            tooltipPanel2.SetActive(false);
        }
        if (tooltipPanel3 != null)
        {
            tooltipPanel3.SetActive(false);
        }
    }

    private void UpdateTooltipContent(Button button, Decisions.DecisionType decisionType)
    {
        if (button == null || !buttonToTextMap.ContainsKey(button))
        {
            return;
        }

        var (titleText, effectsText) = buttonToTextMap[button];

        if (titleText != null)
        {
            string title = Decisions.GetDecisionDisplayName(decisionType);
            titleText.text = title;
        }


        if (effectsText != null && decisionEffectsCache.ContainsKey(decisionType))
        {
            string effects = decisionEffectsCache[decisionType];
            effectsText.text = effects;
        }

    }

    private string GenerateDecisionEffectsText(Decisions.DecisionType decisionType)
    {
        switch (decisionType)
        {
            case Decisions.DecisionType.Execute:
                return "Population: --\nFear: ++\nDivine Favor: ±\nKarma: --";

            case Decisions.DecisionType.Exile:
                return "Population: -\nFear: +\nDivine Favor: +\nKarma: -";

            case Decisions.DecisionType.Forgive:
                return "Population: +\nFear: --\nDivine Favor: ±\nKarma: ++";

            case Decisions.DecisionType.Confiscate:
                return "Population: -\nFear: +\nDivine Favor: +\nKarma: -\nGold: ++";

            case Decisions.DecisionType.Imprison:
                return "Population: ±\nFear: ++\nDivine Favor: +\nKarma: -";

            case Decisions.DecisionType.Torture:
                return "Population: -\nFear: +++\nDivine Favor: --\nKarma: --";

            case Decisions.DecisionType.TrialByOrdeal:
                return "Fear: ++\nDivine Favor: ++\nPopulation: ±";

            case Decisions.DecisionType.RedemptionQuest:
                return "Fear: -\nDivine Favor: ++\nKarma: ++\nChance of future reward";

            case Decisions.DecisionType.PublicHumiliation:
                return "Fear: +\nDivine Favor: +\nKarma: -";

            case Decisions.DecisionType.BanishWilderness:
                return "Population: -\nFear: ++\nDivine Favor: ++\nKarma: -";

            case Decisions.DecisionType.SpareWithWarning:
                return "Population: +\nFear: -\nDivine Favor: +\nKarma: +";

            case Decisions.DecisionType.CollectivePunishment:
                return "Population: ---\nFear: +++\nDivine Favor: ±\nKarma: ---";

            case Decisions.DecisionType.SacrificeToGod:
                return "Population: --\nFear: +++\nDivine Favor: +++\nKarma: ---";

            case Decisions.DecisionType.Corruption:
                return "Fear: --\nDivine Favor: --\nKarma: --\nGold: +++";

            case Decisions.DecisionType.AskGodForGuidance:
                return "Divine Favor: -\nReveals true guilt/innocence";

            default:
                return "Effects vary based on circumstances";
        }
    }

    private void CreateInitialQueue()
    {
        for (int i = 0; i < queueSize; i++)
        {
            SpawnCharacterInQueue(i);
        }
    }

    private void SpawnCharacterInQueue(int position)
    {
        Vector3 queuePosition = spawnPoint.position + (spawnPoint.forward * spacing * (position + 1));
        GameObject newCharacter = Instantiate(characterPrefab, queuePosition, spawnPoint.rotation);

        character_mov charMov = newCharacter.GetComponent<character_mov>();
        if (charMov != null)
        {
            charMov.enabled = false;
        }

        characterQueue.Enqueue(newCharacter);
    }

    private IEnumerator MoveToJudgmentPosition()
    {
        if (characterQueue.Count == 0 || isProcessingDecision) yield break;

        GameObject nextCharacter = characterQueue.Dequeue();
        Characters.currentCharacterInstance = nextCharacter;
        Characters.currentCharacter = Characters.GenerateRandomCharacter();

        character_mov charMov = nextCharacter.GetComponent<character_mov>();
        if (charMov != null)
        {
            charMov.enabled = true;
            charMov.makedecision = this;
        }

        Transform characterTransform = nextCharacter.transform;
        Vector3 startPos = characterTransform.position;
        Vector3 targetPos = judgmentPoint.position;

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            characterTransform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.position = targetPos;

        // Reset god guidance for new character
        godGuidanceRevealed = false;
        revealedGuiltStatus = false;

        ShowDecision();
    }

    public void ShowDecision()
    {
        if (Characters.currentCharacter == null)
        {
            return;
        }

        if (playerCamera != null)
        {
            playerCamera.DisableLook();
        }

        if (characterNameText != null)
            characterNameText.text = Characters.currentCharacter.characterName;

        if (crimeText != null)
        {
            string guiltText = godGuidanceRevealed ?
                $"Crime: {Characters.currentCharacter.crime} [{(revealedGuiltStatus ? "GUILTY" : "INNOCENT")}]" :
                $"Crime: {Characters.currentCharacter.crime}";
            crimeText.text = guiltText;
        }

        if (descriptionText != null)
            descriptionText.text = Characters.currentCharacter.description;

        Decisions.SelectRandomDecisions(Characters.currentCharacter, GameState.Instance);
        UpdateDecisionButtons();

        if (decisionPanel != null)
        {
            decisionPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UpdateDecisionButtons()
    {
        List<Decisions.DecisionType> currentDecisions = Decisions.GetCurrentActiveDecisions();

        // Hide all tooltips first
        HideAllTooltips();

        for (int i = 0; i < decisionButtons.Count; i++)
        {
            if (decisionButtons[i] != null)
            {
                if (i < currentDecisions.Count)
                {
                    decisionButtons[i].gameObject.SetActive(true);
                    if (decisionTexts[i] != null)
                    {
                        string decisionText = Decisions.GetDecisionDisplayName(currentDecisions[i]);
                        decisionTexts[i].text = decisionText;
                    }

                    // Update the tooltip content for this button
                    UpdateTooltipContent(decisionButtons[i], currentDecisions[i]);
                }
                else
                {
                    decisionButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void MakeDecisionAtIndex(int buttonIndex)
    {
        if (isProcessingDecision) return;

        List<Decisions.DecisionType> currentDecisions = Decisions.GetCurrentActiveDecisions();

        if (buttonIndex < currentDecisions.Count)
        {
            Decisions.DecisionType selectedDecision = currentDecisions[buttonIndex];
            MakeDecision(selectedDecision);
        }
    }

    private void MakeDecision(Decisions.DecisionType decision)
    {
        isProcessingDecision = true;

        // Hide all tooltips when making a decision
        HideAllTooltips();

        // Handle Ask God For Guidance special case
        if (decision == Decisions.DecisionType.AskGodForGuidance)
        {
            godGuidanceRevealed = true;
            revealedGuiltStatus = Characters.currentCharacter.isGuilty;

            // Update crime text to show guilt status
            if (crimeText != null)
            {
                crimeText.text = $"Crime: {Characters.currentCharacter.crime} [{(revealedGuiltStatus ? "GUILTY" : "INNOCENT")}]";
            }

            // Don't proceed with normal decision flow yet - let player make another decision
            Decisions.ExecuteDecision(Characters.currentCharacter, decision);
            isProcessingDecision = false;
            return;
        }

        Decisions.ExecuteDecision(Characters.currentCharacter, decision);

        if (decisionPanel != null)
            decisionPanel.SetActive(false);

        bool isGoodOutcome = IsGoodOutcome(decision, Characters.currentCharacter.isGuilty);
        StartCoroutine(ProcessDecisionOutcome(isGoodOutcome, decision));
    }

    private bool IsGoodOutcome(Decisions.DecisionType decision, bool isGuilty)
    {
        switch (decision)
        {
            case Decisions.DecisionType.Forgive:
            case Decisions.DecisionType.RedemptionQuest:
            case Decisions.DecisionType.SpareWithWarning:
                return !isGuilty; // Good outcome for innocent people

            case Decisions.DecisionType.Execute:
            case Decisions.DecisionType.Exile:
            case Decisions.DecisionType.Torture:
            case Decisions.DecisionType.SacrificeToGod:
            case Decisions.DecisionType.Corruption:
                return isGuilty; // Good outcome for guilty people

            default:
                return random.Next(2) == 0;
        }
    }

    private IEnumerator ProcessDecisionOutcome(bool isGoodOutcome, Decisions.DecisionType decision)
    {
        ShowPostDecisionDialogue(decision);
        yield return new WaitForSeconds(1f);

        Transform cameraOutcomePoint = isGoodOutcome ? cameraGoodPoint : cameraBadPoint;
        yield return StartCoroutine(MoveCameraToOutcome(cameraOutcomePoint));
        yield return new WaitForSeconds(0.5f);

        if (postDecisionPanel != null)
            postDecisionPanel.SetActive(false);

        Transform characterOutcomePoint = isGoodOutcome ? pointGood : pointBad;
        yield return StartCoroutine(MoveCharacterToOutcome(characterOutcomePoint.position));
        yield return StartCoroutine(ReturnCameraToPlayer());

        if (Characters.currentCharacterInstance != null)
        {
            Destroy(Characters.currentCharacterInstance);
            Characters.currentCharacterInstance = null;
        }

        SpawnCharacterInQueue(queueSize - 1);
        yield return StartCoroutine(MoveQueueForward());

        isProcessingDecision = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(MoveToJudgmentPosition());
    }

    private IEnumerator MoveCameraToOutcome(Transform outcomePoint)
    {
        if (playerCamera == null || outcomePoint == null) yield break;

        playerCamera.DisableLook();
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;
        Vector3 targetPosition = outcomePoint.position;
        Quaternion targetRotation = outcomePoint.rotation;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = targetPosition;
        playerCamera.transform.rotation = targetRotation;
    }

    private IEnumerator ReturnCameraToPlayer()
    {
        if (playerCamera == null) yield break;

        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            playerCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            playerCamera.transform.rotation = Quaternion.Lerp(startRotation, originalCameraRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;
        playerCamera.EnableLook();
    }

    private IEnumerator MoveCharacterToOutcome(Vector3 targetPosition)
    {
        if (Characters.currentCharacterInstance == null) yield break;

        Transform characterTransform = Characters.currentCharacterInstance.transform;
        Vector3 startPosition = characterTransform.position;
        Quaternion startRotation = characterTransform.rotation;

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            characterTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            characterTransform.rotation = Quaternion.Lerp(startRotation, judgmentPoint.rotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.position = targetPosition;
        characterTransform.rotation = judgmentPoint.rotation;
    }

    private IEnumerator MoveQueueForward()
    {
        List<GameObject> characters = new List<GameObject>(characterQueue);
        characterQueue.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            GameObject character = characters[i];
            Vector3 targetPosition = spawnPoint.position + (spawnPoint.forward * spacing * (i + 1));

            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPosition = character.transform.position;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                character.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            character.transform.position = targetPosition;
            characterQueue.Enqueue(character);
        }
    }

    private void ShowPostDecisionDialogue(Decisions.DecisionType decision)
    {
        string decisionKey = decision.ToString().ToLower();
        string dialogue = Characters.GetPostDecisionDialogue(decisionKey);

        if (postDecisionText != null)
            postDecisionText.text = dialogue;

        if (postDecisionPanel != null)
            postDecisionPanel.SetActive(true);
    }

    private void UpdateStatsUI()
    {
        if (statsText != null && GameState.Instance != null)
        {
            GameState stats = GameState.Instance;
            statsText.text = $"Population: {stats.currentStats.population}\n" +
                           $"Fear: {stats.currentStats.fear}\n" +
                           $"Divine Favor: {stats.currentStats.divineFavor}\n" +
                           $"Karma: {stats.currentStats.karma}\n" +
                           $"Gold: {stats.currentStats.gold}";
        }
    }

    private void Update()
    {
        UpdateStatsUI();
    }

    public void HideDecision()
    {
        if (decisionPanel != null)
            decisionPanel.SetActive(false);

        if (postDecisionPanel != null)
            postDecisionPanel.SetActive(false);

        if (storePanel != null)
            storePanel.SetActive(false);

        // Hide all tooltips when hiding decision
        HideAllTooltips();

        if (playerCamera != null)
            playerCamera.EnableLook();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}