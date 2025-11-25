using UnityEngine;

public class EnergyBall : MonoBehaviour
{

    public int huntedPlayerId;
    public GameEvent onCharacterKilled;
    public ParticleSystem Explosion;
    public AudioSource AudioSource;
    public float Lifespan = 3f;
    private bool isDestroyed = false;

    void Update()
    {
        if (Lifespan > 0)
        {
            Lifespan -= Time.deltaTime;
        }

        if(Lifespan <= 0 && !isDestroyed)
        {
            ExplodeBall(this.gameObject);
        }

        if(isDestroyed && !Explosion.isPlaying)
        {
            Destroy(gameObject);
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player" || collision.transform.tag == "Prisoner")
        {
            onCharacterKilled.TriggerEvent(this, collision.gameObject.GetInstanceID());
            ExplodeBall(this.gameObject);    
        }
    }


    private void ExplodeBall(GameObject gameObject)
    {
        var audio = Instantiate(AudioSource, this.transform);
        audio.Play();

        this.GetComponent<Rigidbody>().isKinematic = true;
        this.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        this.GetComponentInChildren<MeshRenderer>().enabled = false;
        isDestroyed = true;
        var ex = Instantiate(Explosion, this.transform);
        ex.Play();
    }

}
