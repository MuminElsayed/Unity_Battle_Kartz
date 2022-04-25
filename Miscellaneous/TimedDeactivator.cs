using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDeactivator : MonoBehaviour
{
    [SerializeField]
    private float time;

    void Awake()
    {
        Invoke("Deactivate", time);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
