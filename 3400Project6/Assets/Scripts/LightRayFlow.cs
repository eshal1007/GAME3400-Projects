using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightRawFlow : MonoBehaviour
{
    // material used by stream line
    // uv offset is animated every frame
    public Material streamMaterial;
    
    // uv scroll speed
    public float scrollSpeed; 

    void Update()
    {
        // no material assigned
        if (!streamMaterial) return;
        
        // get current texture offset
        Vector2 offset = streamMaterial.mainTextureOffset;
        
        // move texture along x over time
        // change sign to reverse direction
        offset.x += scrollSpeed * Time.deltaTime; 
        
        // apply updated offset to the material
        streamMaterial.mainTextureOffset = offset;
    }
}
