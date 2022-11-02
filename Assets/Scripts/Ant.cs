using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
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
    [SerializeField] float pheromoneDistanceFrequency;
    [SerializeField] LayerMask pheromoneLayer;
    float pheromoneTimer;
    [SerializeField] Transform leftFeeler;
    [SerializeField] Transform centerFeeler;
    [SerializeField] Transform rightFeeler;
    [SerializeField] float feelerRadius = .1f;
    Transform lastPheromone;
    int obstacleAvoidDirection = 0;
    int leftDesirability;
    int rightDesirability;
    int centerDesirability;
    int maxDesirability;
    List<GameObject> ownPheromones = new List<GameObject>();
    Transform player;
    void Start()
    {
        LeavePheromone();
    }
    void Update()
    {

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
        if (Vector3.Distance(lastPheromone.position, transform.position) > pheromoneDistanceFrequency)
        {
            LeavePheromone();
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
        int leftPlayerPheromone = 0;
        int centerPlayerPheromone = 0;
        int rightPlayerPheromone = 0;
        int leftObstacle = 0;
        int rightObstacle = 0;
        int centerObstacle = 0;
        float leftHomeDistance = Vector3.Distance(leftFeeler.position, home.position);
        float rightHomeDistance = Vector3.Distance(rightFeeler.position, home.position);
        List<int> desirabilitySelector = new List<int>();

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
            else if (pheromone.name == "To Player")
            {
                leftPlayerPheromone++;
            }
            else
            {
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
            else if (pheromone.name == "To Player")
            {
                centerPlayerPheromone++;
            }
            else
            {
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
            else if (pheromone.name == "To Player")
            {
                rightPlayerPheromone++;
            }
            else
            {
                rightObstacle++;
            }
        }
        if (!hasFood)
        {
            if (!player)
            {
                rightDesirability = rightFoodPheros;
                leftDesirability = leftFoodPheros;
                centerDesirability = centerFoodPheros;
            }
            else
            {
                rightDesirability = rightPlayerPheromone;
                centerDesirability = centerPlayerPheromone;
                leftDesirability = leftPlayerPheromone;
            }
        }
        else
        {

            rightDesirability = rightHomePheros;
            leftDesirability = leftHomePheros;
            centerDesirability = centerHomePheros;
            if (rightHomeDistance < leftHomeDistance)
            {
                rightDesirability += 2;
            }
            else
            {
                leftDesirability += 2;
            }
        }
        for (int i = 0; i < rightDesirability; i++)
        {
            desirabilitySelector.Add(0);
        }
        for (int i = 0; i < centerDesirability; i++)
        {
            desirabilitySelector.Add(1);
        }
        for (int i = 0; i < leftDesirability; i++)
        {
            desirabilitySelector.Add(2);
        }
        if (desirabilitySelector.Count > 0)
        {
            int directionToChoose = desirabilitySelector[Random.Range(0, desirabilitySelector.Count - 1)];

            switch (directionToChoose)
            {
                case 0:
                    direction = (rightFeeler.transform.position - jaws.position).normalized;
                    break;
                case 1:
                    direction = (centerFeeler.transform.position - jaws.position).normalized;
                    break;
                case 2:
                    direction = (leftFeeler.transform.position - jaws.position).normalized;
                    break;
                default:
                    direction = (centerFeeler.transform.position - jaws.position).normalized;
                    break;
            }
        }

        AvoidObstacles(leftObstacle, rightObstacle, centerObstacle);

    }
    void AttackPlayer()
    {
        if (player != null)
        {

            Vector2 foodDirection = (player.position - jaws.position).normalized;

            if (Vector2.Angle(transform.right, foodDirection) < fieldOfVision / 2)
            {

                targetFood = player;
                foodDetected = true;
            }

        }

    }

    private void AvoidObstacles(int leftObstacle, int rightObstacle, int centerObstacle)
    {
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
            if (obstacleAvoidDirection == 0)
            {
                obstacleAvoidDirection = Random.Range(1, 3);
            }
            if (obstacleAvoidDirection > 1)
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
                    if (detectedFood[i].gameObject.tag == "Player")
                    {
                        player = detectedFood[i].transform;
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
            trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.backPhero, Color.red);
            trail.name = "To Food";
            lastPheromone = trail.transform;
            ownPheromones.Add(trail);

        }
        else
        {
            trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.blue);
            trail.name = "To Home";
            lastPheromone = trail.transform;
            ownPheromones.Add(trail);
        }

    }
    void ConvertPheromones()
    {
        foreach (GameObject pheromone in ownPheromones)
        {
            pheromone.name = "To Player";
        }
    }
}
