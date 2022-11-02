using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float rotationSpeed;
    float horzMovement;
    float vertMovement;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject pheromone;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horzMovement = Input.GetAxisRaw("Horizontal");
        vertMovement = Input.GetAxisRaw("Vertical");

        Vector2 movementDirection = new Vector2(horzMovement,vertMovement);
        float inputMagitude = Mathf.Clamp01(movementDirection.magnitude);
        movementDirection.Normalize();
        transform.Translate(movementDirection * movementSpeed * inputMagitude * Time.deltaTime, Space.World);
        if (movementDirection != Vector2.zero){
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward,movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        if(Input.GetKeyDown(KeyCode.Space)){
            Shoot();
        }
    }
private void OnTriggerEnter2D(Collider2D other) {
    if( other.gameObject.tag == "Food"){
        Destroy(other.gameObject);
        //make character move slower because is heavier
        //make character leave more pheromones
    }
}
void Shoot(){
 GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
}
void LeavePheromone(){
        GameObject trail = Instantiate(pheromone, transform.position, Quaternion.identity);

            trail.GetComponent<Pheromone>().SetupPheromone(PheromoneType.outPhero, Color.black);
            trail.name = "To Player";
            
        }

}

