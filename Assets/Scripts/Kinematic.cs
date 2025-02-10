using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematic : MonoBehaviour
{
    public Vector3 linearVelocity;
    public float angularVelocity;
    public float maxSpeed = 10.0f;
    public float maxAngularVelocity = 45.0f; // degrees

    public GameObject myTarget;
    protected SteeringOutput steeringUpdate;

    // OverlapBox Configuration
    public Vector3 overlapBoxHalfExtents = Vector3.one * 0.5f;
    public LayerMask collisionLayerMask = Physics.DefaultRaycastLayers;
    public LayerMask ignoreCollisionLayerMask = 0; // New: LayerMask to ignore for collisions. Default to None.

    // Collision Resolution Parameters
    public float collisionResolutionForce = 10f;
    public float playerScaleIncrease = 0.1f; // Public variable for scale increase, adjustable in inspector

    void Start()
    {
        steeringUpdate = new SteeringOutput();
    }

    protected virtual void Update()
    {
        if (float.IsNaN(angularVelocity))
        {
            angularVelocity = 0.0f;
        }

        Vector3 totalResolutionVector = Vector3.zero;

        // Scale the overlapBoxHalfExtents by the lossyScale to account for object scaling
        Vector3 scaledOverlapBoxHalfExtents = Vector3.Scale(overlapBoxHalfExtents, transform.lossyScale);
        Collider[] allColliders = Physics.OverlapBox(transform.position, scaledOverlapBoxHalfExtents, transform.rotation, collisionLayerMask, QueryTriggerInteraction.UseGlobal);
        List<Collider> externalColliders = new List<Collider>(); // List to store colliders that are NOT on this GameObject

        // Filter out colliders attached to this GameObject and colliders on the ignoreCollisionLayerMask
        foreach (Collider collider in allColliders)
        {
            if (collider.gameObject != this.gameObject && collider.tag != "End" && !IsInLayerMask(collider.gameObject.layer, ignoreCollisionLayerMask))
            {
                externalColliders.Add(collider); // Add to the list of external colliders
            }
        }

        if (externalColliders.Count > 0) // Use the filtered list for collision resolution
        {
            foreach (Collider otherCollider in externalColliders)
            {
                if (this.gameObject.tag == "Player" && otherCollider.gameObject.tag == "Bird")
                {
                    Destroy(otherCollider.gameObject);
                    Vector3 currentScale = transform.localScale;
                    transform.localScale = new Vector3(currentScale.x + playerScaleIncrease, currentScale.y + playerScaleIncrease, currentScale.z + playerScaleIncrease);
                    continue; // Skip collision resolution for this specific collider after handling Player-Bird collision
                }

                Vector3 closestPoint = otherCollider.ClosestPoint(transform.position);
                Vector3 overlapDirection = transform.position - closestPoint;
                float overlapMagnitude = overlapDirection.magnitude;

                // Use scaledOverlapBoxHalfExtents.magnitude for distance check
                if (overlapMagnitude < scaledOverlapBoxHalfExtents.magnitude * 2f)
                {
                    if (overlapMagnitude > 0)
                    {
                        Vector3 resolutionVector = overlapDirection.normalized * collisionResolutionForce * Time.deltaTime;
                        totalResolutionVector += resolutionVector;
                    }
                    else
                    {
                        totalResolutionVector += Vector3.up * collisionResolutionForce * Time.deltaTime;
                    }
                }
            }
        }

        totalResolutionVector.y = 0;
        transform.position += totalResolutionVector;

        linearVelocity.y = 0;
        this.transform.position += linearVelocity * Time.deltaTime;
        if (Mathf.Abs(angularVelocity) > 0.01f)
        {
            Vector3 v = new Vector3(0, angularVelocity, 0);
            this.transform.eulerAngles += v * Time.deltaTime;
        }


        if (steeringUpdate != null)
        {
            linearVelocity += steeringUpdate.linear * Time.deltaTime;
            angularVelocity += steeringUpdate.angular * Time.deltaTime;
        }

        if (linearVelocity.magnitude > maxSpeed)
        {
            linearVelocity.Normalize();
            linearVelocity *= maxSpeed;
        }
        if (Mathf.Abs(angularVelocity) > maxAngularVelocity)
        {
            angularVelocity = maxAngularVelocity * (angularVelocity / Mathf.Abs(angularVelocity));
        }
    }

    // Helper function to check if a layer is in a LayerMask
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}