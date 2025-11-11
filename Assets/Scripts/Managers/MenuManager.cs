using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Menu UI References")]
        public GameObject menuPanel;
        public Button startGameButton;
        public Button quitGameButton;

        void Start()
        {
            if (menuPanel != null)
                menuPanel.SetActive(true);

            SetupButtons();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void SetupButtons()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveAllListeners();
                startGameButton.onClick.AddListener(StartGame);
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.RemoveAllListeners();
                quitGameButton.onClick.AddListener(QuitGame);
            }
        }

        private void StartGame()
        {
            SceneManager.LoadScene("GameScene");
        }

        private void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        void OnDestroy()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveAllListeners();

            if (quitGameButton != null)
                quitGameButton.onClick.RemoveAllListeners();
        }
    }
}