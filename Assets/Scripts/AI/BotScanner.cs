using UnityEngine;
using UnityEngine.AI;
using Utils;

public class BotScanner : MonoBehaviour
{
    private NavMeshAgent agent;


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

        if (NavMeshUtilities.IsAtTargetLocation(agent) || Lifespan <= 0)
        {
            DestroySelf(null, null);
        }
    }

    public void DestroySelf(Component sender, object data)
    {
        Destroy(gameObject);        
    }
}
