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
        }
    }

    private void FindAndFleeToNearestHatch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, hatchLayer);
        
        Transform closestHatch = null;
        Transform absoluteClosestHatchFallback = null; 
        float closestDistance = float.MaxValue;
        float fallbackDistance = float.MaxValue;

        Vector3 playerPos = GetPlayerPosition();
        Vector3 pushDirection = (transform.position - playerPos).normalized;

        foreach (var hit in hits)
        {
            if (!hit.gameObject.activeSelf) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);

            // Track the absolute closest hatch as a fallback option
            if (dist < fallbackDistance)
            {
                fallbackDistance = dist;
                absoluteClosestHatchFallback = hit.transform;
            }

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(pushDirection, dirToTarget);

            // --- 270 DEGREE ARC FILTER ---
            if (dot < -0.707f) continue; 

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestHatch = hit.transform;
            }
        }

        // REMOVED LINECAST CHECK: Relying on OverlapSphere and safety arc check
        if (closestHatch == null && absoluteClosestHatchFallback != null)
        {
            Debug.Log("[AlienAI] Panic Mode: No path in safety arc. Fleeing to absolute closest hatch.");
            closestHatch = absoluteClosestHatchFallback;
        }

        if (closestHatch != null)
        {
            currentTarget = closestHatch;
            currentState = AIState.FleeingToTarget;
        }
        else
        {
            Debug.LogWarning("CHECKMATE: Alien completely trapped! No hatches available anywhere in the search bubble.");
        }
    }

    private void FindAndFleeToNearestNavPoint()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, navPointLayer);

        Transform closestNavPoint = null;
        Transform absoluteClosestNavFallback = null; 
        float closestDistance = float.MaxValue;
        float fallbackDistance = float.MaxValue;

        Vector3 playerPos = GetPlayerPosition();
        Vector3 pushDirection = (transform.position - playerPos).normalized;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            // Track absolute closest navpoint as a fallback option
            if (dist < fallbackDistance)
            {
                fallbackDistance = dist;
                absoluteClosestNavFallback = hit.transform;
            }

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(pushDirection, dirToTarget);

            // --- 270 DEGREE ARC FILTER ---
            if (dot < -0.707f) continue; 

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestNavPoint = hit.transform;
            }
        }

        // REMOVED LINECAST CHECK: Relying on OverlapSphere and safety arc check
        if (closestNavPoint == null && absoluteClosestNavFallback != null)
        {
            Debug.Log("[AlienAI] Panic Mode: No navpoint in safety arc. Heading to absolute closest navpoint.");
            closestNavPoint = absoluteClosestNavFallback;
        }

        if (closestNavPoint != null)
        {
            currentTarget = closestNavPoint;
            currentState = AIState.FleeingToTarget; 
        }
        else
        {
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

    private void MoveTowardsTarget()
    {
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