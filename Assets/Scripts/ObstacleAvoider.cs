using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoider : Kinematic
{
    ObstacleAvoidance myMoveType;
    LookWhereGoing myRotateType;

    public float avoidDistance;
    public float lookAhead;

    public float whiskerAngle = 30f;
    public float whiskerLength = 10f;

    public LayerMask obstacleLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new ObstacleAvoidance();
        myMoveType.character = this;
        myMoveType.avoidDistance = avoidDistance;
        myMoveType.lookAhead = lookAhead;
        myMoveType.whiskerAngle = whiskerAngle;
        myMoveType.whiskerLength = whiskerLength;
        myMoveType.obstacleLayerMask = obstacleLayerMask;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.linear = myMoveType.getSteering().linear;
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }
}
