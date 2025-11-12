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

    public static IEnumerator MoveCharacterToOutcome(
        Vector3 targetPosition, 
        Transform judgmentPoint, 
        bool isGoodOutcome,
        AudioClip walkSound,
        float walkVolume,
        AudioClip crySound,
        AudioClip celebrateSound,
        float emotionVolume,
        AudioClip decisionSound,
        float decisionSoundVolume)
    {
        if (Characters.currentCharacterInstance == null) yield break;

        Transform characterTransform = Characters.currentCharacterInstance.transform;
        Animator animator = Characters.currentCharacterInstance.GetComponent<Animator>();
        character_mov charMov = Characters.currentCharacterInstance.GetComponent<character_mov>();

        if (animator == null || charMov == null) yield break;

        // Get or add AudioSource
        AudioSource audioSource = Characters.currentCharacterInstance.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = Characters.currentCharacterInstance.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 50f;

        charMov.moving = false;
        charMov.decisionMade = true;

        animator.applyRootMotion = false;
        SetAnimationBool(animator, charMov.walkAnimationParameter, false);
        SetAnimationBool(animator, charMov.sadAnimationParameter, false);
        SetAnimationBool(animator, charMov.celebrateAnimationParameter, false);
        SetAnimationBool(animator, charMov.turnRightAnimationParameter, false);
        SetAnimationBool(animator, charMov.turnLeftAnimationParameter, false);
        SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        
        yield return new WaitForSeconds(0.5f);

        SetAnimationBool(animator, charMov.idleAnimationParameter, false);
        animator.applyRootMotion = true;
        
        string emotionParam = isGoodOutcome ? charMov.celebrateAnimationParameter : charMov.sadAnimationParameter;
        AudioClip emotionSound = isGoodOutcome ? celebrateSound : crySound;
        
        SetAnimationBool(animator, emotionParam, true);
        
        if (audioSource != null && emotionSound != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(emotionSound, emotionVolume);
        }

        yield return new WaitForSeconds(2.0f);

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        SetAnimationBool(animator, emotionParam, false);
        animator.applyRootMotion = false;
        SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        
        yield return new WaitForSeconds(0.5f);

        SetAnimationBool(animator, charMov.idleAnimationParameter, false);
        animator.applyRootMotion = true;
        
        string turnParam = isGoodOutcome ? charMov.turnRightAnimationParameter : charMov.turnLeftAnimationParameter;
        SetAnimationBool(animator, turnParam, true);
        
        yield return new WaitForSeconds(2.0f);
        
        SetAnimationBool(animator, turnParam, false);
        yield return new WaitForSeconds(0.3f);

        animator.applyRootMotion = false;
        SetAnimationBool(animator, charMov.walkAnimationParameter, true);
        
        if (audioSource != null && walkSound != null)
        {
            audioSource.loop = true;
            audioSource.clip = walkSound;
            audioSource.volume = walkVolume;
            audioSource.Play();
        }

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
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        SetAnimationBool(animator, charMov.walkAnimationParameter, false);
        SetAnimationBool(animator, charMov.idleAnimationParameter, true);
        
        if (audioSource != null && decisionSound != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(decisionSound, decisionSoundVolume);
            yield return new WaitForSeconds(decisionSound.length); // Wait for sound to finish
        }
        
        yield return new WaitForSeconds(0.5f);
        Destroy(Characters.currentCharacterInstance);
        Characters.currentCharacterInstance = null;
    }

    public static IEnumerator MoveQueueForward(Queue<GameObject> characterQueue, Transform spawnPoint, float spacing, AudioClip walkSound = null, float walkVolume = 0.7f)
    {
        List<GameObject> characters = new List<GameObject>(characterQueue);
        characterQueue.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            GameObject character = characters[i];
            Animator animator = character.GetComponent<Animator>();
            character_mov charMov = character.GetComponent<character_mov>();
            
            Vector3 targetPosition = spawnPoint.position + (spawnPoint.forward * spacing * (i + 1));

            // Get or add AudioSource
            AudioSource audioSource = character.GetComponent<AudioSource>();
            if (audioSource == null && walkSound != null)
            {
                audioSource = character.AddComponent<AudioSource>();
                audioSource.loop = true;
                audioSource.spatialBlend = 1f;
                audioSource.maxDistance = 50f;
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }

            if (animator != null && charMov != null)
            {
                SetAnimationBool(animator, charMov.idleAnimationParameter, false);
                SetAnimationBool(animator, charMov.walkAnimationParameter, true);
            }

            if (audioSource != null && walkSound != null)
            {
                audioSource.clip = walkSound;
                audioSource.volume = walkVolume;
                audioSource.Play();
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

            if (audioSource != null)
            {
                audioSource.Stop();
            }

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
        Makedecision makedecisionInstance,
        AudioClip walkSound = null,
        float walkVolume = 0.7f
    )
    {
        if (characterQueue.Count == 0 || isProcessingDecision) yield break;

        GameObject nextCharacter = characterQueue.Dequeue();
        Characters.currentCharacterInstance = nextCharacter;
        Characters.currentCharacter = Characters.GenerateRandomCharacter();

        character_mov charMov = nextCharacter.GetComponent<character_mov>();
        Animator animator = nextCharacter.GetComponent<Animator>();
        
        if (animator == null || charMov == null) yield break;

        AudioSource audioSource = nextCharacter.GetComponent<AudioSource>();
        if (audioSource == null && walkSound != null)
        {
            audioSource = nextCharacter.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 50f;
        }

        charMov.moving = false;
        charMov.decisionMade = false;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        Transform characterTransform = nextCharacter.transform;
        Vector3 startPos = characterTransform.position;
        Vector3 targetPos = judgmentPoint.position;

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

        if (audioSource != null && walkSound != null)
        {
            audioSource.clip = walkSound;
            audioSource.volume = walkVolume;
            audioSource.Play();
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

        if (audioSource != null)
        {
            audioSource.Stop();
        }

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