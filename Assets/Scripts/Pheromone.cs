using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PheromoneType{outPhero, backPhero};
public class Pheromone : MonoBehaviour
{
    
    PheromoneType pType;
    [SerializeField] float decayRate;
    float strength = 100;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float trailValue;

    private void Update() {
        strength -= (Time.deltaTime * decayRate);
        if (strength <= 0){
            Destroy(gameObject);
        }
    }
    public void SetupPheromone(PheromoneType type, Color color){
        pType = type;
        spriteRenderer.color = color;
    }
    void SetPheromoneValue(){

    }
    public float GetPheromoneValue(){
        return trailValue;
    }
}
