using UnityEngine;

public class AlienAI : MonoBehaviour
{
    public enum AIState { WaitingAtNavPoint, FleeingToTarget, TransitioningAtHatch }
    
    [Header("Current Status")]
    [SerializeField] private AIState currentState = AIState.WaitingAtNavPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arrivalDistance = 0.4f;

    [Header("Detection Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask hatchLayer;
    [SerializeField] private LayerMask navPointLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Tweakable Radii")]
    [SerializeField] private float playerDetectRadius = 5f;
    [SerializeField] private float searchRadius = 40f;

    private Transform currentTarget;
    private Collider[] playerBuffer = new Collider[1];

    private void Update()
    {
        switch (currentState)
        {
            case AIState.WaitingAtNavPoint:
                // Stand completely still until the player gets within range
                int playerFound = Physics.OverlapSphereNonAlloc(transform.position, playerDetectRadius, playerBuffer, playerLayer);
                if (playerFound > 0)
                {
                    FindAndFleeToNearestHatch();
                }
                break;

            case AIState.FleeingToTarget:
                // Actively track and move towards the chosen target point
                MoveTowardsTarget();
                break;

            case AIState.TransitioningAtHatch:
                // Ran into a hatch, instantly calculate the next valid hallway/room breadcrumb
                FindAndFleeToNearestNavPoint();
                break;
        }
    }

    private void FindAndFleeToNearestHatch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, hatchLayer);
        
        Transform closestHatch = null;
        float closestDistance = float.MaxValue;

        Vector3 playerPos = GetPlayerPosition();
        // The direct line moving away from the player threat
        Vector3 pushDirection = (transform.position - playerPos).normalized;

        foreach (var hit in hits)
        {
            if (!hit.gameObject.activeSelf) continue;
            if (IsBlockedByWall(hit.transform.position)) continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(pushDirection, dirToTarget);

            // --- 270 DEGREE ARC FILTER ---
            // Disqualifies the choice if it forces the alien into a narrow 90-degree cone facing the player
            if (dot < -0.707f) continue; 

            float dist = Vector3.Distance(transform.position, hit.transform.position);
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
            Debug.LogWarning("CHECKMATE: Alien trapped! No valid hatches inside its 270-degree safety arc.");
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
            if (IsBlockedByWall(hit.transform.position)) continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(pushDirection, dirToTarget);

            // --- 270 DEGREE ARC FILTER ---
            if (dot < -0.707f) continue; 

            float dist = Vector3.Distance(transform.position, hit.transform.position);
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
            // If it somehow runs through a hatch but has no valid escape navpoint ahead, drop back to waiting
            currentState = AIState.WaitingAtNavPoint;
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

    private bool IsBlockedByWall(Vector3 targetPosition)
    {
        Vector3 startPos = transform.position + Vector3.up * 0.5f;
        Vector3 endPos = targetPosition + Vector3.up * 0.5f;
        return Physics.Linecast(startPos, endPos, wallLayer);
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        }

        // Check if we hit the targeted destination
        if (Vector3.Distance(transform.position, currentTarget.position) < arrivalDistance)
        {
            // If the reached object layer is our Navpoint layer, sit still and look for player again
            if (((1 << currentTarget.gameObject.layer) & navPointLayer) != 0)
            {
                currentState = AIState.WaitingAtNavPoint;
            }
            else // Otherwise it's a hatch structure, execute immediate link search
            {
                currentState = AIState.TransitioningAtHatch;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visual indicator in Scene view for the player detection bubble
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);
    }
}