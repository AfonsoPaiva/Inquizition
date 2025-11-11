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
        public Button mainMenuButton;
        public Button playAgainButton;

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
                finalStatsText.text = $"Final Stats:\n\n" +
                                     $"Population: {finalPopulation}\n" +
                                     $"Fear: {finalFear}\n" +
                                     $"Divine Favor: {finalDivineFavor}\n" +
                                     $"Karma: {finalKarma}\n" +
                                     $"Gold: {finalGold}";
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