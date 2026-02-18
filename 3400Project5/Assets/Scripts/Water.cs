using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] float speed = 0.25f;
    [SerializeField] Material _mat;
    Vector2 _offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _offset.x += speed * Time.deltaTime;
        _offset.x = Mathf.Repeat(_offset.x, 1f);
        _mat.SetTextureOffset("_BaseMap", _offset);
    }
}
