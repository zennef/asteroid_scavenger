using System;
using UnityEngine;

public class AfterburnerSpawnerManager : MonoBehaviour
{
    [SerializeField] private GameObject afterburnerPrefab;
    [SerializeField] private GameObject afterburnerSpawn;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private GameObject player;
    private GameManager gameManagerScript;

    void Start()
    {
        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnLevelStarted += GameManagerScript_OnLevelStarted;
        gameManagerScript.OnLevelEnded += GameManagerScript_OnLevelEnded;
    }

    private void GameManagerScript_OnLevelEnded(object sender, EventArgs e)
    {
        // Stop spawning afterburners when the level ends.
        CancelInvoke(nameof(SpawnAfterburner));
    }

    private void GameManagerScript_OnLevelStarted(object sender, EventArgs e)
    {
        InvokeRepeating(nameof(SpawnAfterburner), 0f, 0.135f);
    }

    void SpawnAfterburner()
    {
        GameObject afterburner = ObjectPoolManager.SpawnObject(afterburnerPrefab, afterburnerSpawn.transform.position, Quaternion.identity);
        Color32 shipColor = player.GetComponent<SpriteRenderer>().color;
        afterburner.GetComponent<AfterburnerController>().SetColor(shipColor);
    }
}