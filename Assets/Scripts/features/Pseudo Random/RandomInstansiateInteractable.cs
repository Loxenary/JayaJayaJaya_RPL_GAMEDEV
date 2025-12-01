using System.Collections.Generic;
using UnityEngine;

public class RandomInstansiateInteractable : MonoBehaviour
{
    [SerializeField] Interactable interactablePrefab;

    [SerializeField] int objectCount = 1;

    [SerializeField] List<Transform> posInstansiate;


    private void Awake()
    {

        for (int i = 0; i < objectCount; i++) {

            int rand = Random.Range(0, posInstansiate.Count - 1);

            Instantiate(interactablePrefab, posInstansiate[rand].position, Quaternion.identity, posInstansiate[rand]);
            posInstansiate.RemoveAt(rand);
        }
    }

}
