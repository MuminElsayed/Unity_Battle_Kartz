using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EffectsPooler : MonoBehaviour 
{
	public static EffectsPooler instance;
    [SerializeField]
    private PoolObject[] allEffects;

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        //Start pooling
        int counter = 0;
        foreach (PoolObject effect in allEffects)
        {
            effect.objectID = counter; //Effect ID is the same as its array index
            for (int i = 0; i < effect.amount2Pool; i++)
            {
                GameObject obj = Instantiate(effect.object2Pool, Vector3.zero, Quaternion.identity, transform);
                obj.SetActive(false);
                effect.pooledObjects.Add(obj);
            }
            counter ++;
        }
    }

    public void getEffect(int effectID, Vector3 TargetPosition, Transform playerTransform)
    {
        foreach (GameObject pooledEffect in allEffects[effectID].pooledObjects)
        {
            if (!pooledEffect.activeInHierarchy)
            {
                pooledEffect.transform.SetParent(playerTransform);
                pooledEffect.transform.position = TargetPosition;
                pooledEffect.SetActive(true);
                return;
            }
        }
        GameObject obj = Instantiate(allEffects[effectID].pooledObjects[0], TargetPosition, Quaternion.identity, playerTransform);
        allEffects[effectID].pooledObjects.Add(obj);
        print("all " + allEffects[effectID] + "active");
    }
}
