using Assets.Scripts.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Makedecision;


namespace Assets.Scripts.Managers
{


    public class UI : MonoBehaviour
    {

        [Header("References")]
        public camera_mov playerCamera;
        public Makedecision makedecision;
        public static UI ui;

        [Header("UI References")]
        public GameObject decisionPanel;
        public TextMeshProUGUI characterNameText;
        public TextMeshProUGUI crimeText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI postDecisionText;
        public GameObject postDecisionPanel;
        public GameObject storePanel;

        [Header("Pause Menu")]
        public GameObject pauseMenuPanel;
        public Button continueButton;
        public Button mainMenuButton;

        [Header("Stat Bars (assign Image components)")]
        public Image populationBar;      
        public Image fearBar;           
        public Image divineFavorBar;  
        public Image karmaBar;         
       
        [Header("Gold Number (assign TextMeshProUGUI)")]
        public TextMeshProUGUI goldText;

        [Header("Bar smoothing (seconds to reach target)")]
        [Tooltip("Time (in seconds) it takes the bar to reach the target value. 0 = instant.")]
        public float barLerpDuration = 3f;

        // internal smoothing state
        private float targetPopulationFill;
        private float targetFearFill;
        private float targetDivineFavorFill;
        private float targetKarmaFill;

        private float currentPopulationFill;
        private float currentFearFill;
        private float currentDivineFavorFill;
        private float currentKarmaFill;

        [Header("Tooltip Text References")]
        public TextMeshProUGUI tooltipEffects1;
        public TextMeshProUGUI tooltipEffects2;
        public TextMeshProUGUI tooltipEffects3;

        [Header("Tooltip Panels - One for each button")]
        public GameObject tooltipPanel1;
        public GameObject tooltipPanel2;
        public GameObject tooltipPanel3;

        [Header("Button References")]
        public Button decisionButton1;
        public Button decisionButton2;
        public Button decisionButton3;

        [Header("Button Text References")]
        public TextMeshProUGUI decisionText1;
        public TextMeshProUGUI decisionText2;
        public TextMeshProUGUI decisionText3;


        [Header("Store Items")]
        public Button buyPopulationButton;
        public Button buyDivineFavorButton;
        public Button buyKarmaButton;
        public Button toggleStoreButton;
        public TextMeshProUGUI storeMessageText;

        [Header("Store Animation Settings")]
        [Tooltip("Duration of the slide animation in seconds")]
        public float slideAnimationDuration = 0.3f;

        private List<Button> decisionButtons;
        private List<TextMeshProUGUI> decisionTexts;

        private Dictionary<Button, GameObject> buttonToTooltipMap = new Dictionary<Button, GameObject>();
        private Dictionary<Button, TextMeshProUGUI> buttonToTextMap = new Dictionary<Button, TextMeshProUGUI>();

        public static UI Instance { get; private set; }

        // Store panel animation state
        private bool isStoreOpen = false;
        private bool isAnimating = false;
        private RectTransform storePanelRect;
        private Vector2 hiddenPosition;
        private Vector2 visiblePosition;

        // Pause menu state
        private bool isPaused = false;

        void Awake() { Instance = this; }

        private enum StoreItemType
        {
            Population,
            DivineFavor,
            Karma
        }


        void Start()
        {
            InitializeTooltipSystem();

            // initialize current fill values (so the first frame won't jump)
            if (populationBar != null) currentPopulationFill = populationBar.fillAmount;
            if (fearBar != null) currentFearFill = fearBar.fillAmount;
            if (divineFavorBar != null) currentDivineFavorFill = divineFavorBar.fillAmount;
            if (karmaBar != null) currentKarmaFill = karmaBar.fillAmount;

            if (postDecisionPanel != null)
                postDecisionPanel.SetActive(false);

            // Initialize pause menu
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            // Setup pause menu buttons
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ResumeGame);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(LoadMainMenu);
            }

            // Initialize store panel position
            if (storePanel != null)
            {
                storePanel.SetActive(true); // Must be active to get RectTransform
                storePanelRect = storePanel.GetComponent<RectTransform>();
                
                if (storePanelRect != null)
                {
                    // Store the visible position (current position)
                    visiblePosition = storePanelRect.anchoredPosition;

                    // Calculate hidden position (off-screen to the left)
                    hiddenPosition = new Vector2(visiblePosition.x - storePanelRect.rect.width - 200, visiblePosition.y);

                    // Start hidden
                    storePanelRect.anchoredPosition = hiddenPosition;
                }
                
                isStoreOpen = false;
            }


            decisionButtons = new List<Button> { decisionButton1, decisionButton2, decisionButton3 };
            decisionTexts = new List<TextMeshProUGUI> { decisionText1, decisionText2, decisionText3 };

            for (int i = 0; i < decisionButtons.Count; i++)
            {
                int index = i;
                if (decisionButtons[i] != null)
                {
                    decisionButtons[i].onClick.AddListener(() => makedecision.MakeDecisionAtIndex(index));
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

            // Initialize toggle store button
            if (toggleStoreButton != null)
            {
                toggleStoreButton.onClick.AddListener(ToggleStore);
            }
        }

        void Update()
        {
            UpdateStatsUI();

            // Press 'B' to open/close store (like "Buy")
            if (Input.GetKeyDown(KeyCode.B) && !makedecision.isProcessingDecision && !isAnimating && !isPaused)
            {
                ToggleStore();
            }

            // Press ESC to toggle pause menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        private void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Freeze game time

            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }

            // Show and unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Resume game time

            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
        }

        private void LoadMainMenu()
        {
            Time.timeScale = 1f; // Reset time scale before changing scenes
            SceneManager.LoadScene("MenuScene"); // Change to your main menu scene name
        }

        public void UpdateTooltipContent(Button button, Decisions.DecisionType decisionType)
        {
            if (button == null || !buttonToTextMap.ContainsKey(button))
            {
                return;
            }

            var effectsText = buttonToTextMap[button];

            if (effectsText != null && makedecision.decisionEffectsCache.ContainsKey(decisionType))
            {
                string effects = makedecision.decisionEffectsCache[decisionType];
                effectsText.text = effects;
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
                    amount = 15;
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

                // CRITICAL: Check for endings immediately after buying items
                GameState.Instance.CheckForEndings();

                // Only show success message if game didn't end
                if (!GameState.Instance.gameEnded)
                {
                    if (storeMessageText != null)
                    {
                        storeMessageText.text = $"Purchase successful! {message}";
                        StartCoroutine(ClearStoreMessage());
                    }
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


        private void InitializeTooltipSystem()
        {
            // Create cache of decision effects for quick access
            makedecision.decisionEffectsCache.Clear();
            buttonToTooltipMap.Clear();
            buttonToTextMap.Clear();

            // Map buttons to their respective tooltip panels and text components
            if (decisionButton1 != null && tooltipPanel1 != null)
            {
                buttonToTooltipMap[decisionButton1] = tooltipPanel1;
                buttonToTextMap[decisionButton1] = tooltipEffects1;
                SetupButtonHoverEvents(decisionButton1, tooltipPanel1);
            }

            if (decisionButton2 != null && tooltipPanel2 != null)
            {
                buttonToTooltipMap[decisionButton2] = tooltipPanel2;
                buttonToTextMap[decisionButton2] = tooltipEffects2;
                SetupButtonHoverEvents(decisionButton2, tooltipPanel2);
            }

            if (decisionButton3 != null && tooltipPanel3 != null)
            {
                buttonToTooltipMap[decisionButton3] = tooltipPanel3;
                buttonToTextMap[decisionButton3] = tooltipEffects3;
                SetupButtonHoverEvents(decisionButton3, tooltipPanel3);
            }

            // Cache all decision effects
            foreach (Decisions.DecisionType decisionType in System.Enum.GetValues(typeof(Decisions.DecisionType)))
            {
                makedecision.decisionEffectsCache[decisionType] = GenerateDecisionEffectsText(decisionType);
            }


            // Hide all tooltips initially
            HideAllTooltips();
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

        private IEnumerator ClearStoreMessage()
        {
            yield return new WaitForSeconds(2f);
            if (storeMessageText != null)
            {
                storeMessageText.text = "";
            }
        }

        public void ToggleStore()
        {
            if (storePanel == null || storePanelRect == null || isAnimating) return;

            StartCoroutine(AnimateStorePanel(!isStoreOpen));
        }

        private IEnumerator AnimateStorePanel(bool open)
        {
            isAnimating = true;

            Vector2 startPos = storePanelRect.anchoredPosition;
            Vector2 targetPos = open ? visiblePosition : hiddenPosition;

            float elapsed = 0f;

            while (elapsed < slideAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / slideAnimationDuration);
                storePanelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            storePanelRect.anchoredPosition = targetPos;
            isStoreOpen = open;
            isAnimating = false;

        
        }

        

        public void ShowDecision()
        {
            if (Characters.currentCharacter == null)
                return;

            if (playerCamera != null)
                playerCamera.DisableLook();

            if (characterNameText != null)
                characterNameText.text = Characters.currentCharacter.characterName;

            if (crimeText != null)
            {
                string guiltText = makedecision.godGuidanceRevealed ?
                    $"Crime: {Characters.currentCharacter.crime} [{(makedecision.revealedGuiltStatus ? "GUILTY" : "INNOCENT")}]" :
                    $"Crime: {Characters.currentCharacter.crime}";
                crimeText.text = guiltText;
            }

            if (descriptionText != null)
                descriptionText.text = Characters.currentCharacter.description;

            Decisions.SelectRandomDecisions(Characters.currentCharacter, GameState.Instance);
            UpdateDecisionButtons();

            if (decisionPanel != null)
                decisionPanel.SetActive(true);

 
        }

        public static void ShowPostDecisionDialogue(Decisions.DecisionType decision)
        {
            string decisionKey = decision.ToString().ToLower();
            string dialogue = Characters.GetPostDecisionDialogue(decisionKey);

            if (UI.Instance == null)
            {
                Debug.LogError("UI.Instance is null! Make sure a UI object with the UI script is in the scene and enabled.");
                return;
            }

            if (UI.Instance.postDecisionText == null)
            {
                Debug.LogError("UI.Instance.postDecisionText is not assigned in the Inspector!");
                return;
            }

            if (UI.Instance.postDecisionPanel == null)
            {
                Debug.LogError("UI.Instance.postDecisionPanel is not assigned in the Inspector!");
                return;
            }

            UI.Instance.postDecisionText.text = dialogue;
            UI.Instance.postDecisionPanel.SetActive(true);
        }

        public void UpdateDecisionButtons()
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

        public void HideDecision()
        {
            if (decisionPanel != null)
                decisionPanel.SetActive(false);

            if (postDecisionPanel != null)
                postDecisionPanel.SetActive(false);

            // Close store when hiding decisions
            if (isStoreOpen && !isAnimating)
            {
                StartCoroutine(AnimateStorePanel(false));
            }

            // Hide all tooltips when hiding decision
            HideAllTooltips();

            if (playerCamera != null)
                playerCamera.EnableLook();

            
        }

        public void HideAllTooltips()
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


        private void UpdateStatsUI()
        {
            if (GameState.Instance == null) return;

            GameState stats = GameState.Instance;

            // Use bars for the four clamped stats; gold remains a numeric Text.
            bool usingBars = populationBar != null || fearBar != null || divineFavorBar != null || karmaBar != null;

            if (usingBars)
            {

                // calculate targets (values clamped 0..100 in GameState)
                targetPopulationFill = populationBar != null ? Mathf.Clamp01(stats.currentStats.population / 100f) : 0f;
                targetFearFill = fearBar != null ? Mathf.Clamp01(stats.currentStats.fear / 100f) : 0f;
                targetDivineFavorFill = divineFavorBar != null ? Mathf.Clamp01(stats.currentStats.divineFavor / 100f) : 0f;
                targetKarmaFill = karmaBar != null ? Mathf.Clamp01(stats.currentStats.karma / 100f) : 0f;

                // animate current fills towards targets
                float step = barLerpDuration <= 0f ? 1f : (Time.deltaTime / Mathf.Max(0.00001f, barLerpDuration));

                if (populationBar != null)
                {
                    currentPopulationFill = Mathf.MoveTowards(currentPopulationFill, targetPopulationFill, step);
                    populationBar.fillAmount = currentPopulationFill;
                }

                if (fearBar != null)
                {
                    currentFearFill = Mathf.MoveTowards(currentFearFill, targetFearFill, step);
                    fearBar.fillAmount = currentFearFill;
                }

                if (divineFavorBar != null)
                {
                    currentDivineFavorFill = Mathf.MoveTowards(currentDivineFavorFill, targetDivineFavorFill, step);
                    divineFavorBar.fillAmount = currentDivineFavorFill;
                }

                if (karmaBar != null)
                {
                    currentKarmaFill = Mathf.MoveTowards(currentKarmaFill, targetKarmaFill, step);
                    karmaBar.fillAmount = currentKarmaFill;
                }

                // Update gold as number
                if (goldText != null)
                {
                    goldText.gameObject.SetActive(true);
                    goldText.text = $"{stats.currentStats.gold}";
                }
            }
        }

    }
}

