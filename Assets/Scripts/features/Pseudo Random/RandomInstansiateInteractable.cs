using System.Collections.Generic;
using UnityEngine;

public class RandomInstansiateInteractable : MonoBehaviour
{
    [SerializeField] Interactable interactablePrefab;

    [SerializeField] protected int objectCount = 1;

    [SerializeField] protected List<Transform> posInstansiate;

    [SerializeField] protected bool spawnOnAwake;

    protected virtual void Awake()
    {
        if(spawnOnAwake)
            Spawn();
        
    }
    public virtual void Spawn()
    {
        for (int i = 0; i < objectCount; i++)
        {

            int rand = Random.Range(0, posInstansiate.Count - 1);

            Instantiate(interactablePrefab, posInstansiate[rand].position, Quaternion.identity, posInstansiate[rand]);
            posInstansiate.RemoveAt(rand);
        }
    }

}
