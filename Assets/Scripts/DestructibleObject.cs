using FishNet.Component.Transforming;
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
            //explosionTimer += Time.deltaTime;
            //if(explosionTimer >= childDurationAfterExplosion)
            //{
            //    foreach (Transform child in transform)
            //    {
            //        Destroy(child.gameObject);
            //    }
            //    Destroy(gameObject);
            //}
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == GameManager.PlayerLayer)
        {
            var impactForce = collision.rigidbody.mass * collision.relativeVelocity.magnitude;

            // TODO: this breaks sending explosions to the client
            //if(impactForce >= requiredImpactForce)
            //{
                var impactForceMultiplier = impactForce / requiredImpactForce;
                Debug.Log("Sending explosion RPC");
                Explode(collision.GetContact(0).point, impactForceMultiplier);
            //}
        }
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void Explode(Vector3 hitPoint, float impactForceMultiplier)
    {
        Debug.Log("Recieved explosion RPC");
        // check if already exploded, prevent double interaction
        if (hasExploded) return;
        hasExploded = true;

        NetworkObject obj = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Spawn(obj);

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
            // TODO: adjust mass based on size of child? Might not be necessary since force is based on requiredImpactForce
            // maybe just calcualte required force to be based on the child size?
            var rb = child.gameObject.AddComponent<Rigidbody>();
            child.gameObject.AddComponent<NetworkTransform>();

            // we want the explosion to push outwards from the hit point, but always have an upwards component
            rb.AddExplosionForce(explosionForce, hitPoint, 1000, 10);
        }
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
