using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class BotScanner : MonoBehaviour
{
    private NavMeshAgent agent;

    public ParticleSystem Explosion;
    public AudioSource AudioSource;

    public Transform droneTransform;

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
        var audio = Instantiate(AudioSource, this.gameObject.transform);
        audio.Play();

        this.GetComponent<NavMeshAgent>().isStopped = true;
        this.GetComponent<NavMeshAgent>().velocity = Vector3.zero;
        this.GetComponentInChildren<MeshRenderer>().enabled = false;
        this.GetComponentsInChildren<AudioSource>()[0].enabled = false;
        this.GetComponentInChildren<Light>().enabled = false;
        isDestroyed = true;
        var ex = Instantiate(Explosion, droneTransform);
        ex.Play();
    }
}
