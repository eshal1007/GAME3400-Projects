using UnityEngine;

public class Float : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float amplitude = 0.25f;
    [SerializeField] float speed = 1.0f;
    [SerializeField] float initialLocalY = 0f;
    Vector3 _baseLocalPos;
  

    void Awake()
    {
        _baseLocalPos = transform.localPosition;
        _baseLocalPos.y = initialLocalY;
        transform.localPosition = _baseLocalPos;
    }

    void Update()
    {
        float y = _baseLocalPos.y + Mathf.Sin(Time.time * (Mathf.PI * 2f) * speed) * amplitude;
        transform.localPosition = new Vector3(_baseLocalPos.x, y, _baseLocalPos.z);
    }
}
