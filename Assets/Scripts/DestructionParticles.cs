using UnityEngine;

public class DestructionParticles : MonoBehaviour
{
    public float particleLifetime = 1.0f;
    private float timer;

    void Start()
    {
        
    }

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= particleLifetime)
        {
            Destroy(gameObject);
        }
    }
}
