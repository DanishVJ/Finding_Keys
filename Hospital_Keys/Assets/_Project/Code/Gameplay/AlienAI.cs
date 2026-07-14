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

    [Header("Audio Clips")]
    [Tooltip("Sound played the split second the alien spots the player and flees")]
    [SerializeField] private AudioClip detectionSound;

    private Transform currentTarget;
    private Collider[] playerBuffer = new Collider[1];
    private bool wasOnceCornered = false;

    private void Update()
    {
        switch (currentState)
        {
            case AIState.WaitingAtNavPoint:
                int playerFound = Physics.OverlapSphereNonAlloc(transform.position, playerDetectRadius, playerBuffer, playerLayer);
                if (playerFound > 0)
                {
                    PlaySound(detectionSound);
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
                // --- FAIL-SAFE: If a player pulls a box away while cornered, break free instantly ---
                if (CardboardBox.BlockedHatchCount < 2)
                {
                    currentState = AIState.WaitingAtNavPoint;
                    FindAndFleeToNearestHatch();
                    break;
                }

                int playerApproaching = Physics.OverlapSphereNonAlloc(transform.position, playerDetectRadius, playerBuffer, playerLayer);
                if (playerApproaching > 0)
                {
                    PlaySound(detectionSound);
                    FindAndFleeToNearestHatch();
                }
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    private void FindAndFleeToNearestHatch()
    {
        // --- FAIL-SAFE CHEAT: Only allowed to be cornered if BOTH boxes block the hatches ---
        if (CardboardBox.BlockedHatchCount >= 2)
        {
            TriggerCorneredState();
            return;
        }

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

            // Bypasses dot check constraints if it helps the alien find ANY open path to safety
            if (dot < 0.0f && hits.Length > 1) continue; 

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
            // Back-up search fallback: look for ANY active hatch regardless of player angle
            foreach (var hit in hits)
            {
                if (!hit.gameObject.activeSelf) continue;
                currentTarget = hit.transform;
                currentState = AIState.FleeingToTarget;
                return;
            }
        }
    }

    private void FindAndFleeToNearestNavPoint()
    {
        if (CardboardBox.BlockedHatchCount >= 2)
        {
            TriggerCorneredState();
            return;
        }

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
            // Fallback route: clear state blockages and flee blindly 
            currentState = AIState.WaitingAtNavPoint;
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
        wasOnceCornered = true; 
        Debug.Log("VICTORY: Both hatches sealed! The alien has been successfully cornered!");
        
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
        if (GameUIManager.Instance != null)
        {
            if (wasOnceCornered)
            {
                GameUIManager.Instance.DisplayNotification("Alien escaped again!");
                wasOnceCornered = false; 
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