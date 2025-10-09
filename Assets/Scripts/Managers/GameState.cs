using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    [System.Serializable]
    public class GameStats
    {
        public int population = 50;
        public int fear = 30;
        public int divineFavor = 50;
        public int karma = 50;
        public int gold = 100;

        public void ClampValues()
        {
            population = Mathf.Clamp(population, 0, 100);
            fear = Mathf.Clamp(fear, 0, 100);
            divineFavor = Mathf.Clamp(divineFavor, 0, 100);
            karma = Mathf.Clamp(karma, 0, 100);
        }
    }

    public GameStats currentStats = new GameStats();

    // Single instance for easy access
    public static GameState Instance { get; private set; }

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