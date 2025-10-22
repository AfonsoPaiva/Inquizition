using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [System.Serializable]
    public class GameStats
    {
        public int population = 100;
        public int fear = 50;
        public int divineFavor = 50;
        public int karma = 50;
        public int gold = 100; 

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

    // Track decision counts for delayed effects
    public int recentExecutions = 0;
    public int recentExiles = 0;
    public int recentForgives = 0;
    public int confiscatedFromInnocents = 0;
    public int currentPrisoners = 0;
    public int tortureCount = 0;
    public int trialByOrdealCount = 0;
    public int publicHumiliationCount = 0;

    // New tracking variables
    public int askGodCount = 0;
    public int corruptionLevel = 0;
    public bool divineFavorCrisis = false;
    public bool godRevealedTruth = false;
    public bool revealedGuiltStatus = false;
    public string lastDecisionType = "";
    public List<string> sparedCharacters = new List<string>();

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
}