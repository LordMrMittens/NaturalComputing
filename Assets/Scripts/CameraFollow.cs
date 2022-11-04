using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform player;

    private void LateUpdate()
    {
        if (GameManager.GM.isPlayerAlive && player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, -10);
        }
        else
        {
            if (GameManager.GM.isPlayerAlive)
            {
                player = FindObjectOfType<PlayerController>().transform;
            }
        }
    }
}
