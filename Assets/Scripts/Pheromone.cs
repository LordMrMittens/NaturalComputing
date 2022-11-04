using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Pheromone : MonoBehaviour
{
    [SerializeField] float decayRate;
    float strength = 100;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float trailValue;
    

    private void Update() {
        strength -= (Time.deltaTime * decayRate);
        if (strength <= 0){
            Destroy(gameObject);
        }
        if (GameManager.GM.displayPheromones){
            spriteRenderer.enabled = true;
        } else {
            spriteRenderer.enabled = false;
        }
    }
    public void SetupPheromone(Color color, float decay, string pheromoneName){
        spriteRenderer.color = color;
        decayRate = decay;
        name = pheromoneName; 
    }
    
}
