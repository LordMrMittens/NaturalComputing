using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float minMoventSpeed = 2.5f;
    [SerializeField] float rotationSpeed;
    float horzMovement;
    float vertMovement;
    float currentspeed;
    [SerializeField] GameObject pheromone;
    [SerializeField] float pheromoneDistanceInterval;
    [SerializeField] Vector3 lastPheromonePosition;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float bulletForce;
    [SerializeField] float timeBetweenShots;
    int foodHeld=0;
    float shotTimer;
    void Start()
    {
        LeavePheromone();
        currentspeed = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {

        shotTimer += Time.deltaTime;
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");
        if (foodHeld > 1)
        {
            currentspeed = movementSpeed / ((float)foodHeld / 5);
            
        }
        if (currentspeed < minMoventSpeed)
        {
            currentspeed = minMoventSpeed;
        }
        Vector2 movementDirection = new Vector2(horzMovement, vertMovement);
        float inputMagitude = Mathf.Clamp01(movementDirection.magnitude);
        movementDirection.Normalize();
        transform.Translate(movementDirection * currentspeed * inputMagitude * Time.deltaTime, Space.World);
        // if (movementDirection != Vector2.zero)
        // {
        //      Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, movementDirection);
        //      transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        //   }
        if (Input.GetKeyDown(KeyCode.Mouse0) && timeBetweenShots < shotTimer)
        {
            Shoot();
            shotTimer = 0;
        }
        if (Vector2.Distance(transform.position, lastPheromonePosition) > pheromoneDistanceInterval)
        {
           LeavePheromone();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Food")
        {
            Destroy(other.gameObject);
            foodHeld++;
            GameManager.GM.UpdateFoodScore();
        }
    }
    void Shoot()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        Vector2 bulletDirection = (mousePos - (Vector2)transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = bulletDirection * bulletForce;

    }
    void LeavePheromone()
    {
        GameObject trail = Instantiate(pheromone, transform.position, Quaternion.identity);
        trail.GetComponent<Pheromone>().SetupPheromone(Color.magenta, 6, "To Player");
        lastPheromonePosition = trail.transform.position;
        }

    private void OnDestroy() {
        GameManager.GM.UpdateLivesScore();
        GameManager.GM.isPlayerAlive = false;
    }
}

