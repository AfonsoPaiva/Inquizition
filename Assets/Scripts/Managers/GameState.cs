using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [System.Serializable]
    public class GameStats
    {
        public int population = 45;
        public int fear = 50;
        public int divineFavor = 50;
        public int karma = 50;
        public int gold = 45;

        public void ClampValues()
        {
            population = Mathf.Clamp(population, 0, 100);
            fear = Mathf.Clamp(fear, 0, 100);
            divineFavor = Mathf.Clamp(divineFavor, 0, 100);
            karma = Mathf.Clamp(karma, 0, 100);
            gold = Mathf.Max(0, gold);
        }
    }

    public GameStats currentStats = new GameStats();

    // delayed effects
    public int recentExecutions = 0;
    public int recentExiles = 0;
    public int recentForgives = 0;
    public int confiscatedFromInnocents = 0;
    public int currentPrisoners = 0;
    public int tortureCount = 0;
    public int trialByOrdealCount = 0;
    public int publicHumiliationCount = 0;

    // tracking variables
    public int askGodCount = 0;
    public int corruptionLevel = 0;
    public bool divineFavorCrisis = false;
    public bool godRevealedTruth = false;
    public bool revealedGuiltStatus = false;
    public string lastDecisionType = "";
    public List<string> sparedCharacters = new List<string>();
    public int roundsAt100Population = 0;
    public int roundsAt0Fear = 0;
    public int roundsAt100DivineFavor = 0;
    public bool gameEnded = false;
    public string endingType = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckForEndings()
    {
        if (gameEnded) return;

        // Immediate endings
        if (currentStats.population <= 0)
        {
            TriggerEnding("Population Revolts", "The people have risen up against your tyranny! Your reign has ended in chaos and rebellion.");
            return;
        }

        if (currentStats.fear >= 100)
        {
            TriggerEnding("Maniac Ending", "Your reign of terror is complete. Fear permeates every corner of your realm, but at what cost?");
            return;
        }

        if (currentStats.divineFavor <= 0)
        {
            TriggerEnding("Punished by God", "The gods have forsaken you. Divine wrath strikes down your kingdom!");
            return;
        }

        if (currentStats.karma <= 0)
        {
            TriggerEnding("Karma Struck", "Your evil deeds have caught up with you. The universe has balanced the scales.");
            return;
        }

        if (currentStats.karma >= 100)
        {
            TriggerEnding("Karma Free", "Your virtuous rule has brought perfect harmony. You are enlightened!");
            return;
        }

        // special endings
        if (currentStats.population >= 100)
        {
            roundsAt100Population++;
            if (roundsAt100Population >= 3)
            {
                TriggerEnding("Population Loves You", "The people adore you! Your wisdom and mercy have created a golden age of prosperity.");
                return;
            }
        }
        else
        {
            roundsAt100Population = 0;
        }

        if (currentStats.fear <= 0)
        {
            roundsAt0Fear++;
            if (roundsAt0Fear >= 3)
            {
                TriggerEnding("Relaxed Ending", "Peace reigns supreme. Your gentle rule has created a realm without fear.");
                return;
            }
        }
        else
        {
            roundsAt0Fear = 0;
        }

        if (currentStats.divineFavor >= 100)
        {
            roundsAt100DivineFavor++;
            if (roundsAt100DivineFavor >= 3)
            {
                TriggerEnding("God Loves You", "The divine light shines upon you! The gods have blessed your righteous rule.");
                return;
            }
        }
        else
        {
            roundsAt100DivineFavor = 0;
        }
    }

    private void TriggerEnding(string endingName, string endingDescription)
    {
        gameEnded = true;
        endingType = endingName;

        Debug.Log($"GAME ENDED: {endingName} - {endingDescription}");

        // info for the ending scene
        PlayerPrefs.SetString("EndingName", endingName);
        PlayerPrefs.SetString("EndingDescription", endingDescription);
        PlayerPrefs.SetInt("FinalPopulation", currentStats.population);
        PlayerPrefs.SetInt("FinalFear", currentStats.fear);
        PlayerPrefs.SetInt("FinalDivineFavor", currentStats.divineFavor);
        PlayerPrefs.SetInt("FinalKarma", currentStats.karma);
        PlayerPrefs.SetInt("FinalGold", currentStats.gold);
        PlayerPrefs.Save();
        SceneManager.LoadScene("EndingScene");
    }
}