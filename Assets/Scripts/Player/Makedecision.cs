using Assets.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class Makedecision : MonoBehaviour
{

    [Header("Game References")]
    public camera_mov playerCamera;
    public Transform cameraFocusPoint;

    [Header("Character Management")]
    public GameObject characterPrefab;
    public Transform spawnPoint;
    public Transform judgmentPoint;
    public Transform pointGood;
    public Transform pointBad;

    [Header("Queue Settings")]
    public int queueSize = 5;
    public float spacing = 2f;

    [Header("Camera Points")]
    public Transform cameraGoodPoint;
    public Transform cameraBadPoint;

    [Header("Tutorial Settings")]
    public bool isTutorialActive = true;
    public GameObject tutorialPrefab;
    public float tutorialMoveDuration = 8f;
    public Transform tutorialGuySpawn;
    public GameObject tutorialMessagePanel;
    public float minimumSpeakDuration = 3.0f;

    [Header("Tutorial Animation")]
    public string walkAnimationParameter = "isWalking";
    public string turnRightAnimationParameter = "isTurningRight";
    public string turnLeftAnimationParameter = "isTurningLeft";
    public string speakAnimationParameter = "isSpeaking";

    [Header("Tutorial Audio")]
    public AudioClip footstepSound;
    public AudioClip tutorialSpeakingSound;
    [Range(0f, 1f)] public float tutorialFootstepVolume = 0.7f;
    [Range(0f, 1f)] public float tutorialSpeakingVolume = 0.7f;

    [Header("Character Audio Clips")]
    public AudioClip characterWalkSound;
    public AudioClip characterCrySound;
    public AudioClip characterCelebrateSound;
    [Range(0f, 1f)] public float characterWalkVolume = 0.7f;
    [Range(0f, 1f)] public float characterEmotionVolume = 0.7f;

    [Header("UI Sound Effects")]
    public AudioClip decisionCanvasAppearSound;
    [Range(0f, 1f)] public float uiSoundVolume = 0.8f;

    [Header("Decision Outcome Sound Effects")]
    public AudioClip executeSound;
    public AudioClip exileSound;
    public AudioClip forgiveSound;
    public AudioClip confiscateSound;
    public AudioClip imprisonSound;
    public AudioClip tortureSound;
    public AudioClip trialByOrdealSound;
    public AudioClip redemptionQuestSound;
    public AudioClip publicHumiliationSound;
    public AudioClip banishWildernessSound;
    public AudioClip spareWithWarningSound;
    public AudioClip acceptBribeSound;
    public AudioClip collectivePunishmentSound;
    public AudioClip sacrificeToGodSound;
    public AudioClip corruptionSound;
    public AudioClip askGodForGuidanceSound;
    [Range(0f, 1f)] public float decisionSoundVolume = 0.8f;

    public Dictionary<Decisions.DecisionType, string> decisionEffectsCache = new Dictionary<Decisions.DecisionType, string>();

    private System.Random random = new System.Random();

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;

    public Queue<GameObject> characterQueue = new Queue<GameObject>();
    public bool isProcessingDecision = false;
    public bool godGuidanceRevealed = false;
    public bool revealedGuiltStatus = false;

    private UI ui;
    private AudioSource uiAudioSource;

    void Start()
    {
        ui = UI.Instance;

        Decisions.InitializeDecisions();

        // Create AudioSource for UI sounds
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f; // 2D sound

        // Ensure cursor is visible and unlocked for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerCamera != null)
        {
            // Store WORLD position and rotation (not local)
            originalCameraPosition = playerCamera.transform.position;
            originalCameraRotation = playerCamera.transform.rotation;
            originalCameraParent = playerCamera.transform.parent;
        }

        // Create initial queue
        for (int i = 0; i < queueSize; i++)
        {
            character_mov.SpawnCharacterInQueue(i, spawnPoint, characterPrefab, characterQueue, spacing);
        }

        // Check if tutorial has been shown in this session (NOT persistent across game restarts)
        bool tutorialShownThisSession = PlayerPrefs.GetInt("TutorialShownThisSession", 0) == 1;

        // Start tutorial or regular game
        if (isTutorialActive && !tutorialShownThisSession)
        {
            // Mark tutorial as shown for this session
            PlayerPrefs.SetInt("TutorialShownThisSession", 1);
            PlayerPrefs.Save();
            StartCoroutine(PlayTutorialSequence());
        }
        else
        {
            StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this, characterWalkSound, characterWalkVolume));
        }
    }

    void Update()
    {
        // Ensure cursor stays visible and unlocked (in case something else tries to lock it)
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }

    private IEnumerator PlayTutorialSequence()
    {
        // Ensure cursor is visible for tutorial
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 1. Spawn tutorial character at spawn point
        if (tutorialPrefab == null || tutorialGuySpawn == null)
        {
            Debug.LogError("Tutorial Prefab or Tutorial Guy Spawn is not assigned!");
            isTutorialActive = false;
            yield break;
        }

        GameObject tutorialGuy = Instantiate(tutorialPrefab, tutorialGuySpawn.position, tutorialGuySpawn.rotation);

        // Get Animator component
        Animator tutorialAnimator = tutorialGuy.GetComponent<Animator>();

        if (tutorialAnimator == null)
        {
            Debug.LogError("Tutorial prefab has no Animator component!");
            yield break;
        }

        // Get or add AudioSource component
        AudioSource tutorialAudioSource = tutorialGuy.GetComponent<AudioSource>();
        if (tutorialAudioSource == null)
        {
            tutorialAudioSource = tutorialGuy.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource
        tutorialAudioSource.loop = true;
        tutorialAudioSource.playOnAwake = false;
        tutorialAudioSource.spatialBlend = 1f; // 3D sound
        tutorialAudioSource.maxDistance = 50f;
        
        // Enable root motion so animation controls rotation
        tutorialAnimator.applyRootMotion = true;

        // Disable character_mov component if it exists
        character_mov tutorialCharMov = tutorialGuy.GetComponent<character_mov>();
        if (tutorialCharMov != null)
        {
            tutorialCharMov.enabled = false;
        }

        // Small delay to ensure character is fully spawned
        yield return new WaitForSeconds(0.2f);

        // 2. Camera focuses on the spawned tutorial character
        if (playerCamera != null && tutorialGuy != null)
        {
            yield return StartCoroutine(playerCamera.LookAtTransform(tutorialGuy.transform, 1.0f));
        }

        // === STEP 1: WALK FORWARD ===
        if (tutorialAnimator != null && !string.IsNullOrWhiteSpace(walkAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == walkAnimationParameter))
            {
                tutorialAnimator.SetBool(walkAnimationParameter, true);
            }
        }

        // Start playing footstep sound (continuous loop)
        if (tutorialAudioSource != null && footstepSound != null)
        {
            tutorialAudioSource.loop = true;
            tutorialAudioSource.clip = footstepSound;
            tutorialAudioSource.volume = tutorialFootstepVolume;
            tutorialAudioSource.Play();
        }
        
        float walkDistance = 10f;
        Vector3 walkDirection = tutorialGuy.transform.forward;
        
        yield return StartCoroutine(MoveTutorialCharacterPositionOnly(tutorialGuy, walkDirection, walkDistance, tutorialMoveDuration));

        // Stop footstep sound
        if (tutorialAudioSource != null)
        {
            tutorialAudioSource.Stop();
        }

        // Stop walking
        if (tutorialAnimator != null && !string.IsNullOrWhiteSpace(walkAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == walkAnimationParameter))
            {
                tutorialAnimator.SetBool(walkAnimationParameter, false);
            }
        }
        
        yield return new WaitForSeconds(0.5f);

        // === STEP 2: TURN RIGHT ===
        if (tutorialAnimator != null && !string.IsNullOrEmpty(turnRightAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnRightAnimationParameter))
            {
                tutorialAnimator.SetBool(turnRightAnimationParameter, true);
            }
        }
        
        yield return new WaitForSeconds(2.0f);
        
        if (tutorialAnimator != null && !string.IsNullOrEmpty(turnRightAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnRightAnimationParameter))
            {
                tutorialAnimator.SetBool(turnRightAnimationParameter, false);
            }
        }
        
        yield return new WaitForSeconds(0.5f);

        // === STEP 3: TALK ===
        if (tutorialAnimator != null && !string.IsNullOrEmpty(speakAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == speakAnimationParameter))
            {
                tutorialAnimator.SetBool(speakAnimationParameter, true);
            }
        }

        // Play speaking sound as ONE-SHOT (not looping)
        if (tutorialAudioSource != null && tutorialSpeakingSound != null)
        {
            tutorialAudioSource.loop = false;
            tutorialAudioSource.PlayOneShot(tutorialSpeakingSound, tutorialSpeakingVolume);
        }

        // Camera focuses on tutorial character's upper body/head for speaking
        if (playerCamera != null && tutorialGuy != null)
        {
            GameObject tempTarget = new GameObject("TempCameraTarget");
            tempTarget.transform.position = tutorialGuy.transform.position + Vector3.up * 1.5f;

            yield return StartCoroutine(playerCamera.LookAtTransform(tempTarget.transform, 1.5f));

            Destroy(tempTarget);
        }

        yield return new WaitForSeconds(0.5f);

        // Display message panel and wait for spacebar
        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        if (tutorialMessagePanel != null)
        {   
            tutorialMessagePanel.SetActive(false);
        }

        // Stop speaking sound (if still playing)
        if (tutorialAudioSource != null)
        {
            tutorialAudioSource.Stop();
        }

        // Stop speaking
        if (tutorialAnimator != null && !string.IsNullOrEmpty(speakAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == speakAnimationParameter))
            {
                tutorialAnimator.SetBool(speakAnimationParameter, false);
            }
        }

        yield return new WaitForSeconds(8f);

        // === STEP 4: TURN LEFT ===
        if (tutorialAnimator != null)
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == walkAnimationParameter))
            {
                tutorialAnimator.SetBool(walkAnimationParameter, false);
            }
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnRightAnimationParameter))
            {
                tutorialAnimator.SetBool(turnRightAnimationParameter, false);
            }
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == speakAnimationParameter))
            {
                tutorialAnimator.SetBool(speakAnimationParameter, false);
            }
            
            yield return new WaitForSeconds(0.2f);
            
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnLeftAnimationParameter))
            {
                tutorialAnimator.SetBool(turnLeftAnimationParameter, true);
            }
        }
        
        yield return new WaitForSeconds(2.0f);
        
        if (tutorialAnimator != null && !string.IsNullOrEmpty(turnLeftAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnLeftAnimationParameter))
            {
                tutorialAnimator.SetBool(turnLeftAnimationParameter, false);
            }
        }
        
        yield return new WaitForSeconds(0.5f);

        // === STEP 5: WALK FORWARD (in new direction) ===
        if (tutorialAnimator != null && !string.IsNullOrEmpty(walkAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == walkAnimationParameter))
            {
                tutorialAnimator.SetBool(walkAnimationParameter, true);
            }
        }

        // Start playing footstep sound again (looping for walking)
        if (tutorialAudioSource != null && footstepSound != null)
        {
            tutorialAudioSource.loop = true;
            tutorialAudioSource.clip = footstepSound;
            tutorialAudioSource.volume = tutorialFootstepVolume;
            tutorialAudioSource.Play();
        }

        Vector3 exitDirection = tutorialGuy.transform.forward;
        
        yield return StartCoroutine(MoveTutorialCharacterPositionOnly(tutorialGuy, exitDirection, walkDistance, tutorialMoveDuration));

        if (tutorialAudioSource != null)
        {
            tutorialAudioSource.Stop();
        }

        if (tutorialAnimator != null && !string.IsNullOrEmpty(walkAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == walkAnimationParameter))
            {
                tutorialAnimator.SetBool(walkAnimationParameter, false);
            }
        }

        yield return new WaitForSeconds(0.5f);

        Destroy(tutorialGuy);
        isTutorialActive = false;

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this, characterWalkSound, characterWalkVolume));
    }

    private IEnumerator MoveTutorialCharacterPositionOnly(GameObject tutorialGuy, Vector3 direction, float distance, float duration)
    {
        Transform character = tutorialGuy.transform;
        
        Vector3 startPosition = character.position;
        Vector3 targetPosition = startPosition + direction * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            character.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        character.position = targetPosition;
    }

    private IEnumerator WaitForAnimationState(Animator animator, float normalizedTime)
    {
        yield return null;
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        while (stateInfo.normalizedTime < normalizedTime || animator.IsInTransition(0))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
    }
  
    public void MakeDecisionAtIndex(int buttonIndex)
    {
        if (isProcessingDecision) return;

        List<Decisions.DecisionType> currentDecisions = Decisions.GetCurrentActiveDecisions();

        if (buttonIndex < currentDecisions.Count)
        {
            Decisions.DecisionType selectedDecision = currentDecisions[buttonIndex];
            MakeDecision(selectedDecision);
        }
    }

    private void MakeDecision(Decisions.DecisionType decision)
    {
        isProcessingDecision = true;

        ui.HideAllTooltips();

        // Handle Ask God For Guidance special case - PLAY SOUND IMMEDIATELY
        if (decision == Decisions.DecisionType.AskGodForGuidance)
        {
            // Play Ask God sound immediately when button is pressed
            if (uiAudioSource != null && askGodForGuidanceSound != null)
            {
                uiAudioSource.PlayOneShot(askGodForGuidanceSound, decisionSoundVolume);
            }
            
            StartCoroutine(ProcessGodGuidance());
            return;
        }

        Decisions.ExecuteDecision(Characters.currentCharacter, decision);

        if (GameState.Instance != null)
        {
            GameState.Instance.CheckForEndings();

            if (GameState.Instance.gameEnded)
            {
                isProcessingDecision = false;

                if (ui.decisionPanel != null)
                    ui.decisionPanel.SetActive(false);

                return;
            }
        }

        if (ui.decisionPanel != null)
            ui.decisionPanel.SetActive(false);

        bool isGoodOutcome = IsGoodOutcome(decision, Characters.currentCharacter.isGuilty);
        StartCoroutine(ProcessDecisionOutcome(isGoodOutcome, decision));
    }

    private IEnumerator ProcessGodGuidance()
    {
        godGuidanceRevealed = true;
        revealedGuiltStatus = Characters.currentCharacter.isGuilty;

        if (ui.decisionPanel != null)
            ui.decisionPanel.SetActive(false);

        if (ui.postDecisionPanel != null && ui.postDecisionText != null)
        {
            string godMessage = revealedGuiltStatus
                ? " THIS SOUL IS GUILTY \nTheir sins weigh heavy upon the scales of justice."
                : " THIS SOUL IS INNOCENT \nTheir heart is pure, untainted by this crime.";

            ui.postDecisionText.text = godMessage;
            ui.postDecisionPanel.SetActive(true);
        }

        yield return new WaitForSeconds(3f);

        if (ui.postDecisionPanel != null)
            ui.postDecisionPanel.SetActive(false);

        if (ui.crimeText != null)
        {
            ui.crimeText.text = $"Crime: {Characters.currentCharacter.crime} [{(revealedGuiltStatus ? "GUILTY" : "INNOCENT")}]";
        }

        Decisions.ExecuteDecision(Characters.currentCharacter, Decisions.DecisionType.AskGodForGuidance);

        if (GameState.Instance != null)
        {
            GameState.Instance.CheckForEndings();

            if (GameState.Instance.gameEnded)
            {
                isProcessingDecision = false;
                yield break;
            }
        }

        if (ui.decisionPanel != null)
            ui.decisionPanel.SetActive(true);

        ui.UpdateDecisionButtons();

        isProcessingDecision = false;
    }

    private bool IsGoodOutcome(Decisions.DecisionType decision, bool isGuilty)
    {
        if (decision == Decisions.DecisionType.Forgive ||
            decision == Decisions.DecisionType.RedemptionQuest ||
            decision == Decisions.DecisionType.Corruption ||
            decision == Decisions.DecisionType.AcceptBribe||
            decision == Decisions.DecisionType.SpareWithWarning)
        {
            return true;
        }

        if (decision == Decisions.DecisionType.Execute ||
            decision == Decisions.DecisionType.Torture ||
            decision == Decisions.DecisionType.Imprison ||
            decision == Decisions.DecisionType.Confiscate ||
            decision == Decisions.DecisionType.PublicHumiliation ||
            decision == Decisions.DecisionType.TrialByOrdeal ||
            decision == Decisions.DecisionType.SacrificeToGod ||
            decision == Decisions.DecisionType.Exile ||
            decision == Decisions.DecisionType.BanishWilderness ||
            decision == Decisions.DecisionType.CollectivePunishment)
        {
            return false;
        }

        if (decision == Decisions.DecisionType.AskGodForGuidance)
        {
            return random.Next(2) == 0;
        }

        return random.Next(2) == 0;
    }

    private AudioClip GetDecisionSound(Decisions.DecisionType decision)
    {
        switch (decision)
        {
            case Decisions.DecisionType.Execute:
                return executeSound;
            case Decisions.DecisionType.Exile:
                return exileSound;
            case Decisions.DecisionType.Forgive:
                return forgiveSound;
            case Decisions.DecisionType.Confiscate:
                return confiscateSound;
            case Decisions.DecisionType.Imprison:
                return imprisonSound;
            case Decisions.DecisionType.Torture:
                return tortureSound;
            case Decisions.DecisionType.TrialByOrdeal:
                return trialByOrdealSound;
            case Decisions.DecisionType.RedemptionQuest:
                return redemptionQuestSound;
            case Decisions.DecisionType.PublicHumiliation:
                return publicHumiliationSound;
            case Decisions.DecisionType.BanishWilderness:
                return banishWildernessSound;
            case Decisions.DecisionType.SpareWithWarning:
                return spareWithWarningSound;
            case Decisions.DecisionType.AcceptBribe:
                return acceptBribeSound;
            case Decisions.DecisionType.CollectivePunishment:
                return collectivePunishmentSound;
            case Decisions.DecisionType.SacrificeToGod:
                return sacrificeToGodSound;
            case Decisions.DecisionType.Corruption:
                return corruptionSound;
            case Decisions.DecisionType.AskGodForGuidance:
                return null; // Handled separately when button is pressed
            default:
                return null;
        }
    }

    public IEnumerator ProcessDecisionOutcome(bool isGoodOutcome, Decisions.DecisionType decision)
    {
        UI.ShowPostDecisionDialogue(decision);
        yield return new WaitForSeconds(1f);

        Transform cameraOutcomePoint = isGoodOutcome ? cameraGoodPoint : cameraBadPoint;
        yield return StartCoroutine(playerCamera.MoveCameraToOutcome(cameraOutcomePoint, playerCamera));
        yield return new WaitForSeconds(0.5f);

        if (ui.postDecisionPanel != null)
            ui.postDecisionPanel.SetActive(false);

        Transform characterOutcomePoint = isGoodOutcome ? pointGood : pointBad;
        
        // Get decision-specific sound
        AudioClip decisionSound = GetDecisionSound(decision);
        
        // Pass all audio parameters including decision sound
        yield return StartCoroutine(character_mov.MoveCharacterToOutcome(
            characterOutcomePoint.position, 
            judgmentPoint, 
            isGoodOutcome, 
            characterWalkSound,
            characterWalkVolume,
            characterCrySound, 
            characterCelebrateSound,
            characterEmotionVolume,
            decisionSound,
            decisionSoundVolume
        ));
        
        yield return StartCoroutine(camera_mov.ReturnCameraToPlayer(playerCamera, originalCameraPosition, originalCameraRotation));

        if (Characters.currentCharacterInstance != null)
        {
            Destroy(Characters.currentCharacterInstance);
            Characters.currentCharacterInstance = null;
        }

        character_mov.SpawnCharacterInQueue(queueSize - 1, spawnPoint, characterPrefab, characterQueue, spacing);
        yield return StartCoroutine(character_mov.MoveQueueForward(characterQueue, spawnPoint, spacing, characterWalkSound, characterWalkVolume));

        godGuidanceRevealed = false;
        revealedGuiltStatus = false;

        isProcessingDecision = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this, characterWalkSound, characterWalkVolume));
    }

    public void ShowDecision()
    {
        // Play decision canvas appear sound
        if (uiAudioSource != null && decisionCanvasAppearSound != null)
        {
            uiAudioSource.PlayOneShot(decisionCanvasAppearSound, uiSoundVolume);
        }
        
        ui.ShowDecision();
    }
}