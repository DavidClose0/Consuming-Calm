using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidance : SteeringBehavior
{
    public Kinematic character;
    public Kinematic[] targets;

    float maxAcceleration = 1f;
    float radius = .1f;

    public override SteeringOutput getSteering()
    {
        float shortestTime = float.PositiveInfinity;
        Kinematic firstTarget = null;
        float firstMinSeparation;
        float firstDistance;
        Vector3 firstRelativePos;
        Vector3 firstRelativeVel;

        foreach (Kinematic target in targets)
        {
            Vector3 relativePos = target.transform.position - character.transform.position;
            Vector3 relativeVel = character.linearVelocity - target.linearVelocity;
            float relativeSpeed = relativeVel.magnitude;
            float timeToCollision = Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

            float distance = relativePos.magnitude;
            float minSeparation = distance - relativeSpeed * timeToCollision;
            if (minSeparation > 2 * radius)
            {
                continue;
            }

            if (timeToCollision > 0 && timeToCollision < shortestTime)
            {
                shortestTime = timeToCollision;
                firstTarget = target;
                firstMinSeparation = minSeparation;
                firstDistance = distance;
                firstRelativePos = relativePos;
                firstRelativeVel = relativeVel;
            }
        }

        if (firstTarget == null)
        {
            return null;
        }

        SteeringOutput result = new SteeringOutput();

        float dotResult = Vector3.Dot(character.linearVelocity.normalized, firstTarget.linearVelocity.normalized);
        if (dotResult < -.9)
        {
            result.linear = -firstTarget.transform.right;
        }
        else
        {
            result.linear = -firstTarget.linearVelocity;
        }
        result.linear.Normalize();
        result.linear *= maxAcceleration;
        result.angular = 0;
        return result;
    }
}
