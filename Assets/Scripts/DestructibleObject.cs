using FishNet.Object;
using System.Buffers.Text;
using System.Drawing;
using UnityEngine;

public class DestructibleObject : NetworkBehaviour
{
    public NetworkObject explosionEffectPrefab;
    public float requiredImpactForce = 5.0f;
    public float baseExplosionForce = 500.0f;

    private float explosionTimer = 0.0f;
    private float childDurationAfterExplosion = 2.0f;
    private bool hasExploded = false;

    private Rigidbody rb;

    void Start()
    {
        gameObject.layer = GameManager.DestructibleLayer;
        SetupCollider();
    }

    void Update()
    {
        if(hasExploded)
        {
            explosionTimer += Time.deltaTime;
            if(explosionTimer >= childDurationAfterExplosion)
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
            // F = m * a (where a = change in velocity / time, in this case we assume all of the mass is applied and an instant stop time)
            var impactForce = collision.rigidbody.mass * collision.relativeVelocity.magnitude;

            if(impactForce >= requiredImpactForce)
            {
                var impactForceMultiplier = impactForce / requiredImpactForce;
                Explode(collision.GetContact(0).point, impactForceMultiplier);
            }
        }
    }

    public void Explode(Vector3 hitPoint, float impactForceMultiplier)
    {
        // re-check if already exploded, required for ExplosionTester
        if (hasExploded) return;
        hasExploded = true;

        SpawnExplosion();

        var explosionForce = baseExplosionForce * impactForceMultiplier;

        // if the parent object has a mesh, add a rigidbody to it too so it can be affected by the explosion
        // prevents moving an invisible parent object
        var myMesh = gameObject.GetComponent<MeshFilter>();
        if (myMesh)
        {
            rb.AddExplosionForce(explosionForce, hitPoint, 1000);
        }
        

        foreach (Transform child in transform)
        {
            var rb = child.gameObject.AddComponent<Rigidbody>();
            // TODO: adjust mass based on size of child? Might not be necessary since force is based on requiredImpactForce
            // maybe just calcualte required force to be based on the child size?
            rb.mass = 1.0f; 

            // we want the explosion to push outwards from the hit point, but always have an upwards component
            rb.AddExplosionForce(explosionForce, hitPoint, 1000, 10);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnExplosion()
    {
        Debug.Log("Received explosion message from server");
        NetworkObject obj = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Spawn(obj);
    }

    private void SetupCollider()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        if(transform.childCount == 0)
        {
            var collider = gameObject.GetComponent<Collider>();
            if (!collider)
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }
            return;
        }

        foreach (Transform child in transform)
        {
            var collider = child.gameObject.GetComponent<Collider>();
            if (!collider)
            {
                collider = child.gameObject.AddComponent<BoxCollider>();
            }
        }

        //rb.isKinematic = true;
    }

}
