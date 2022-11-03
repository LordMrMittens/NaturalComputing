using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntHill : MonoBehaviour
{
    [SerializeField] GameObject antPrefab;
    [SerializeField] int antsInHill;
    public int antsOut {get;set;}

    [SerializeField] int InitialRelease;
    [SerializeField] float timeBetweenAnts;
    float antTimer;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < InitialRelease; i++)
        {
            CreateAnt();
        }
    }

    private void CreateAnt()
    {
        GameObject ant = Instantiate(antPrefab, transform.position, Quaternion.identity);
        Ant antSettings = ant.GetComponent<Ant>();
        antSettings.SetUpAnt(transform.position);
        antSettings.homeAnthill = this;
        antsOut++;
    }

    // Update is called once per frame
    void Update()
    {
        if (antsInHill>antsOut){
            antTimer+= Time.deltaTime;
            if(antTimer>timeBetweenAnts){
                CreateAnt();
                antTimer = 0;
            }
        }
    }
}
