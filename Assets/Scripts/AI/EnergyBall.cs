using UnityEngine;

public class EnergyBall : MonoBehaviour
{

    public int huntedPlayerId;
    public GameEvent onCharacterKilled;

    public float Lifespan = 3f;


    void Update()
    {
        if (Lifespan > 0)
        {
            Lifespan -= Time.deltaTime;
        }

        if(Lifespan <= 0)
        {
            ExplodeBall(this.gameObject);
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player" || collision.transform.tag == "Prisoner")
        {
            onCharacterKilled.TriggerEvent(this, huntedPlayerId);
            ExplodeBall(this.gameObject);    
        }

        
    }


    private void ExplodeBall(GameObject gameObject)
    {


        Destroy(gameObject);
    }

}
