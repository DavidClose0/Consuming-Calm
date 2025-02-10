using UnityEngine;

public class PrioritySteering
{
    public SteeringBehavior[] behaviors;

    public SteeringOutput getSteering()
    {
        float epsilon = 0.01f; // Small value, effectively zero

        SteeringOutput accumulatedSteering = new SteeringOutput(); // Initialize to zero

        foreach (SteeringBehavior behavior in behaviors)
        {
            SteeringOutput steering = behavior.getSteering();

            // Check if we’re above the threshold, if so return.
            if (steering.linear.magnitude > epsilon || Mathf.Abs(steering.angular) > epsilon)
            {
                return steering;
            }
            accumulatedSteering = steering; // Accumulate in case no higher priority steering was active, last behavior will be returned if none are above epsilon
        }

        // If we get here, it means that no behavior had a large enough
        // acceleration, so return the small acceleration from the
        // final behavior (which will be zero if no behavior produced significant steering).
        return accumulatedSteering;
    }
}