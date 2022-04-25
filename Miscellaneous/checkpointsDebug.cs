using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class checkpointsDebug : MonoBehaviour
{
    void OnEnable()
    {
        //Renames checkpoints to their order in parent
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        int counter = 1;
        foreach (Transform checkpoint in children)
        {
            if (checkpoint.parent != null && counter > 1)
            {
                checkpoint.name = (counter - 1).ToString();
            } else {
                checkpoint.name = "Checkpoints"; //Parent
            }
            counter ++;
        }
    }
}
