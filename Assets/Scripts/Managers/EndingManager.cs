using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Assets.Scripts.Managers
{
    public class EndingManager : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI endingTitleText;
        public TextMeshProUGUI endingDescriptionText;
        public TextMeshProUGUI finalStatsText;
        public Image endingImage;
        public Button mainMenuButton;
        public Button playAgainButton;

        [Header("Ending Images")]
        public Sprite populationRevoltsSprite;
        public Sprite maniacEndingSprite;
        public Sprite punishedByGodSprite;
        public Sprite karmaStruckSprite;
        public Sprite karmaFreeSprite;
        public Sprite populationLovesYouSprite;
        public Sprite relaxedEndingSprite;
        public Sprite godLovesYouSprite;
        public Sprite defaultEndingSprite;

        void Start()
        {
            DisplayEnding();
            SetupButtons();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void DisplayEnding()
        {
            string endingName = PlayerPrefs.GetString("EndingName", "Unknown Ending");
            string endingDescription = PlayerPrefs.GetString("EndingDescription", "Your reign has ended.");
            int finalPopulation = PlayerPrefs.GetInt("FinalPopulation", 0);
            int finalFear = PlayerPrefs.GetInt("FinalFear", 0);
            int finalDivineFavor = PlayerPrefs.GetInt("FinalDivineFavor", 0);
            int finalKarma = PlayerPrefs.GetInt("FinalKarma", 0);
            int finalGold = PlayerPrefs.GetInt("FinalGold", 0);

            if (endingTitleText != null)
            {
                endingTitleText.text = endingName;
            }

            if (endingDescriptionText != null)
            {
                endingDescriptionText.text = endingDescription;
            }

            if (finalStatsText != null)
            {
                finalStatsText.text = 
                                     $"Population: {finalPopulation}\n" +
                                     $"Fear: {finalFear}\n" +
                                     $"Divine Favor: {finalDivineFavor}\n" +
                                     $"Karma: {finalKarma}\n" +
                                     $"Gold: {finalGold}";
            }

            // Set the appropriate ending image
            if (endingImage != null)
            {
                endingImage.sprite = GetEndingSprite(endingName);
            }
        }

        private Sprite GetEndingSprite(string endingName)
        {
            switch (endingName)
            {
                case "Population Revolts":
                    return populationRevoltsSprite != null ? populationRevoltsSprite : defaultEndingSprite;
                case "Maniac Ending":
                    return maniacEndingSprite != null ? maniacEndingSprite : defaultEndingSprite;
                case "Punished by God":
                    return punishedByGodSprite != null ? punishedByGodSprite : defaultEndingSprite;
                case "Karma Struck":
                    return karmaStruckSprite != null ? karmaStruckSprite : defaultEndingSprite;
                case "Karma Free":
                    return karmaFreeSprite != null ? karmaFreeSprite : defaultEndingSprite;
                case "Population Loves You":
                    return populationLovesYouSprite != null ? populationLovesYouSprite : defaultEndingSprite;
                case "Relaxed Ending":
                    return relaxedEndingSprite != null ? relaxedEndingSprite : defaultEndingSprite;
                case "God Loves You":
                    return godLovesYouSprite != null ? godLovesYouSprite : defaultEndingSprite;
                default:
                    return defaultEndingSprite;
            }
        }

        private void SetupButtons()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (playAgainButton != null)
            {
                playAgainButton.onClick.RemoveAllListeners();
                playAgainButton.onClick.AddListener(PlayAgain);
            }
        }

        private void ReturnToMainMenu()
        {
            // Clean up GameState
            if (GameState.Instance != null)
            {
                Destroy(GameState.Instance.gameObject);
            }

            SceneManager.LoadScene("MenuScene");
        }

        private void PlayAgain()
        {
            // Mark tutorial as completed so it won't play again
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            // Clean up GameState
            if (GameState.Instance != null)
            {
                Destroy(GameState.Instance.gameObject);
            }

            SceneManager.LoadScene("GameScene");
        }

        void OnDestroy()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();

            if (playAgainButton != null)
                playAgainButton.onClick.RemoveAllListeners();
        }
    }
}