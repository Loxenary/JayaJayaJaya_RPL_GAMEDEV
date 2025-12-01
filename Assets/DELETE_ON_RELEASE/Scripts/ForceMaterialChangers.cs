using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ForceMaterialChangers : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] MeshRenderer[] renderers;
    [ContextMenu("Get Mesh Renderer On Child")]
    void GetMaterials()
    {
        renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
    }
    [ContextMenu("Override Materials")]
    void OverrideMaterial()
    {
        foreach (var item in renderers)
        {
            item.material = material;
        }
    }
    [ContextMenu("Flip Object")]
    void FlipRotate()
    {
        foreach (var item in renderers)
        {
            item.gameObject.transform.rotation = new Quaternion(180, 0, 0, 0);
        }
    }
}
