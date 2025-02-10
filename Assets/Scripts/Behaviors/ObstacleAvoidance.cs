using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : SteeringBehavior
{
    public float avoidDistance = 10f;
    public float lookAhead = 10f;
    public float sideAvoidanceMultiplier = 1f;
    public float forwardAvoidanceMultiplier = 1f;

    public float whiskerAngle = 30f;
    public float whiskerLength = 10f;

    public LayerMask obstacleLayerMask;

    private Vector3 targetPosition;

    public Kinematic character;
    public GameObject target;
    float maxAcceleration = 100f;

    public Kinematic[] targetsToIgnore; // Array of targets to ignore during obstacle avoidance

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        RaycastHit forwardHit, leftHit, rightHit;
        bool forwardObstacle = false;
        bool leftObstacle = false;
        bool rightObstacle = false;
        RaycastHit closestHit = new RaycastHit();

        // Forward Raycast
        if (Physics.Raycast(character.transform.position, character.transform.forward, out forwardHit, lookAhead, obstacleLayerMask))
        {
            if (!IsTargetToIgnore(forwardHit.collider.gameObject)) // Check if hit object is in targetsToIgnore
            {
                // Debug.DrawRay(character.transform.position, character.transform.forward * forwardHit.distance, Color.red, 0.5f);
                forwardObstacle = true;
                closestHit = forwardHit;
            }
            else
            {
                // Debug.DrawRay(character.transform.position, character.transform.forward * forwardHit.distance, Color.yellow, 0.5f); // Visual cue for ignored fish
            }
        }
        else
        {
            // Debug.DrawRay(character.transform.position, character.transform.forward * lookAhead, Color.green, 0.5f);
        }

        // Left Whisker Raycast
        Quaternion leftRotation = Quaternion.AngleAxis(-whiskerAngle, character.transform.up); // Negative angle for left (counter-clockwise)
        Vector3 leftWhiskerDirection = leftRotation * character.transform.forward;
        if (Physics.Raycast(character.transform.position, leftWhiskerDirection, out leftHit, whiskerLength, obstacleLayerMask))
        {
            if (!IsTargetToIgnore(leftHit.collider.gameObject)) // Check if hit object is in targetsToIgnore
            {
                // Debug.DrawRay(character.transform.position, leftWhiskerDirection * leftHit.distance, Color.red, 0.5f);
                leftObstacle = true;
                if (!closestHit.collider || leftHit.distance < closestHit.distance)
                {
                    closestHit = leftHit;
                }
            }
            else
            {
                // Debug.DrawRay(character.transform.position, leftWhiskerDirection * leftHit.distance, Color.yellow, 0.5f); // Visual cue for ignored fish
            }
        }
        else
        {
            // Debug.DrawRay(character.transform.position, leftWhiskerDirection * whiskerLength, Color.green, 0.5f);
        }

        // Right Whisker Raycast
        Quaternion rightRotation = Quaternion.AngleAxis(whiskerAngle, character.transform.up); // Positive angle for right (clockwise)
        Vector3 rightWhiskerDirection = rightRotation * character.transform.forward;
        if (Physics.Raycast(character.transform.position, rightWhiskerDirection, out rightHit, whiskerLength, obstacleLayerMask))
        {
            if (!IsTargetToIgnore(rightHit.collider.gameObject)) // Check if hit object is in targetsToIgnore
            {
                // Debug.DrawRay(character.transform.position, rightWhiskerDirection * rightHit.distance, Color.red, 0.5f);
                rightObstacle = true;
                if (!closestHit.collider || rightHit.distance < closestHit.distance)
                {
                    closestHit = rightHit;
                }
            }
            else
            {
                // Debug.DrawRay(character.transform.position, rightWhiskerDirection * rightHit.distance, Color.yellow, 0.5f); // Visual cue for ignored fish
            }
        }
        else
        {
            // Debug.DrawRay(character.transform.position, rightWhiskerDirection * whiskerLength, Color.green, 0.5f);
        }

        if (forwardObstacle || leftObstacle || rightObstacle) // Obstacle detected by forward or whiskers (and not ignored)
        {
            Vector3 avoidanceDirection = Vector3.zero;

            if (!leftObstacle)
            {
                // Avoid left (counter-clockwise) - use left whisker direction as avoidance direction
                avoidanceDirection = leftWhiskerDirection.normalized;
            }
            else if (!rightObstacle)
            {
                // Avoid right (clockwise) - use right whisker direction as avoidance direction
                avoidanceDirection = rightWhiskerDirection.normalized;
            }
            else
            {
                // Both left and right are blocked, or only forward is blocked and both whiskers also blocked.
                // Default to avoiding right (clockwise) as before, or use normal of closest hit
                Vector3 sideAvoidanceDirection = Vector3.Cross(closestHit.normal, Vector3.up).normalized;
                avoidanceDirection = sideAvoidanceDirection; // Default to right side avoidance if both whiskers are blocked.
            }

            targetPosition = character.transform.position + (avoidanceDirection * avoidDistance * sideAvoidanceMultiplier) + (-character.transform.forward * avoidDistance * forwardAvoidanceMultiplier);

            Vector3 direction = targetPosition - character.transform.position;
            direction.Normalize();
            result.linear = direction * maxAcceleration;
            result.angular = 0f;
            return result;
        }
        else
        {
            // No obstacle detected (or all detected obstacles are ignored targets), no avoidance steering
            result.linear = Vector3.zero;
            result.angular = 0f;
            return result;
        }
    }


    private bool IsTargetToIgnore(GameObject hitObject)
    {
        if (targetsToIgnore == null) return false; // No targets to ignore

        foreach (Kinematic target in targetsToIgnore)
        {
            if (target != null && target.gameObject == hitObject)
            {
                return true; // Hit object is in the list of targets to ignore
            }
        }
        return false; // Hit object is not in the list of targets to ignore
    }
}