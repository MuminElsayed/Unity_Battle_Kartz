using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolObject
{
    public GameObject object2Pool;
    public Sprite objectSprite;
    public int objectID;
    public int amount2Pool = 3;
    public List<GameObject> pooledObjects;
}

public class Skills : MonoBehaviour 
{
	public static Skills instance;
    [SerializeField]
    public PoolObject[] allSkills;

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
        foreach (PoolObject skill in allSkills)
        {
            skill.objectID = counter; //skill ID is the same as its array index
            for (int i = 0; i < skill.amount2Pool; i++)
            {
                if (skill.object2Pool != null) //Has a prefab (not a self skill)
                {
                    GameObject obj = Instantiate(skill.object2Pool, Vector3.zero, Quaternion.identity, transform);
                    obj.SetActive(false);
                    skill.pooledObjects.Add(obj);
                }
            }
            counter ++;
        }
    }

    public void getSkill(int skillID, Vector3 TargetPosition, Vector3 forwardRot, Transform playerTransform)
    {
        if (allSkills[skillID].object2Pool != null)
        {
            foreach (GameObject pooledSkill in allSkills[skillID].pooledObjects)
            {
                if (!pooledSkill.activeInHierarchy)
                {
                    pooledSkill.transform.SetParent(playerTransform);
                    pooledSkill.transform.position = TargetPosition;
                    // pooledSkill.GetComponentInChildren<Rigidbody>(). position = TargetPosition;
                    pooledSkill.transform.rotation = Quaternion.Euler(forwardRot);
                    pooledSkill.SetActive(true);
                    return;
                }
            }
            //All objects enabled
            GameObject obj = Instantiate(allSkills[skillID].pooledObjects[0], TargetPosition, Quaternion.Euler(forwardRot), playerTransform);
            obj.SetActive(false); //Reactivates since some scripts use OnEnable/OnDisable
            obj.SetActive(true);
            allSkills[skillID].pooledObjects.Add(obj);
            Debug.Log("all " + allSkills[skillID].object2Pool + "active");
        }
    }
}
