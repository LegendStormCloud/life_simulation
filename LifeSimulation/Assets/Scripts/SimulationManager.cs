using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public GameObject creaureGO;
    public GameObject foodGO;
    public int creatureNumber = 25;
    public int foodNumber = 25;
    public float spawnRadius = 1f;
    public float simRadius = 5f;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, simRadius);
    }

    public void StartSimulation()
    {
        float angle = 0;
        for(int i = 0; i < creatureNumber; i++)
        {
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Creature c = Instantiate(creaureGO, pos, Quaternion.identity).GetComponent<Creature>();

            c.InitCreature();

            angle += (2 * Mathf.PI / creatureNumber);
        }

        for(int i = 0; i < foodNumber; i++)
        {
            Vector2 pos = Random.insideUnitCircle * simRadius;
            Instantiate(foodGO, pos, Quaternion.identity);
        }
    }
}
