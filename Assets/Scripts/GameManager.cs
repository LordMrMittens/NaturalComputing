using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] float timeBeweenRespawns;
    float respawnTimer;
    public bool isPlayerAlive;
    int foodCollected;
    int antsEliminated;
    int playerLives;
    public static GameManager GM;
    [SerializeField] TMP_Text foodText;
    [SerializeField] TMP_Text antsText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] GameObject GameOverScreen;
    bool isPaused = false;
    // Start is called before the first frame update

    void Awake()
    {
        GM = this;
    }
    void Start()
    {
        ResetGame();
        
    }

    private void SpawnPlayer()
    {
        GameObject playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        isPlayerAlive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            Pause();
        }
        if (!isPlayerAlive)
        {
            if (playerLives > 0)
            {
                respawnTimer += Time.deltaTime;
                if (respawnTimer > timeBeweenRespawns)
                {
                    SpawnPlayer();
                    respawnTimer = 0;
                }
            } else {
                GameOverScreen.SetActive(true);
            }
        }
    }
    public void UpdateFoodScore()
    {
        foodCollected++;
        foodText.text = $"Food: {foodCollected}";
    }
    public void UpdateAntsScore()
    {
        antsEliminated++;
        antsText.text = $"Ants Killed: {antsEliminated}";
    }
    public void UpdateLivesScore()
    {
        playerLives--;
        livesText.text = $"Lives: {playerLives}";
    }

    void ResetGame(){
        playerLives=3;
        antsEliminated=0;
        foodCollected=0;
        foodText.text = $"Food: {foodCollected}";
        antsText.text = $"Ants Killed: {antsEliminated}";
        livesText.text = $"Lives: {playerLives}";
        GameOverScreen.SetActive(false);
        Time.timeScale = 1;
        SpawnPlayer();
    }

    void Pause(){
        if(isPaused){
            Time.timeScale = 1;
        } else {
            Time.timeScale = 0;
        }
    }
}
