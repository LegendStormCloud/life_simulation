using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public LayerMask foodMask;
    public float sight;
    public float FOV;
    float cosFOVh;
    public int steps = 12;

    //public bool foundFood;
    public bool searchingForFood = true;
    public GameObject targetFood;
    public float speed = 4f;

    void OnDrawGizmosSelected()
    {
        Vector3 lastPos = Quaternion.Euler(0, 0, -FOV / 2) * transform.right * sight;
        Gizmos.DrawLine(transform.position, transform.position + lastPos);
        for (int i = 0; i < steps; i++)
        {
            Vector3 newPos = Quaternion.Euler(0, 0, FOV / steps) * lastPos;
            Gizmos.DrawLine(transform.position + lastPos, transform.position + newPos);
            lastPos = newPos;
        }
        Gizmos.DrawLine(transform.position + lastPos, transform.position);
    }

    private void Start()
    {
        InitCreature();
    }

    void InitCreature()
    {
        cosFOVh = Mathf.Cos(FOV * Mathf.Deg2Rad / 2);
        searchingForFood = true;
    }

    private void Update()
    {
        Collider2D[] foodCs = Physics2D.OverlapCircleAll(transform.position, sight, foodMask);
        if (foodCs.Length == 0) return;
        foodCs = foodCs.OrderBy((f) => (f.transform.position - transform.position).sqrMagnitude).ToArray();

        targetFood = foodCs[0].transform.gameObject;
        Vector3 fPos = foodCs[0].transform.position;

        if ((fPos - transform.position).sqrMagnitude > sight * sight) return;
        
        float dotP = Vector2.Dot(transform.right, fPos);
        if (dotP < 0) return;

        float cosF = dotP/(fPos.magnitude);
        if (cosF < cosFOVh) return;

        searchingForFood = false;
        //inside the FOV, this works only with FOV angles between 0 adn 180

        if (!searchingForFood)
        {
            if (targetFood == null)
            {
                searchingForFood = true;
                return;
            }

            Vector3 dir = fPos - transform.position;
            transform.Translate(dir.normalized * speed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.01f) return;

            Destroy(targetFood);
            targetFood = null;
            searchingForFood = false;
            Debug.Log("Food eaten");
        }
    }
}
