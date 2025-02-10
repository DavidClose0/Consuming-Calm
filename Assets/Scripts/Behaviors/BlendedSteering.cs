using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendedSteering : SteeringBehavior
{
    public class BehaviorAndWeight
    {
        public SteeringBehavior behavior;
        public float weight;
    }

    public BehaviorAndWeight[] behaviors;

    public float maxAcceleration = 100f;
    public float maxRotation = 45f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        // Accumulate all accelerations.
        foreach (BehaviorAndWeight b in behaviors)
        { 
            // Check for null behavior
            if (b.behavior != null)
            {
                SteeringOutput currentSteering = b.behavior.getSteering();
                // Check for null steering output from behavior
                if (currentSteering != null)
                {
                    result.linear += b.weight * currentSteering.linear;
                    result.angular += b.weight * currentSteering.angular;
                }
            }
        }

        // Crop the result and return.
        if (result.linear.magnitude > maxAcceleration)
        {
            result.linear = result.linear.normalized * maxAcceleration;
        }
        if (Mathf.Abs(result.angular) > maxRotation)
        {
            result.angular = Mathf.Clamp(result.angular, -maxRotation, maxRotation);
        }

        return result;
    }
}