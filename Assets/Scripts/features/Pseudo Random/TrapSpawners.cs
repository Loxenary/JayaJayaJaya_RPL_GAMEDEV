using UnityEngine;

public class TrapSpawners : RandomInstansiateInteractable
{
    [SerializeField] Damage trapPrefab;

    public override void Spawn()
    {
        int num = Random.Range(1, objectCount+1);
        for (int i = 0; i < num; i++)
        {

            int rand = Random.Range(0, posInstansiate.Count - 1);

            Instantiate(trapPrefab, posInstansiate[rand].position, Quaternion.identity, posInstansiate[rand]);
            posInstansiate.RemoveAt(rand);
        }
    }
}
