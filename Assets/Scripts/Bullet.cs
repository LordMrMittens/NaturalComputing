using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision");
        if (other.gameObject.tag == "Ant")
        {
            Destroy(other.gameObject);

        }
        if (other.gameObject.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
    }
}
