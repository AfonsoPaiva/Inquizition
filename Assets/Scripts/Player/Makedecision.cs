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

    void Start()
    {
        ui = UI.Instance;

        Decisions.InitializeDecisions();

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

        // Start tutorial or regular game
        if (isTutorialActive)
        {
            StartCoroutine(PlayTutorialSequence());
        }
        else
        {
            StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this));
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

        float walkDistance = 10f;
        Vector3 walkDirection = tutorialGuy.transform.forward;
        
        yield return StartCoroutine(MoveTutorialCharacterPositionOnly(tutorialGuy, walkDirection, walkDistance, tutorialMoveDuration));

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
        
        // Wait for turn animation duration (adjust this to match your animation length)
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

        // Stop speaking
        if (tutorialAnimator != null && !string.IsNullOrEmpty(speakAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == speakAnimationParameter))
            {
                tutorialAnimator.SetBool(speakAnimationParameter, false);
            }
        }

        // Wait longer to ensure speak animation fully stops before starting turn left
        yield return new WaitForSeconds(8f);

        // === STEP 4: TURN LEFT ===
        // Make absolutely sure walk is OFF and only turn left is active
        if (tutorialAnimator != null)
        {
            // Turn off ALL other animations first
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
            
            // Now activate turn left
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnLeftAnimationParameter))
            {
                Debug.Log("[Tutorial] Triggering turn left animation");
                tutorialAnimator.SetBool(turnLeftAnimationParameter, true);
            }
            else
            {
                Debug.LogWarning($"[Tutorial] Turn left parameter '{turnLeftAnimationParameter}' not found!");
            }
        }
        
        // Wait for turn animation duration (adjust this to match your animation length)
        yield return new WaitForSeconds(2.0f);
        
        if (tutorialAnimator != null && !string.IsNullOrEmpty(turnLeftAnimationParameter))
        {
            if (System.Array.Exists(tutorialAnimator.parameters, p => p.name == turnLeftAnimationParameter))
            {
                tutorialAnimator.SetBool(turnLeftAnimationParameter, false);
                Debug.Log($"[Tutorial] Turn left complete. Rotation: {tutorialGuy.transform.rotation.eulerAngles}");
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

        // Walk in the NEW direction (after turning left)
        Vector3 exitDirection = tutorialGuy.transform.forward;
        
        yield return StartCoroutine(MoveTutorialCharacterPositionOnly(tutorialGuy, exitDirection, walkDistance, tutorialMoveDuration));

        // Stop walking
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
        StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this));
    }

    private IEnumerator MoveTutorialCharacterPositionOnly(GameObject tutorialGuy, Vector3 direction, float distance, float duration)
    {
        Transform character = tutorialGuy.transform;
        
        Vector3 startPosition = character.position;
        Vector3 targetPosition = startPosition + direction * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Only interpolate position, never touch rotation
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            character.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exact
        character.position = targetPosition;
    }

    private IEnumerator WaitForAnimationState(Animator animator, float normalizedTime)
    {
        // Wait a couple frames for animation to start
        yield return null;
        yield return null;

        // Get current state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Wait until animation reaches the specified normalized time
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

        // Hide all tooltips when making a decision
        ui.HideAllTooltips();

        // Handle Ask God For Guidance special case
        if (decision == Decisions.DecisionType.AskGodForGuidance)
        {
            StartCoroutine(ProcessGodGuidance());
            return;
        }

        Decisions.ExecuteDecision(Characters.currentCharacter, decision);

        // Check for endings immediately after executing decision
        if (GameState.Instance != null)
        {
            GameState.Instance.CheckForEndings();

            // If game ended, stop all processing immediately
            if (GameState.Instance.gameEnded)
            {
                isProcessingDecision = false;

                // Hide UI panels before transitioning
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

        // Hide decision panel temporarily
        if (ui.decisionPanel != null)
            ui.decisionPanel.SetActive(false);

        // Show God's answer in the post-decision panel
        if (ui.postDecisionPanel != null && ui.postDecisionText != null)
        {
            string godMessage = revealedGuiltStatus
                ? " THIS SOUL IS GUILTY \nTheir sins weigh heavy upon the scales of justice."
                : " THIS SOUL IS INNOCENT \nTheir heart is pure, untainted by this crime.";

            ui.postDecisionText.text = godMessage;
            ui.postDecisionPanel.SetActive(true);
        }

        // Wait for player to read the message
        yield return new WaitForSeconds(3f);

        // Hide the message
        if (ui.postDecisionPanel != null)
            ui.postDecisionPanel.SetActive(false);

        // Update crime text to show guilt status
        if (ui.crimeText != null)
        {
            ui.crimeText.text = $"Crime: {Characters.currentCharacter.crime} [{(revealedGuiltStatus ? "GUILTY" : "INNOCENT")}]";
        }

        // Execute the decision (affects stats)
        Decisions.ExecuteDecision(Characters.currentCharacter, Decisions.DecisionType.AskGodForGuidance);

        // Check for endings immediately after executing decision
        if (GameState.Instance != null)
        {
            GameState.Instance.CheckForEndings();

            // If game ended, stop processing
            if (GameState.Instance.gameEnded)
            {
                isProcessingDecision = false;
                yield break;
            }
        }

        // Show decision panel again with updated options (without Ask God For Guidance)
        if (ui.decisionPanel != null)
            ui.decisionPanel.SetActive(true);

        // Refresh the decision buttons to remove "Ask God For Guidance" and show remaining options
        ui.UpdateDecisionButtons();

        isProcessingDecision = false;
    }

    private bool IsGoodOutcome(Decisions.DecisionType decision, bool isGuilty)
    {
        // Merciful/Positive decisions - ALWAYS good outcome (forgiveness is inherently positive)
        if (decision == Decisions.DecisionType.Forgive ||
            decision == Decisions.DecisionType.RedemptionQuest ||
            decision == Decisions.DecisionType.Corruption ||
            decision == Decisions.DecisionType.AcceptBribe||
            decision == Decisions.DecisionType.SpareWithWarning)
        {
            return true; // Always celebrate mercy and forgiveness
        }

        // Harsh/Immoral decisions - NEVER a good outcome (always sad)
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
            return false; // Always sad - these are morally questionable
        }



        // Ask God for Guidance - Neutral/Random
        if (decision == Decisions.DecisionType.AskGodForGuidance)
        {
            return random.Next(2) == 0; // 50/50
        }

        // Default fallback
        return random.Next(2) == 0;
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
        
        // Pass isGoodOutcome to MoveCharacterToOutcome
        yield return StartCoroutine(character_mov.MoveCharacterToOutcome(characterOutcomePoint.position, judgmentPoint, isGoodOutcome));
        
        yield return StartCoroutine(camera_mov.ReturnCameraToPlayer(playerCamera, originalCameraPosition, originalCameraRotation));

        if (Characters.currentCharacterInstance != null)
        {
            Destroy(Characters.currentCharacterInstance);
            Characters.currentCharacterInstance = null;
        }

        character_mov.SpawnCharacterInQueue(queueSize - 1, spawnPoint, characterPrefab, characterQueue, spacing);
        yield return StartCoroutine(character_mov.MoveQueueForward(characterQueue, spawnPoint, spacing));

        // Reset god guidance flag for new character
        godGuidanceRevealed = false;
        revealedGuiltStatus = false;

        isProcessingDecision = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this));
    }

    public void ShowDecision()
    {
        ui.ShowDecision();
    }
}