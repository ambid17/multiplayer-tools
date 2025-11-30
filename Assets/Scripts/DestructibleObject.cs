using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public GameObject explosionEffectPrefab;
    private readonly int PlayerLayer = 7; 
    public float requiredImpactForce = 5.0f;
    public float explosionRadius = 5.0f;
    public float explosionForce = 500.0f;

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
        if(hasExploded)
            return;

        if (collision.gameObject.layer == PlayerLayer)
        {
            Debug.DrawLine(collision.transform.position, collision.GetContact(0).point, Color.red, 10.0f);
            var impactForce = collision.relativeVelocity.magnitude * collision.transform.localScale.magnitude;

            if(impactForce >= requiredImpactForce)
            {
                Explode(collision.GetContact(0).point);
            }
        }
    }

    private void Explode(Vector3 hitPoint)
    {
        hasExploded = true;
        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

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
            rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);
        }
    }

    private Vector3 GetCenterOfObject()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return transform.position;
        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }
        return combinedBounds.center;
    }
}
