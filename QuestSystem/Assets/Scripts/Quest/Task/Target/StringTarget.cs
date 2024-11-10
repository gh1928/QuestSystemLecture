using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/String", fileName = "Target_")]
public class StringTarget : TaskTarget
{
    [SerializeField] private string value;
    public override object Value => value;

    public override bool IsEqual(object target)
    {
        if(target is not string targetAsString)
        {
            return false;
        }

        return value == targetAsString;
    }
}
