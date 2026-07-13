using UnityEngine;

public class AlienAI : MonoBehaviour
{
    public enum AIState { WaitingAtNavPoint, FleeingToTarget, TransitioningAtHatch, Cornered }
    
    [Header("Current Status")]
    [SerializeField] private AIState currentState = AIState.WaitingAtNavPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arrivalDistance = 0.4f;

    [Header("Detection Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask hatchLayer;
    [SerializeField] private LayerMask navPointLayer;
    
    [Header("Obstacle Layers")]
    [Tooltip("Select your Wall and Interactable (Box) layers here!")]
    [SerializeField] private LayerMask obstacleLayers; 

    [Header("Tweakable Radii")]
    [SerializeField] private float playerDetectRadius = 5f;
    [SerializeField] private float searchRadius = 40f;

    private Transform currentTarget;
    private Collider[] playerBuffer = new Collider[1];
    
    // Tracks if the alien is breaking free from a previous trap state
    private bool wasOnceCornered = false;

    private void Update()
    {
        switch (currentState)
        {
            case AIState.WaitingAtNavPoint:
                int playerFound = Physics.OverlapSphereNonAlloc(transform.position, playerDetectRadius, playerBuffer, playerLayer);
                if (playerFound > 0)
                {
                    FindAndFleeToNearestHatch();
                }
                break;

            case AIState.FleeingToTarget:
                MoveTowardsTarget();
                break;

            case AIState.TransitioningAtHatch:
                FindAndFleeToNearestNavPoint();
                break;

            case AIState.Cornered:
                int playerApproaching = Physics.OverlapSphereNonAlloc(transform.position, playerDetectRadius, playerBuffer, playerLayer);
                if (playerApproaching > 0)
                {
                    FindAndFleeToNearestHatch();
                }
                break;
        }
    }

    private void FindAndFleeToNearestHatch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, hatchLayer);
        
        Transform closestHatch = null;
        float closestDistance = float.MaxValue;

        Vector3 playerPos = GetPlayerPosition();
        Vector3 pushDirection = (transform.position - playerPos).normalized;

        foreach (var hit in hits)
        {
            if (!hit.gameObject.activeSelf) continue;
            if (IsPathBlocked(hit.transform.position)) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(pushDirection, dirToTarget);

            if (dot < 0.0f) continue; 

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestHatch = hit.transform;
            }
        }

        if (closestHatch != null)
        {
            currentTarget = closestHatch;
            currentState = AIState.FleeingToTarget;
        }
        else
        {
            if (currentState != AIState.Cornered)
            {
                TriggerCorneredState();
            }
        }
    }

    private void FindAndFleeToNearestNavPoint()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, navPointLayer);

        Transform closestNavPoint = null;
        float closestDistance = float.MaxValue;

        Vector3 playerPos = GetPlayerPosition();
        Vector3 pushDirection = (transform.position - playerPos).normalized;

        foreach (var hit in hits)
        {
            if (IsPathBlocked(hit.transform.position)) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(pushDirection, dirToTarget);

            if (dot < 0.0f) continue; 

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestNavPoint = hit.transform;
            }
        }

        if (closestNavPoint != null)
        {
            currentTarget = closestNavPoint;
            currentState = AIState.FleeingToTarget; 
        }
        else
        {
            TriggerCorneredState();
        }
    }

    private bool IsPathBlocked(Vector3 targetPosition)
    {
        Vector3 startPos = transform.position + Vector3.up * 0.1f; 
        Vector3 endPos = targetPosition + Vector3.up * 0.1f; 
        return Physics.Linecast(startPos, endPos, obstacleLayers);
    }

    private void TriggerCorneredState()
    {
        currentState = AIState.Cornered;
        wasOnceCornered = true; // Flips our escape checker toggle
        Debug.Log("VICTORY: The alien has been successfully cornered!");
        
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.DisplayNotification("You trapped the alien!");
        }
    }

    private Vector3 GetPlayerPosition()
    {
        Collider[] playerCheck = new Collider[1];
        if (Physics.OverlapSphereNonAlloc(transform.position, 100f, playerCheck, playerLayer) > 0)
        {
            return playerCheck[0].transform.position;
        }
        return transform.position - transform.forward * 10f; 
    }

    private void MoveTowardsTarget()
    {
        // Custom notification handling based on whether it broke free or is just walking normal
        if (GameUIManager.Instance != null)
        {
            if (wasOnceCornered)
            {
                GameUIManager.Instance.DisplayNotification("Alien escaped again!");
                wasOnceCornered = false; // Reset toggle so it can be cleared or retrapped cleanly
            }
            else
            {
                GameUIManager.Instance.DisplayNotification(""); 
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        }

        if (Vector3.Distance(transform.position, currentTarget.position) < arrivalDistance)
        {
            if (((1 << currentTarget.gameObject.layer) & navPointLayer) != 0)
            {
                currentState = AIState.WaitingAtNavPoint;
            }
            else 
            {
                currentState = AIState.TransitioningAtHatch;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);
    }
}