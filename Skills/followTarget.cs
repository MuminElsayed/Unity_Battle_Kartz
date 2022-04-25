using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followTarget : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        transform.position = target.transform.position;
    }
}
