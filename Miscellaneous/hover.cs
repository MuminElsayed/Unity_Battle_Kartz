using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hover : MonoBehaviour
{
    [SerializeField] float amplitude = 5f;
    private Vector3 tempPos = new Vector3(0, 0, 0);

    void Update () 
    {
        FloatAnimation();
	}

    void FloatAnimation()
    {
        tempPos = new Vector3 (0, 0, 0);
        tempPos.y += Mathf.Sin(Mathf.PI * Time.fixedTime) * amplitude;
        transform.Translate(tempPos * Time.deltaTime);
    }
}
