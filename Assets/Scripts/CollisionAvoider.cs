using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoider : Kinematic
{
    CollisionAvoidance myMoveType;
    LookWhereGoing myRotateType;

    public Kinematic[] myTargets;


    // Start is called before the first frame update
    void Start()
    {
        myMoveType = new CollisionAvoidance();
        myMoveType.character = this;
        myMoveType.targets = myTargets;

        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        SteeringOutput moveSteeringUpdate = myMoveType.getSteering();
        if (moveSteeringUpdate != null)
        {
            steeringUpdate.linear = moveSteeringUpdate.linear;
        }
        else
        {
            steeringUpdate.linear = Vector3.zero;
        }
        steeringUpdate.angular = myRotateType.getSteering().angular;
        base.Update();
    }
}
