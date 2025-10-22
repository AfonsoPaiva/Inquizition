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
        public List<string> crimeSeverity;
        public PostDecisionDialogue postDecisionDialogue;
        public CharacterContext characterContext;
    }

    [System.Serializable]
    public class PostDecisionDialogue
    {
        public List<string> execute;
        public List<string> exile;
        public List<string> forgive;
        public List<string> confiscate;
        public List<string> imprison;
        public List<string> torture;
        public List<string> spare;
        public List<string> sacrifice;
        public List<string> corruption;
        public List<string> divine_guidance;
    }

    [System.Serializable]
    public class CharacterContext
    {
        public List<bool> firstTimeOffender;
        public List<bool> showsRemorse;
        public List<bool> offersBribe;
        public List<bool> partOfConspiracy;
    }

    public class Character
    {
        public string characterName;
        public string crime;
        public string description;
        public bool isGuilty;
        public string crimeSeverity;
        public bool isFirstTimeOffender;
        public bool showsRemorse;
        public bool offersBribe;
        public bool isPartOfConspiracy;

        public Character(string name, string crimeDesc, string desc, bool guilty,
                       string severity, bool firstTime, bool remorse, bool bribe, bool conspiracy)
        {
            characterName = name;
            crime = crimeDesc;
            description = desc;
            isGuilty = guilty;
            crimeSeverity = severity;
            isFirstTimeOffender = firstTime;
            showsRemorse = remorse;
            offersBribe = bribe;
            isPartOfConspiracy = conspiracy;
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

        int index = random.Next(characterData.names.Count);

        // Get context data with bounds checking
        bool firstTime = index < characterData.characterContext.firstTimeOffender.Count ?
            characterData.characterContext.firstTimeOffender[index] : random.Next(2) == 0;

        bool remorse = index < characterData.characterContext.showsRemorse.Count ?
            characterData.characterContext.showsRemorse[index] : random.Next(2) == 0;

        bool bribe = index < characterData.characterContext.offersBribe.Count ?
            characterData.characterContext.offersBribe[index] : random.Next(2) == 0;

        bool conspiracy = index < characterData.characterContext.partOfConspiracy.Count ?
            characterData.characterContext.partOfConspiracy[index] : random.Next(2) == 0;

        string severity = index < characterData.crimeSeverity.Count ?
            characterData.crimeSeverity[index] : "moderate";

        return new Character(
            characterData.names[index],
            characterData.crimes[index],
            characterData.descriptions[index],
            random.Next(0, 2) == 1,
            severity,
            firstTime,
            remorse,
            bribe,
            conspiracy
        );
    }

    public static string GetPostDecisionDialogue(string decisionType)
    {
        if (characterData?.postDecisionDialogue == null)
            return "The decision has been made.";

        List<string> dialogueList = null;

        switch (decisionType.ToLower())
        {
            case "execute": dialogueList = characterData.postDecisionDialogue.execute; break;
            case "exile": dialogueList = characterData.postDecisionDialogue.exile; break;
            case "forgive": dialogueList = characterData.postDecisionDialogue.forgive; break;
            case "confiscate": dialogueList = characterData.postDecisionDialogue.confiscate; break;
            case "imprison": dialogueList = characterData.postDecisionDialogue.imprison; break;
            case "torture": dialogueList = characterData.postDecisionDialogue.torture; break;
            case "spare": dialogueList = characterData.postDecisionDialogue.spare; break;
            case "sacrifice": dialogueList = characterData.postDecisionDialogue.sacrifice; break;
            case "corruption": dialogueList = characterData.postDecisionDialogue.corruption; break;
            case "divine_guidance": dialogueList = characterData.postDecisionDialogue.divine_guidance; break;
        }

        return dialogueList != null && dialogueList.Count > 0 ?
            dialogueList[random.Next(dialogueList.Count)] : "The decision has been made.";
    }

    // Existing SpawnNewCharacter method remains the same
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
    }
}