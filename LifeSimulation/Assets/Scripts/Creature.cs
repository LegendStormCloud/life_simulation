using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Genes
{
    [Range(1.5f, 4.5f)]
    public float speed = 3f;
    [Range(0, 180)]
    public float FOV = 45f;
    [Range(0.5f, 3.5f)]
    public float sight = 2f;

    public Genes(Genes pA = null, Genes pB = null)
    {
        if (pA == null && pB != null || pA != null && pB == null)
        {
            Debug.LogError("Error only one of the parents has been inserted");
            return;
        }

        //initial run
        if (pA == null && pB == null)
        {
            speed = 3f; FOV = 45f; sight = 2f;
            return;
        }

        //from reproduction of two creatures

        float spd = (pA.speed + pB.speed) / 2 + (.3f * (Random.value - 0.5f));
        float fov = (pA.FOV + pB.FOV) / 2 + (18f * (Random.value - 0.5f));
        float sgt = (pA.sight + pB.sight) / 2 + (.3f * (Random.value - 0.5f));

        speed = spd;
        FOV = fov;
        sight = sgt;
    }
}

public class Creature : MonoBehaviour
{
    public Genes genes;

    public LayerMask foodMask;

    float cosFOVh;
    public int steps = 12;

    public float fakeRotation = 0;
    public Vector2 fakeForward
    {
        get 
        {
            return Quaternion.Euler(0, 0, fakeRotation) * transform.right;
        }
    }

    //public bool foundFood;
    public bool searchingForFood = true;
    public GameObject targetFood;
    public Vector3 targetPos;

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, 0.1f);

        Vector3 lastPos = Quaternion.Euler(0, 0, -genes.FOV / 2) * fakeForward * genes.sight;
        Gizmos.DrawLine(transform.position, transform.position + lastPos);
        for (int i = 0; i < steps; i++)
        {
            Vector3 newPos = Quaternion.Euler(0, 0, genes.FOV / steps) * lastPos;
            Gizmos.DrawLine(transform.position + lastPos, transform.position + newPos);
            lastPos = newPos;
        }
        Gizmos.DrawLine(transform.position + lastPos, transform.position);
    }

    public void InitCreature(Genes a = null, Genes b = null)
    {
        //set genes
        genes = new(a, b);

        cosFOVh = Mathf.Cos(genes.FOV * Mathf.Deg2Rad / 2);
        targetPos = Random.insideUnitCircle * genes.sight;
        searchingForFood = true;
    }

    void ResetSearchingFoodVars()
    {
        targetFood = null;
        targetPos = Random.insideUnitCircle * genes.sight;
        searchingForFood = true;
    }


    private void Update()
    {
        if (searchingForFood)
        {
            Vector3 delta = targetPos - transform.position;
            if (delta.sqrMagnitude <= 0.01f)
            {
                targetPos = Random.insideUnitCircle.normalized * genes.sight;
                while(targetPos.sqrMagnitude > SimulationManager.instance.simRadius*SimulationManager.instance.simRadius)
                {
                    targetPos = Random.insideUnitCircle.normalized * genes.sight;
                }
            }
            fakeRotation = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            transform.Translate(delta.normalized * genes.speed * Time.deltaTime);

            Collider2D[] foodCs = Physics2D.OverlapCircleAll(transform.position, genes.sight, foodMask);
            if (foodCs.Length == 0) return;

            foodCs = foodCs.OrderBy((f) => (f.transform.position - transform.position).sqrMagnitude).ToArray();
            Vector3 fPos = foodCs[0].transform.position;

            if ((fPos - transform.position).sqrMagnitude > genes.sight * genes.sight) return;

            float dotP = Vector2.Dot(fakeForward, fPos);
            if (dotP < 0) return;

            float cosF = dotP / (fPos.magnitude);
            if (cosF < cosFOVh) return;

            targetFood = foodCs[0].transform.gameObject;
            searchingForFood = false;
            //inside the FOV, this works only with FOV angles between 0 adn 180
        }
        else
        {
            if (targetFood == null)
            {
                ResetSearchingFoodVars();
                return;
            }

            Vector3 dir = targetFood.transform.position - transform.position;
            fakeRotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.Translate(dir.normalized * genes.speed * Time.deltaTime);

            if (dir.sqrMagnitude > genes.sight * genes.sight) ResetSearchingFoodVars();
            if (dir.sqrMagnitude > 0.01f) return;

            Destroy(targetFood);
            ResetSearchingFoodVars();
        }
    }
}
