using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntMovement : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float turnSpeed;
    [SerializeField] float randomTurnStrenght;
    [SerializeField] Transform Jaws;
    [SerializeField] Transform Butt;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime, Space.Self );
        Vector2 rotationTowards = Random.insideUnitCircle * randomTurnStrenght;
        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward,rotationTowards);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, turnSpeed * Time.deltaTime);

        // Vector2 movementDirection = new Vector2(horzMovement,vertMovement);
        // float inputMagitude = Mathf.Clamp01(movementDirection.magnitude);
        // transform.Translate(movementDirection * movementSpeed * inputMagitude * Time.deltaTime, Space.World);
        // if (movementDirection != Vector2.zero){
        //     Quaternion toRotation = Quaternion.LookRotation(Vector3.forward,movementDirection);
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        // }
    }
}
