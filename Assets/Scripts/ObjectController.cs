using System;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [SerializeField] private float speed;
    private GameObject player;
    private PlayerController playerController;
    private bool isGamePaused;
    
    void Start()
    {
        isGamePaused = false;

        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerController.OnGamePaused += PlayerController_HandleGamePaused;
    }

    private void PlayerController_HandleGamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        isGamePaused = e.IsGamePaused;
    }

    void Update()
    {
        if (!isGamePaused)
        {
            transform.Translate(Vector3.down * Time.deltaTime * speed);
        }
    }
}
