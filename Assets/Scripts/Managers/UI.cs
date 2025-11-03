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
using UnityEngine.UI;
using static Makedecision;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


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
        public TextMeshProUGUI statsText;
        public TextMeshProUGUI postDecisionText;
        public GameObject postDecisionPanel;
        public GameObject storePanel;



        [Header("Tooltip Text References")]
        public TextMeshProUGUI tooltipTitle1;
        public TextMeshProUGUI tooltipEffects1;
        public TextMeshProUGUI tooltipTitle2;
        public TextMeshProUGUI tooltipEffects2;
        public TextMeshProUGUI tooltipTitle3;
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
        public TextMeshProUGUI storeMessageText;

        private List<Button> decisionButtons;
        private List<TextMeshProUGUI> decisionTexts;

        private Dictionary<Button, GameObject> buttonToTooltipMap = new Dictionary<Button, GameObject>();
        private Dictionary<Button, (TextMeshProUGUI title, TextMeshProUGUI effects)> buttonToTextMap = new Dictionary<Button, (TextMeshProUGUI, TextMeshProUGUI)>();
        
        public static UI Instance { get; private set; }

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


            if (postDecisionPanel != null)
                postDecisionPanel.SetActive(false);

            if (storePanel != null)
                storePanel.SetActive(false);


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
        }

        public void UpdateTooltipContent(Button button, Decisions.DecisionType decisionType)
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

        private void ToggleStore(GameObject storePanel)
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

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

            if (storePanel != null)
                storePanel.SetActive(false);

            // Hide all tooltips when hiding decision
            HideAllTooltips();

            if (playerCamera != null)
                playerCamera.EnableLook();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

    }
}
