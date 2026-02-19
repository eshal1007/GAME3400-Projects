using System.Collections;
using UnityEngine;

public class BreakableGlass : MonoBehaviour
{
    [SerializeField] private Collider floorCollider;
    [SerializeField] private Renderer intactRenderer;
    [SerializeField] private Transform surfacePoint;
    [SerializeField] private GameObject shardsRoot;
    [SerializeField] private float explosionForce = 2.0f;
    [SerializeField] private float explosionRadius = 1.2f;
    [SerializeField] private float upwardModifier = 0.15f;
    [SerializeField] private float breakDelay = 0.10f;
    [SerializeField] private float shardLifetime = 5.0f;

    private bool broken = false;

    private void Awake()
    {
        if (shardsRoot != null)
            shardsRoot.SetActive(false);
    }

    public void TryBreak(Collider other)
    {
        if (broken) return;
        if (!other.CompareTag("Player")) return;

        if (surfacePoint != null)
        {
            float playerFeetY = other.bounds.min.y;
            float glassY = surfacePoint.position.y;
            if (playerFeetY < glassY - 0.03f) return;
        }

        broken = true;
        StartCoroutine(BreakRoutine());
    }

    private IEnumerator BreakRoutine()
    {
        yield return new WaitForSeconds(breakDelay);

        if (floorCollider != null) floorCollider.enabled = false;
        if (intactRenderer != null) intactRenderer.enabled = false;

        if (shardsRoot != null)
        {
            shardsRoot.SetActive(true);
            yield return new WaitForFixedUpdate();

            Vector3 origin = (surfacePoint != null) ? surfacePoint.position : transform.position;

            var bodies = shardsRoot.GetComponentsInChildren<Rigidbody>(true);

            if (bodies.Length == 0)
            {
                var renderers = shardsRoot.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    var go = r.gameObject;
                    var rb = go.GetComponent<Rigidbody>();
                    if (rb == null) rb = go.AddComponent<Rigidbody>();
                    rb.mass = 0.2f;
                    rb.drag = 0f;
                    rb.angularDrag = 0.05f;
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    var mc = go.GetComponent<MeshCollider>();
                    if (mc == null) mc = go.AddComponent<MeshCollider>();
                    mc.convex = true;
                }

                bodies = shardsRoot.GetComponentsInChildren<Rigidbody>(true);
            }

            foreach (var rb in bodies)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.useGravity = true;
                rb.detectCollisions = true;
                rb.isKinematic = false;
                rb.WakeUp();
                rb.AddExplosionForce(explosionForce, origin, explosionRadius, upwardModifier, ForceMode.Impulse);
            }

            Destroy(shardsRoot, shardLifetime);
        }
    }
}
