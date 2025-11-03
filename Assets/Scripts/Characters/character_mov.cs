using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_mov : MonoBehaviour
{
    public float speed = 5f;
    public float stopDistance = 10f;
    public Makedecision makedecision;

    private Vector3 startPosition;
    private bool moving = true;
    private bool decisionMade = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (moving && !decisionMade)
        {
            transform.Translate(-Vector3.forward * speed * Time.deltaTime);

            float distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled >= stopDistance)
            {
                moving = false;
                TriggerOtherAction();
            }
        }
    }

    public static IEnumerator MoveCharacterToOutcome(Vector3 targetPosition, Transform judgmentPoint)
    {
        if (Characters.currentCharacterInstance == null) yield break;

        Transform characterTransform = Characters.currentCharacterInstance.transform;
        Vector3 startPosition = characterTransform.position;
        Quaternion startRotation = characterTransform.rotation;

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            characterTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            characterTransform.rotation = Quaternion.Lerp(startRotation, judgmentPoint.rotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.position = targetPosition;
        characterTransform.rotation = judgmentPoint.rotation;
    }

    public static IEnumerator MoveQueueForward(Queue<GameObject> characterQueue, Transform spawnPoint, float spacing)
    {
        List<GameObject> characters = new List<GameObject>(characterQueue);
        characterQueue.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            GameObject character = characters[i];
            Vector3 targetPosition = spawnPoint.position + (spawnPoint.forward * spacing * (i + 1));

            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPosition = character.transform.position;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                character.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            character.transform.position = targetPosition;
            characterQueue.Enqueue(character);
        }
    }

    public static void SpawnCharacterInQueue(int position, Transform spawnPoint, GameObject characterPrefab, Queue<GameObject> characterQueue, float spacing)
    {
        Vector3 queuePosition = spawnPoint.position + (spawnPoint.forward * spacing * (position + 1));
        GameObject newCharacter = Instantiate(characterPrefab, queuePosition, spawnPoint.rotation);

        character_mov charMov = newCharacter.GetComponent<character_mov>();
        if (charMov != null)
        {
            charMov.enabled = false;
        }

        characterQueue.Enqueue(newCharacter);
    }


    public static IEnumerator MoveToJudgmentPosition(
        Queue<GameObject> characterQueue,
        bool isProcessingDecision,
        Transform judgmentPoint,
        bool godGuidanceRevealed,
        bool revealedGuiltStatus,
        Makedecision makedecisionInstance
    )
    {
        if (characterQueue.Count == 0 || isProcessingDecision) yield break;

        GameObject nextCharacter = characterQueue.Dequeue();
        Characters.currentCharacterInstance = nextCharacter;
        Characters.currentCharacter = Characters.GenerateRandomCharacter();

        character_mov charMov = nextCharacter.GetComponent<character_mov>();
        if (charMov != null)
        {
            charMov.enabled = true;
            charMov.makedecision = charMov.GetComponent<Makedecision>();
        }

        Transform characterTransform = nextCharacter.transform;
        Vector3 startPos = characterTransform.position;
        Vector3 targetPos = judgmentPoint.position;

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            characterTransform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.position = targetPos;

        // Reset god guidance for new character
        godGuidanceRevealed = false;
        revealedGuiltStatus = false;

        if (makedecisionInstance != null)
        {
            makedecisionInstance.ShowDecision();
        }
    }

    void TriggerOtherAction()
    {
        // Only trigger if makedecision is assigned
        if (makedecision != null)
        {
            makedecision.ShowDecision();
        }

    }

    // This will be called by Makedecision after a choice is made
    public void OnDecisionMade()
    {
        decisionMade = true;
        // Destruction is now handled by Makedecision script
    }
}