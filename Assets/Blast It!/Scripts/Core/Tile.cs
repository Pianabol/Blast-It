using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Tile : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            
            // DÜZELTME: Hem eski nesil Standart hem de yeni nesil URP materyallerini desteklemesi için iki property'e de renk veriyoruz.
            propBlock.SetColor("_Color", color);      // Standart Pipeline
            propBlock.SetColor("_BaseColor", color);  // Universal Render Pipeline (URP)
            
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
}