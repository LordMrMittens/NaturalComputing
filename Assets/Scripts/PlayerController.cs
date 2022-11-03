using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float rotationSpeed;
    float horzMovement;
    float vertMovement;
    
    [SerializeField] GameObject pheromone;
    [SerializeField] float pheromoneDistanceInterval;
    [SerializeField] Vector3 lastPheromonePosition;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float bulletForce;
    [SerializeField] float timeBetweenShots;
    float shotTimer;
    void Start()
    {
        LeavePheromone();
    }

    // Update is called once per frame
    void Update()
    {
        shotTimer += Time.deltaTime;
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");

        Vector2 movementDirection = new Vector2(horzMovement, vertMovement);
        float inputMagitude = Mathf.Clamp01(movementDirection.magnitude);
        movementDirection.Normalize();
        transform.Translate(movementDirection * movementSpeed * inputMagitude * Time.deltaTime, Space.World);
        if (movementDirection != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.Space) && timeBetweenShots < shotTimer)
        {
            Shoot();
            shotTimer =0;
        }
        if (Vector2.Distance(transform.position, lastPheromonePosition) > pheromoneDistanceInterval)
        {
           // LeavePheromone();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Food")
        {
            Destroy(other.gameObject);
            //make character move slower because is heavier
            //make character leave more pheromones
        }
    }
    void Shoot()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 bulletDirection = (mousePos - (Vector2)transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().AddForce(bulletDirection * bulletForce,ForceMode2D.Impulse);

    }
    void LeavePheromone()
    {
        GameObject trail = Instantiate(pheromone, transform.position, Quaternion.identity);

        trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.black);
        trail.name = "To Player";
        lastPheromonePosition = trail.transform.position;
        }

}

