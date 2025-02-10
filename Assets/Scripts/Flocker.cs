using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocker : Kinematic
{
    PrioritySteering myMoveType;
    LookWhereGoing myRotateType;
    Arrive cohesion;
    Separation separation;
    VelocityMatch velocityMatch;
    ObstacleAvoidance obstacleAvoidance;
    public Kinematic[] targets;

    public float cohesionWeight = 0.1f;
    public float separationWeight = 0.6f;
    public float velocityMatchWeight = 0.3f;
    public float obstacleAvoidanceWeight = 1.0f;

    public float avoidDistance = 5f;
    public float lookAhead = 5f;
    public float whiskerAngle = 30f;
    public float whiskerLength = 3f;
    public LayerMask obstacleLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize flocking behaviors
        cohesion = new Arrive();
        cohesion.character = this;
        cohesion.target = new GameObject("CohesionTarget"); // Dummy target, will be updated

        separation = new Separation();
        separation.character = this;
        separation.targets = targets;

        velocityMatch = new VelocityMatch();
        velocityMatch.character = this;
        velocityMatch.targets = targets;

        BlendedSteering flockingBlendedSteering = new BlendedSteering(); // BlendedSteering for flocking
        flockingBlendedSteering.behaviors = new BlendedSteering.BehaviorAndWeight[]
        {
            new BlendedSteering.BehaviorAndWeight() { behavior = cohesion, weight = cohesionWeight },
            new BlendedSteering.BehaviorAndWeight() { behavior = separation, weight = separationWeight },
            new BlendedSteering.BehaviorAndWeight() { behavior = velocityMatch, weight = velocityMatchWeight }
        };

        // Initialize Obstacle Avoidance
        obstacleAvoidance = new ObstacleAvoidance();
        obstacleAvoidance.character = this;
        obstacleAvoidance.avoidDistance = avoidDistance;
        obstacleAvoidance.lookAhead = lookAhead;
        obstacleAvoidance.whiskerAngle = whiskerAngle;
        obstacleAvoidance.whiskerLength = whiskerLength;
        obstacleAvoidance.obstacleLayerMask = obstacleLayerMask;
        obstacleAvoidance.targetsToIgnore = targets;


        myMoveType = new PrioritySteering(); // Initialize PrioritySteering
        myMoveType.behaviors = new SteeringBehavior[] // Array of SteeringBehaviors for priority
        {
            obstacleAvoidance, // Obstacle avoidance has priority
            flockingBlendedSteering // Flocking behaviors are lower priority
        };

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Calculate cohesion target (center of mass)
        Vector3 centerOfMass = Vector3.zero;
        int validTargetCount = 0; // Counter for valid targets

        if (targets != null && targets.Length > 0) // Check if targets array is not null and not empty
        {
            foreach (Kinematic target in targets)
            {
                if (target != null) // Check if target is not null (not destroyed)
                {
                    centerOfMass += target.transform.position;
                    validTargetCount++;
                }
            }
            if (validTargetCount > 0) // Only divide if there are valid targets
            {
                centerOfMass /= validTargetCount;
            }
        }
        cohesion.target.transform.position = centerOfMass; // Update cohesion target position

        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear; // Get steering from PrioritySteering
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }
}