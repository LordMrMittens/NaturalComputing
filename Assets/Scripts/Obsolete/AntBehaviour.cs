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
    [SerializeField] float dropDistance;
    [SerializeField] float homeDistance;
    public bool hasFood = false;
    bool foodDetected = false;
    [SerializeField] Transform home;
    [SerializeField] GameObject pheromone;
    GameObject pickedFood;
    [SerializeField] float pheromoneFrequency;
    [SerializeField] LayerMask pheromoneLayer;
    float pheromoneTimer;
    [SerializeField] Transform leftFeeler;
    [SerializeField] Transform centerFeeler;
    [SerializeField] Transform rightFeeler;
    [SerializeField] float feelerRadius = .1f;
    Transform lastPheromone;
    Transform lastRelevantPheromone;
    int obstacleAvoidDirection=0;
    
    void Update()
    {

        pheromoneTimer += Time.deltaTime;
        if (!hasFood)
        {
            FindFood();
            direction = (direction + Random.insideUnitCircle * randomnessForce).normalized;
        }
        else
        {
            if (Vector2.Distance(home.position, jaws.position) < homeDistance)
            {
                direction = (home.position - jaws.position).normalized;
            }
            if (Vector2.Distance(home.position, jaws.position) < dropDistance)
            {
                Destroy(pickedFood);
                hasFood = false;
                direction *= -direction;
            }
        }

        if (!foodDetected && Vector2.Distance(home.position, jaws.position) > homeDistance)
        {
            FindPheromone();
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
        int leftHomePheros = 0;
        int leftFoodPheros = 0;
        int centerHomePheros = 0;
        int centerFoodPheros = 0;
        int rightHomePheros = 0;
        int rightFoodPheros = 0;
        int leftObstacle = 0;
        int rightObstacle = 0;
        int centerObstacle = 0;

        Collider2D[] leftCollisions = Physics2D.OverlapCircleAll(leftFeeler.position, feelerRadius, pheromoneLayer);
        Collider2D[] centerCollisions = Physics2D.OverlapCircleAll(centerFeeler.position, feelerRadius, pheromoneLayer);
        Collider2D[] rightCollisions = Physics2D.OverlapCircleAll(rightFeeler.position, feelerRadius, pheromoneLayer);

        foreach (var pheromone in leftCollisions)
        {
            if (pheromone.name == "To Home")
            {
                leftHomePheros++;
            }
            else if (pheromone.name == "To Food")
            {
                leftFoodPheros++;
            }
            else
            {
                Debug.Log("Obstacle left");
                leftObstacle++;
            }
        }
        foreach (var pheromone in centerCollisions)
        {
            if (pheromone.name == "To Home")
            {
                centerHomePheros++;
            }
            else if (pheromone.name == "To Food")
            {
                centerFoodPheros++;
            }
            else
            {
                Debug.Log("Obstacle center");
                centerObstacle++;
            }
        }
        foreach (var pheromone in rightCollisions)
        {
            if (pheromone.name == "To Home")
            {
                rightHomePheros++;
            }
            else if (pheromone.name == "To Food")
            {
                rightFoodPheros++;
            }
            else
            {
                Debug.Log("Obstacle right");
                rightObstacle++;
            }
        }
        if (!hasFood)
        {
            if (rightFoodPheros > leftFoodPheros)
            {
                direction = (rightFeeler.transform.position - jaws.position).normalized;

            }
            else if (rightFoodPheros < leftFoodPheros)
            {
                direction = (leftFeeler.transform.position - jaws.position).normalized;

            }
            if (centerFoodPheros >= leftFoodPheros || centerFoodPheros >= rightFoodPheros)
            {

            }
        }
        else
        {
            if (rightHomePheros > leftHomePheros)
            {
                direction = (rightFeeler.transform.position - jaws.position).normalized;

            }
            else if (rightHomePheros < leftHomePheros)
            {
                direction = (leftFeeler.transform.position - jaws.position).normalized;

            }
            else if (centerHomePheros >= leftHomePheros || centerHomePheros >= rightHomePheros)
            {
                direction = (centerFeeler.transform.position - jaws.position).normalized;

            }
            if (centerHomePheros == 0 && leftHomePheros == 0 && rightHomePheros == 0)
            {
                direction = (home.position - jaws.position).normalized;
            }
        }
        if (leftObstacle > 0)
        {
            direction = (rightFeeler.transform.position - jaws.position).normalized;
        }
        if (rightObstacle > 0)
        {
            direction = (leftFeeler.transform.position - jaws.position).normalized;
        }
        if (centerObstacle > 0)
        {
            if(obstacleAvoidDirection==0){
            obstacleAvoidDirection = Random.Range(1, 3);
            }
            if ( obstacleAvoidDirection > 1)
            {
                direction = (leftFeeler.transform.position - jaws.position).normalized;
            }
            else
            {
                direction = (rightFeeler.transform.position - jaws.position).normalized;
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
                direction = (lastPheromone.position - jaws.position).normalized;
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
    void LeavePheromone()
    {
        GameObject trail = Instantiate(pheromone, butt.position, Quaternion.identity);
        if (hasFood)
        {
            trail.GetComponent<Pheromone>().SetupPheromone(Color.red, 3, "To Food");

        }
        else
        {
            trail.GetComponent<Pheromone>().SetupPheromone(Color.red, 3, "To Home");
            lastPheromone = trail.transform;
        }

    }
}