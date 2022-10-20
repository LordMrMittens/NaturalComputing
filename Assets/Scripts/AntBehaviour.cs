using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntBehaviour : MonoBehaviour
{
    [SerializeField] float maxSpeed = 2;
    [SerializeField] float turningForce = 2;
    [SerializeField] float randomnessForce = 1;
    [SerializeField] float pheromoneAttraction = 1;
    Vector2 currentPos;
    Vector2 velocity;
    Vector2 direction;
    [SerializeField] Transform jaws;
    [SerializeField] Transform butt;
    Transform targetFood;
    [SerializeField] float fieldOfVision;
    [SerializeField] float visionDistance;
    [SerializeField] LayerMask foodLayer;
    [SerializeField] float pickupDistance;
    bool hasFood = false;
    bool foodDetected = false;
    [SerializeField] Transform home;

    [SerializeField] GameObject pheromone;
    GameObject pickedFood;
    [SerializeField] float pheromoneFrequency;
    [SerializeField] LayerMask pheromoneLayer;
    float pheromoneTimer;

    void Update()
    {
        //movement needs to be redone!!
        FindPheromone();
        pheromoneTimer+= Time.deltaTime;
        if (!hasFood)
        {
            FindFood();
        }
        if (!hasFood && !foodDetected)
        {
            direction = (direction + Random.insideUnitCircle * randomnessForce).normalized;
        }
        else if (hasFood)
        {
            direction = (home.position - jaws.position).normalized;
            if (Vector2.Distance(home.position, jaws.position) < pickupDistance)
            {
                Destroy(pickedFood);
                hasFood = false;
            }
        }
        Vector2 desiredVelocity = direction * maxSpeed;
        Vector2 desiredTurningForce = (desiredVelocity - velocity) * turningForce;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredTurningForce, turningForce) / 1;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        currentPos += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(currentPos, Quaternion.Euler(0, 0, angle));
        if (velocity.magnitude > 0 && pheromoneTimer >= pheromoneFrequency)
        {
            
            GameObject trail = LeavePheromone();
            pheromoneTimer=0;
            if (hasFood)
            {
                trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.backPhero, Color.red);
            }
            else
            {
                trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.blue);
            }
        }
    }
    void FindPheromone()
    {
        Collider2D[] detectedPheromone = Physics2D.OverlapCircleAll(currentPos, visionDistance, pheromoneLayer);
        if (detectedPheromone.Length > 0)
        {
            int randomPheromone = Random.Range(0,detectedPheromone.Length-1);
            //find home or away pheromone needed need to implement
            if (Vector2.Angle(transform.right, detectedPheromone[randomPheromone].transform.position) < fieldOfVision / 2)
            {
                direction = (direction + (Vector2)detectedPheromone[randomPheromone].transform.position * pheromoneAttraction).normalized;
            }
        }
    }

    void FindFood()
    {
        if (targetFood == null)
        {
            Collider2D[] detectedFood = Physics2D.OverlapCircleAll(currentPos, visionDistance, foodLayer);
            if (detectedFood.Length > 0)
            {
                float bestDistance = Vector2.Distance(detectedFood[0].transform.position, jaws.position);
                int bestIndex = 0;
                for (int i = 0; i < detectedFood.Length - 1; i++)
                {
                    float foodIterationDistance = Vector2.Distance(detectedFood[i].transform.position, jaws.position);
                    if (bestDistance > foodIterationDistance)
                    {
                        bestIndex = i;
                        bestDistance = foodIterationDistance;
                    }
                }
                Transform closestFood = detectedFood[bestIndex].transform;
                Vector2 foodDirection = (closestFood.position - jaws.position).normalized;

                if (Vector2.Angle(transform.right, foodDirection) < fieldOfVision / 2)
                {
                    targetFood = closestFood;
                    foodDetected = true;
                }
            }
        }
        else
        {
            direction = (targetFood.position - jaws.position).normalized;
            if (Vector2.Distance(targetFood.position, jaws.position) < pickupDistance)
            {
                targetFood.position = jaws.position;
                targetFood.parent = jaws;
                targetFood.gameObject.layer = LayerMask.NameToLayer("Default");
                pickedFood = targetFood.gameObject;
                targetFood = null;
                hasFood = true;
                foodDetected = false;
            }
        }
    }
    GameObject LeavePheromone()
    {
        GameObject trail = Instantiate(pheromone, butt.position, Quaternion.identity);
        return trail;

    }
}

