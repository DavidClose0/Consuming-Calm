using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityMatch : SteeringBehavior
{
    public Kinematic character;
    public Kinematic[] targets;
    public float MaxAcceleration = 100f;

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        // Calculate average velocity
        Vector3 averageVelocity = Vector3.zero;
        if (targets != null)
        {
            foreach (Kinematic target in targets)
            {
                averageVelocity += target.linearVelocity;
            }
            averageVelocity /= targets.Length;
        }

        result.linear = averageVelocity;

        // give full acceleration along this direction
        result.linear.Normalize();
        result.linear *= MaxAcceleration;

        result.angular = 0;
        return result;
    }
}