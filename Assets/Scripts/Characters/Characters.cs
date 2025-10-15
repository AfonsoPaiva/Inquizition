using UnityEngine;
using System.Collections.Generic;

public class Characters : Makedecision
{
    [System.Serializable]
    public class CharacterData
    {
        public List<string> names;
        public List<string> crimes;
        public List<string> descriptions;
    }

    public class Character
    {
        public string characterName;
        public string crime;
        public string description;
        public bool isGuilty;

        public Character(string name, string crimeDesc, string desc, bool guilty)
        {
            characterName = name;
            crime = crimeDesc;
            description = desc;
            isGuilty = guilty;
        }
    }

    public static Character currentCharacter;
    public static GameObject currentCharacterInstance;
    private static System.Random random = new System.Random();
    public TextAsset characterJsonFile;
    public static CharacterData characterData;

    void Awake()
    {
        if (characterJsonFile != null)
        {
            characterData = JsonUtility.FromJson<CharacterData>(characterJsonFile.text);
            Debug.Log($"Loaded {characterData.names.Count} names, {characterData.crimes.Count} crimes, {characterData.descriptions.Count} descriptions");
        }
        else
        {
            Debug.LogError("Character JSON file not assigned in Inspector!");
        }
    }

    public static Character GenerateRandomCharacter()
    {
        if (characterData == null || characterData.names.Count == 0)
        {
            Debug.LogError("Character data not loaded!");
            return null;
        }

        return new Character(
            characterData.names[random.Next(characterData.names.Count)],
            characterData.crimes[random.Next(characterData.crimes.Count)],
            characterData.descriptions[random.Next(characterData.descriptions.Count)],
            random.Next(0, 2) == 1
        );
    }

    public static void SpawnNewCharacter(Makedecision characters, GameObject characterPrefab, Transform spawnPoint)
    {
        if (characterPrefab != null && spawnPoint != null)
        {
            currentCharacterInstance = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

            character_mov newCharMov = currentCharacterInstance.GetComponent<character_mov>();
            if (newCharMov != null)
            {
                newCharMov.makedecision = characters;
            }
        }
        else
        {
            Debug.LogError("Character prefab or spawn point not assigned!");
        }
    }
}