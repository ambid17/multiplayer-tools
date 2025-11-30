using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public GameObject explosionEffectPrefab;
    public float requiredImpactForce = 5.0f;
    public float baseExplosionForce = 500.0f;

    private float explosionTimer = 0.0f;
    public float explosionDuration = 2.0f;
    private bool hasExploded = false;

    void Start()
    {
        
    }

    void Update()
    {
        if(hasExploded)
        {
            explosionTimer += Time.deltaTime;
            if(explosionTimer >= explosionDuration)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(hasExploded) return;

        if (collision.gameObject.layer == GameManager.PlayerLayer)
        {
            Debug.DrawLine(collision.transform.position, collision.GetContact(0).point, Color.red, 10.0f);
            var impactForce = collision.relativeVelocity.magnitude * collision.transform.localScale.magnitude;

            if(impactForce >= requiredImpactForce)
            {
                Explode(collision.GetContact(0).point, impactForce / requiredImpactForce);
            }
        }
    }

    public void Explode(Vector3 hitPoint, float impactForceMultiplier)
    {
        if (hasExploded) return;
        hasExploded = true;
        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        var explosionForce = baseExplosionForce * impactForceMultiplier;
        var explosionRadius = GetObjectSize().magnitude * 1.1f;
        var myRb = gameObject.AddComponent<Rigidbody>();
        myRb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);

        foreach (Transform child in transform)
        {
            var rb = child.gameObject.AddComponent<Rigidbody>();
            rb.mass = 1.0f;

            if(hitPoint.y > child.position.y)
            {
                hitPoint.y = child.position.y - 0.5f; // ensure explosion is below the child so it flies upwards
            }
            Debug.DrawLine(hitPoint, child.position, Color.blue, 10.0f);
            rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);
        }
    }

    private Vector3 GetCenterOfObject()
    {
        return GetCombinedBounds().center;
    }

    private Vector3 GetObjectSize()
    {
        return GetCombinedBounds().size;
    }

    private Bounds GetCombinedBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(transform.position, Vector3.zero);
        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }
        return combinedBounds;
    }
}
