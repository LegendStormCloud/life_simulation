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
    public float speed = 4f;

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, 0.1f);

        Vector3 lastPos = Quaternion.Euler(0, 0, -FOV / 2) * fakeForward * sight;
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
        targetPos = Random.insideUnitCircle * sight;
        searchingForFood = true;
    }

    void ResetSearchingFoodVars()
    {
        targetFood = null;
        targetPos = Random.insideUnitCircle * sight;
        searchingForFood = true;
    }


    private void Update()
    {
        if (searchingForFood)
        {
            Vector3 delta = targetPos - transform.position;
            if (delta.sqrMagnitude <= 0.01f)
            {
                targetPos = Random.insideUnitCircle.normalized * sight;
            }
            fakeRotation = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            transform.Translate(delta.normalized * speed * Time.deltaTime);

            Collider2D[] foodCs = Physics2D.OverlapCircleAll(transform.position, sight, foodMask);
            if (foodCs.Length == 0) return;

            foodCs = foodCs.OrderBy((f) => (f.transform.position - transform.position).sqrMagnitude).ToArray();
            Vector3 fPos = foodCs[0].transform.position;

            if ((fPos - transform.position).sqrMagnitude > sight * sight) return;

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
            transform.Translate(dir.normalized * speed * Time.deltaTime);

            if (dir.sqrMagnitude > sight * sight) ResetSearchingFoodVars();
            if (dir.sqrMagnitude > 0.01f) return;

            Destroy(targetFood);
            ResetSearchingFoodVars();
        }
    }
}
