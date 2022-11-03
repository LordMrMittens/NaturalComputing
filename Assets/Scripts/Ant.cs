using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{

    [SerializeField] float maxSpeed = 2;
    [SerializeField] float turningForce = 2;
    [SerializeField] float randomnessForce = 1;
    [SerializeField] int ignorePheromoneFactor=3;
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
    [SerializeField] float dropDistance;
    [SerializeField] float homeDistance;
    bool hasFood = false;
    bool foodDetected = false;
    [SerializeField] Transform home;
    [SerializeField] GameObject pheromone;
    GameObject pickedFood;
    [SerializeField] float pheromoneDistanceFrequency;
    [SerializeField] LayerMask pheromoneLayer;
    [SerializeField] Transform leftFeeler;
    [SerializeField] Transform centerFeeler;
    [SerializeField] Transform rightFeeler;
    [SerializeField] float feelerRadius = .1f;
    Transform lastPheromone;
    int obstacleAvoidDirection = 0;
    int leftDesirability;
    int rightDesirability;
    int centerDesirability;
    List<GameObject> ownPheromones = new List<GameObject>();
    Transform player;
    float pheromoneListCounter;
    float pheromoneListClearInterval;
    [SerializeField] float playerLoseDistance = 10;
    void Start()
    {
        LeavePheromone();
    }
    void Update()
    {
        pheromoneListCounter += Time.deltaTime;
        if (pheromoneListCounter > pheromoneListClearInterval)
        {
            ClearPheromoneList();
        }
        Navigation();
        if (Vector3.Distance(lastPheromone.position, transform.position) > pheromoneDistanceFrequency)
        {
            LeavePheromone();
        }
    }

    private void Navigation()
    {
        UseFeelers();
        if (!hasFood)
        {
            FindTarget();
            direction = (direction + Random.insideUnitCircle * randomnessForce).normalized;
            if (player != null)
            {
                AttackPlayer();
            }
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
                ClearPheromoneList();
            }
        }

        Vector2 desiredVelocity = direction * maxSpeed;
        Vector2 desiredTurningForce = (desiredVelocity - velocity) * turningForce;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredTurningForce, turningForce) / 1;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        currentPos += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(currentPos, Quaternion.Euler(0, 0, angle));
    }

    void UseFeelers()
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
        int currentIgnorePheroFactor = ignorePheromoneFactor;
        float leftHomeDistance = Vector3.Distance(leftFeeler.position, home.position);
        float rightHomeDistance = Vector3.Distance(rightFeeler.position, home.position);
        List<int> desirabilitySelector = new List<int>();

        Collider2D[] leftCollisions = Physics2D.OverlapCircleAll(leftFeeler.position, feelerRadius, pheromoneLayer);
        Collider2D[] centerCollisions = Physics2D.OverlapCircleAll(centerFeeler.position, feelerRadius, pheromoneLayer);
        Collider2D[] rightCollisions = Physics2D.OverlapCircleAll(rightFeeler.position, feelerRadius, pheromoneLayer);
        SetDirectionValues(ref leftHomePheros, ref leftFoodPheros, ref centerHomePheros,
        ref centerFoodPheros, ref rightHomePheros, ref rightFoodPheros, ref leftPlayerPheromone,
        ref centerPlayerPheromone, ref rightPlayerPheromone, ref leftObstacle, ref rightObstacle,
        ref centerObstacle, leftCollisions, centerCollisions, rightCollisions);

        if (hasFood)
        {
            rightDesirability = rightHomePheros;
            leftDesirability = leftHomePheros;
            centerDesirability = centerHomePheros;
            if (rightHomeDistance < leftHomeDistance)
            {
                rightDesirability += 3;
            }
            else
            {
                leftDesirability += 3;
            }
        }
        else if (!foodDetected)
        {
            rightDesirability = rightFoodPheros;
            leftDesirability = leftFoodPheros;
            centerDesirability = centerFoodPheros;
            rightDesirability += rightPlayerPheromone;
            centerDesirability += centerPlayerPheromone;
            leftDesirability += leftPlayerPheromone;
        }
        int totalDesirabilityRange = rightDesirability + centerDesirability + leftDesirability;

        for (int i = 0; i < totalDesirabilityRange; i++)
        {
            if (i % 10 == 0)
            {
                currentIgnorePheroFactor++;
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
        for (int i = 0; i < currentIgnorePheroFactor; i++){
            desirabilitySelector.Add(3);
        }
        if (desirabilitySelector.Count > 0)
        {
            ChooseDirection(desirabilitySelector);
        }
        AvoidObstacles(leftObstacle, rightObstacle, centerObstacle);
    }

    private void ChooseDirection(List<int> desirabilitySelector)
    {
        int directionToChoose = desirabilitySelector[Random.Range(0, desirabilitySelector.Count)];

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
            case 3:
                direction = (direction + Random.insideUnitCircle * randomnessForce).normalized;
                break;
            default:
                direction = (centerFeeler.transform.position - jaws.position).normalized;
                break;
        }
    }

    private static void SetDirectionValues(ref int leftHomePheros, ref int leftFoodPheros, ref int centerHomePheros, ref int centerFoodPheros, ref int rightHomePheros, ref int rightFoodPheros, ref int leftPlayerPheromone, ref int centerPlayerPheromone, ref int rightPlayerPheromone, ref int leftObstacle, ref int rightObstacle, ref int centerObstacle, Collider2D[] leftCollisions, Collider2D[] centerCollisions, Collider2D[] rightCollisions)
    {
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
    }

    void AttackPlayer()
    {
        Vector2 PlayerDirection = (player.position - jaws.position).normalized;
        float distance = Vector2.Distance(player.position, jaws.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerDirection, distance);
        if (hit != false && hit.collider.gameObject.tag == "Player" && playerLoseDistance > distance)
        {
            direction = (player.position - jaws.position).normalized;
        }
        else
        {
            player = null;
            foodDetected = false;
            targetFood = null;
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

    void FindTarget()
    {
        if (targetFood == null)
        {
            Collider2D[] detectedFood = Physics2D.OverlapCircleAll(currentPos, visionDistance, foodLayer);
            if (detectedFood.Length > 0)
            {

                float bestDistance = Vector2.Distance(detectedFood[0].transform.position, jaws.position);
                int bestIndex = 0;
                for (int i = 0; i < detectedFood.Length; i++)
                {
                    if (detectedFood[i].gameObject.tag == "Player")
                    {
                        player = detectedFood[i].transform;
                        ConvertPheromones();
                        ClearPheromoneList();
                        targetFood = player;
                        break;
                    }
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
                    Debug.Log(targetFood);
                    foodDetected = true;
                }
            }
        }
        else if (targetFood != null)
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
                ClearPheromoneList();
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
            if (player != null)
            {
                trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.black);
                trail.name = "To Player";
                lastPheromone = trail.transform;
            }
            else
            {
                trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.blue);
                trail.name = "To Home";
                lastPheromone = trail.transform;
                ownPheromones.Add(trail);
            }
        }

    }
    void ConvertPheromones()
    {
        foreach (GameObject pheromone in ownPheromones)
        {
            pheromone.name = "To Player";
        }
    }
    void ClearPheromoneList()
    {
        ownPheromones.Clear();
    }
}
