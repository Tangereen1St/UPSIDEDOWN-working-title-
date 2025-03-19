using SGoap;
using UnityEngine;

// Every SGoap Action inherits from either BasicAction or Action
public class ShootAction : BasicAction
{
    // This action has a cool down of 1 second every use.
    public override float CooldownTime => 1;

    // Override Perform to execute the action.
    public override EActionStatus Perform()
    {
        Debug.Log("Shot");
        return EActionStatus.Success;
    }
}