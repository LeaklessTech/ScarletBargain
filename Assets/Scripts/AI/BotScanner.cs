using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class BotScanner : MonoBehaviour
{
    private NavMeshAgent agent;

    public ParticleSystem Explosion;

    private bool isDestroyed = false;

    public float Lifespan = 30;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Lifespan > 0)
        {
            Lifespan -= Time.deltaTime;
        }

        if ((NavMeshUtilities.IsAtTargetLocation(agent) || Lifespan <= 0) && !isDestroyed)
        {
            DestroySelf(null, null);
        }

        if(isDestroyed && !Explosion.isPlaying)
        {
            Destroy(gameObject);
        }
        
    }

    public void DestroySelf(Component sender, object data)
    {
        Explosion.Play();
        this.GetComponentInChildren<MeshRenderer>().enabled = false;
        this.GetComponentsInChildren<AudioSource>()[0].enabled = false;
        this.GetComponentsInChildren<AudioSource>()[1].Play();
        this.GetComponentInChildren<Light>().enabled = false;
        isDestroyed = true;
    }
}
