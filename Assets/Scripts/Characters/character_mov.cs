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

    void TriggerOtherAction()
    {
        if (makedecision != null)
        {
            makedecision.ShowDecision();
        }
    }

    // This will be called by Makedecision after a choice is made
    public void OnDecisionMade()
    {
        decisionMade = true;

        // Start the respawn process
        StartCoroutine(RespawnCharacter());
    }

    private System.Collections.IEnumerator RespawnCharacter()
    {
        // Wait a moment before destroying (optional)
        yield return new WaitForSeconds(1f);

        // Destroy this character
        Destroy(gameObject);

        // The Makedecision script will handle creating a new one
    }
}