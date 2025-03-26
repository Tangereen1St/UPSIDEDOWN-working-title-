using SGoap;
using UnityEngine;

public class GoToAction : BasicAction
{
    public Transform Target;

    public float Range = 4;
    public float MoveSpeed = 2;

    public override EActionStatus Perform()
    {
        AgentData.Target = Target;

        var distanceToTarget = Vector3.Distance(Target.position, transform.position);
        if (distanceToTarget <= Range)
            return EActionStatus.Success;

        var directionToTarget = (Target.position - transform.position).normalized;
        AgentData.Position += directionToTarget * Time.deltaTime * MoveSpeed;

        // Returning Running will keep this action going until we return Success.
        return EActionStatus.Running;
    }
}