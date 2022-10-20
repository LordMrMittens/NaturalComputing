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
    Transform targetPhero;
    [SerializeField] float fieldOfVision;
    [SerializeField] float visionDistance;
    [SerializeField] LayerMask foodLayer;
    [SerializeField] float pickupDistance;
    public bool hasFood = false;
    bool foodDetected = false;
    [SerializeField] Transform home;

    [SerializeField] GameObject pheromone;
    GameObject pickedFood;
    [SerializeField] float pheromoneFrequency;
    [SerializeField] LayerMask pheromoneLayer;
    float pheromoneTimer;
    public List<Transform> homePheros = new List<Transform>();
    void Update()
    {

        pheromoneTimer += Time.deltaTime;
        

        if (hasFood)
        {
            FindPheromone();
            if (Vector2.Distance(home.position, jaws.position) < pickupDistance)
            {
                Destroy(pickedFood);
                hasFood = false;
                homePheros.Clear();
            }
        }
        else
        {
            direction = (direction + Random.insideUnitCircle * randomnessForce).normalized;
            FindFood();
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
            LeavePheromone();
            pheromoneTimer = 0;
        }
    }
    void FindPheromone()
    {
        if (hasFood)
        {
            if (homePheros.Count > 0)
            {
                Vector2 nextPheromone = homePheros[homePheros.Count - 1].transform.position;
                direction = (nextPheromone - (Vector2)jaws.position).normalized;
                if (Vector2.Distance(nextPheromone, jaws.position) < pickupDistance)
                {
                    homePheros.RemoveAt(homePheros.Count - 1);
                }
            }
            else
            {
                direction = ((Vector2)home.transform.position - (Vector2)jaws.position).normalized;
            }
        }
        else
        {
            if (targetPhero == null)
            {
                Transform targetPhero;
                Collider2D[] detectedPheros = Physics2D.OverlapCircleAll(currentPos, visionDistance, pheromoneLayer);
                for (int i = 0; i < detectedPheros.Length; i++)
                {
                    if (detectedPheros[i].name != "To Food")
                    {
                        detectedPheros[i] = null;
                    }
                }
                if (detectedPheros.Length > 0)
                {
                    float bestDistance = Vector2.Distance(detectedPheros[0].transform.position, jaws.position);
                    int bestIndex = 0;
                    for (int i = 0; i < detectedPheros.Length - 1; i++)
                    {
                        if (detectedPheros[i].name == "To Food")
                        {
                            float pheroIterationDistance = Vector2.Distance(detectedPheros[i].transform.position, jaws.position);
                            if (bestDistance > pheroIterationDistance)
                            {
                                bestIndex = i;
                                bestDistance = pheroIterationDistance;
                            }
                        }
                    }
                    Transform closestphero = detectedPheros[bestIndex].transform;
                    Vector2 PheroDirection = (closestphero.position - jaws.position).normalized;

                    if (Vector2.Angle(transform.right, PheroDirection) < fieldOfVision / 2)
                    {
                        targetPhero = closestphero;
                    }
                }
            }
            else
            {
                direction = ((Vector2)targetPhero.position - (Vector2)jaws.position).normalized;
                if (Vector2.Distance(targetPhero.position, jaws.position) < pickupDistance)
                {
                    targetPhero = null;
                }
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
                FindPheromone();
            }
        }
    }
    void LeavePheromone()
    {
        GameObject trail = Instantiate(pheromone, butt.position, Quaternion.identity);
        if (hasFood)
        {
            trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.backPhero, Color.red);
            trail.name = "To Food";
            
        }
        else
        {
            trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.blue);
            trail.name = "To Home";
            homePheros.Add(trail.transform);
        }
    }
}

