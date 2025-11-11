using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_mov : MonoBehaviour
{
    public float speed = 5f;
    public float stopDistance = 10f;
    public Makedecision makedecision;

    [Header("Animation Parameters")]
    public string walkAnimationParameter = "isWalking";
    public string idleAnimationParameter = "isIdle";
    public string sadAnimationParameter = "isSad";
    public string celebrateAnimationParameter = "isCelebrating";
    public string turnRightAnimationParameter = "isTurningRight";
    public string turnLeftAnimationParameter = "isTurningLeft";

    private Vector3 startPosition;
    private bool moving = false;
    private bool decisionMade = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator != null)
        {
            animator.applyRootMotion = true;
        }
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

    public static IEnumerator MoveCharacterToOutcome(Vector3 targetPosition, Transform judgmentPoint, bool isGoodOutcome)
    {
        if (Characters.currentCharacterInstance == null) yield break;

        Transform characterTransform = Characters.currentCharacterInstance.transform;
        Animator animator = Characters.currentCharacterInstance.GetComponent<Animator>();
        character_mov charMov = Characters.currentCharacterInstance.GetComponent<character_mov>();

        if (animator == null || charMov == null) yield break;

        charMov.moving = false;
        charMov.decisionMade = true;

        // Step 0: Set to IDLE first (after walking stops)
        animator.applyRootMotion = false;
        SetAnimationBool(animator, charMov.walkAnimationParameter, false);
        SetAnimationBool(animator, charMov.sadAnimationParameter, false);
        SetAnimationBool(animator, charMov.celebrateAnimationParameter, false);
        SetAnimationBool(animator, charMov.turnRightAnimationParameter, false);
        SetAnimationBool(animator, charMov.turnLeftAnimationParameter, false);
        SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        
        yield return new WaitForSeconds(0.5f);

        // Step 1: Play outcome emotion animation WITH ROOT MOTION
        SetAnimationBool(animator, charMov.idleAnimationParameter, false);
        animator.applyRootMotion = true;
        
        string emotionParam = isGoodOutcome ? charMov.celebrateAnimationParameter : charMov.sadAnimationParameter;
        SetAnimationBool(animator, emotionParam, true);

        yield return new WaitForSeconds(2.0f);

        // Step 1.5: Return to IDLE after emotion
        SetAnimationBool(animator, emotionParam, false);
        animator.applyRootMotion = false;
        SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        
        yield return new WaitForSeconds(0.5f);

        // Step 2: Turn animation
        SetAnimationBool(animator, charMov.idleAnimationParameter, false);
        animator.applyRootMotion = true;
        
        string turnParam = isGoodOutcome ? charMov.turnRightAnimationParameter : charMov.turnLeftAnimationParameter;
        SetAnimationBool(animator, turnParam, true);
        
        yield return new WaitForSeconds(2.0f);
        
        SetAnimationBool(animator, turnParam, false);
        yield return new WaitForSeconds(0.3f);

        // Step 3: Walk to destination
        animator.applyRootMotion = false;
        SetAnimationBool(animator, charMov.walkAnimationParameter, true);

        Vector3 startPosition = characterTransform.position;
        float walkDistance = Vector3.Distance(startPosition, targetPosition);
        float duration = walkDistance / charMov.speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {   
            float t = elapsed / duration;
            characterTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.position = targetPosition;
        
        // Step 4: Character destroyed - no need to reset animations
        yield return new WaitForSeconds(0.5f);
        Destroy(Characters.currentCharacterInstance);
        Characters.currentCharacterInstance = null;
    }

    public static IEnumerator MoveQueueForward(Queue<GameObject> characterQueue, Transform spawnPoint, float spacing)
    {
        List<GameObject> characters = new List<GameObject>(characterQueue);
        characterQueue.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            GameObject character = characters[i];
            Animator animator = character.GetComponent<Animator>();
            character_mov charMov = character.GetComponent<character_mov>();
            
            Vector3 targetPosition = spawnPoint.position + (spawnPoint.forward * spacing * (i + 1));

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            if (animator != null && charMov != null)
            {
                SetAnimationBool(animator, charMov.idleAnimationParameter, false);
                SetAnimationBool(animator, charMov.walkAnimationParameter, true);
            }

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

            if (animator != null && charMov != null)
            {
                SetAnimationBool(animator, charMov.walkAnimationParameter, false);
                SetAnimationBool(animator, charMov.idleAnimationParameter, true);
            }

            characterQueue.Enqueue(character);
        }
    }

    public static void SpawnCharacterInQueue(int position, Transform spawnPoint, GameObject characterPrefab, Queue<GameObject> characterQueue, float spacing)
    {
        Vector3 queuePosition = spawnPoint.position + (spawnPoint.forward * spacing * (position + 1));
        Quaternion flippedRotation = spawnPoint.rotation * Quaternion.Euler(0, 180, 0);

        GameObject newCharacter = Instantiate(characterPrefab, queuePosition, flippedRotation);

        character_mov charMov = newCharacter.GetComponent<character_mov>();
        if (charMov != null)
        {
            charMov.enabled = true;
            charMov.moving = false;
            charMov.decisionMade = false;
        }

        Animator animator = newCharacter.GetComponent<Animator>();
        if (animator != null && charMov != null)
        {
            animator.applyRootMotion = false;
            SetAnimationBool(animator, charMov.idleAnimationParameter, true);
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
        Animator animator = nextCharacter.GetComponent<Animator>();
        
        if (animator == null || charMov == null) yield break;

        charMov.moving = false;
        charMov.decisionMade = false;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        Transform characterTransform = nextCharacter.transform;
        Vector3 startPos = characterTransform.position;
        Vector3 targetPos = judgmentPoint.position;

        // Character should already be in idle from spawn, just ensure it
        if (animator != null && charMov != null)
        {
            SetAnimationBool(animator, charMov.walkAnimationParameter, false);
            SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        }

        yield return new WaitForSeconds(0.5f);

        if (animator != null && charMov != null)
        {
            SetAnimationBool(animator, charMov.idleAnimationParameter, false);
            SetAnimationBool(animator, charMov.walkAnimationParameter, true);
        }

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

        if (animator != null && charMov != null)
        {
            SetAnimationBool(animator, charMov.walkAnimationParameter, false);
            SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        }

        godGuidanceRevealed = false;
        revealedGuiltStatus = false;

        if (makedecisionInstance != null && makedecisionInstance.playerCamera != null && judgmentPoint != null)
        {
            yield return makedecisionInstance.StartCoroutine(
                makedecisionInstance.playerCamera.LookAtTransform(judgmentPoint, 1.0f)
            );
        }

        if (makedecisionInstance != null)
        {
            makedecisionInstance.ShowDecision();
        }
    }

    private static void SetAnimationBool(Animator animator, string parameter, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(parameter))
        {
            if (System.Array.Exists(animator.parameters, p => p.name == parameter))
            {
                animator.SetBool(parameter, value);
            }
        }
    }

    void TriggerOtherAction()
    {
        if (makedecision != null)
        {
            makedecision.ShowDecision();
        }
    }

    public void OnDecisionMade()
    {
        decisionMade = true;
        moving = false;
    }
}