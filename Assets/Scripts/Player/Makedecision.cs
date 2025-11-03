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
    public  float spacing = 2f;

    [Header("Camera Points")]
    public Transform cameraGoodPoint;
    public Transform cameraBadPoint;

    public  Dictionary<Decisions.DecisionType, string> decisionEffectsCache = new Dictionary<Decisions.DecisionType, string>();

    private  System.Random random = new System.Random();

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;

    public Queue<GameObject> characterQueue = new Queue<GameObject>();
    public  bool isProcessingDecision = false;
    public  bool godGuidanceRevealed = false;
    public  bool revealedGuiltStatus = false;

    private UI ui;

    void Start()
    {
        ui = UI.Instance;

        Decisions.InitializeDecisions();




        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.position;
            originalCameraRotation = playerCamera.transform.rotation;
            originalCameraParent = playerCamera.transform.parent;
        }

        // Create initial queue
        for (int i = 0; i < queueSize; i++)
        {
            character_mov.SpawnCharacterInQueue(i, spawnPoint, characterPrefab, characterQueue, spacing);
        }

        // Move first character to judgment point
        StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue,isProcessingDecision,judgmentPoint,godGuidanceRevealed,revealedGuiltStatus, this));
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
            godGuidanceRevealed = true;
            revealedGuiltStatus = Characters.currentCharacter.isGuilty;

            // Update crime text to show guilt status
            if (ui.crimeText != null)
            {
                ui.crimeText.text = $"Crime: {Characters.currentCharacter.crime} [{(revealedGuiltStatus ? "GUILTY" : "INNOCENT")}]";
            }

            // Don't proceed with normal decision flow yet - let player make another decision
            Decisions.ExecuteDecision(Characters.currentCharacter, decision);
            isProcessingDecision = false;
            return;
        }

        Decisions.ExecuteDecision(Characters.currentCharacter, decision);

        if (ui.decisionPanel != null)
            ui.decisionPanel.SetActive(false);

        bool isGoodOutcome = IsGoodOutcome(decision, Characters.currentCharacter.isGuilty);
        StartCoroutine(ProcessDecisionOutcome(isGoodOutcome, decision));
    }

    private bool IsGoodOutcome(Decisions.DecisionType decision, bool isGuilty)
    {
        switch (decision)
        {
            case Decisions.DecisionType.Forgive:
            case Decisions.DecisionType.RedemptionQuest:
            case Decisions.DecisionType.SpareWithWarning:
                return !isGuilty; // Good outcome for innocent people

            case Decisions.DecisionType.Execute:
            case Decisions.DecisionType.Exile:
            case Decisions.DecisionType.Torture:
            case Decisions.DecisionType.SacrificeToGod:
            case Decisions.DecisionType.Corruption:
                return isGuilty; // Good outcome for guilty people

            default:
                return random.Next(2) == 0;
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
        yield return StartCoroutine(character_mov.MoveCharacterToOutcome(characterOutcomePoint.position, judgmentPoint));
        yield return StartCoroutine(camera_mov.ReturnCameraToPlayer(playerCamera,originalCameraPosition,originalCameraRotation));

        if (Characters.currentCharacterInstance != null)
        {
            Destroy(Characters.currentCharacterInstance);
            Characters.currentCharacterInstance = null;
        }

        character_mov.SpawnCharacterInQueue(queueSize - 1, spawnPoint, characterPrefab, characterQueue, spacing);
        yield return StartCoroutine(character_mov.MoveQueueForward(characterQueue, spawnPoint, spacing));

        isProcessingDecision = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(character_mov.MoveToJudgmentPosition(characterQueue, isProcessingDecision, judgmentPoint, godGuidanceRevealed, revealedGuiltStatus, this));
    }

    public void ShowDecision()
    {
        ui.ShowDecision();
    }


}