using System.Collections;
using UnityEngine;

public class ObjectSpawnerManager : MonoBehaviour
{
    private int[] lanes = { -4, -2, 0, 2, 4 };
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject fuelPrefab;
    [SerializeField] private GameObject crystalPrefab;
    [SerializeField] private float ySpawnPosition = 13f;
    [SerializeField] private float wallSpawnRate = 1.5f;
    [SerializeField] private float rockSpawnRate = 1.25f;
    [SerializeField] private float fuelSpawnRate = 5f;
    [SerializeField] private float crystalMinSpawnRate = 8f;
    [SerializeField] private float crystalMaxSpawnRate = 12f;
    [SerializeField] private GameObject player;
    private PlayerController playerController;
    private bool isCrystalSpawn;
    private bool isFuelSpawn;
    private bool isPaused;

    private void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.OnGamePaused += PlayerController_OnGamePaused;
    }

    private void PlayerController_OnGamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        SetPaused(e.IsGamePaused);
    }

    public void StartLevel()
    {
        isCrystalSpawn = false;
        isFuelSpawn = false;
        StartCoroutine(WallSpawnRoutine());
        StartCoroutine(RockSpawnRoutine());
        StartCoroutine(CrystalSpawnRoutine());
        StartCoroutine(FuelSpawnRoutine());
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    IEnumerator WaitForSecondsPaused(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isPaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }

    public void EndLevel()
    {
        StopAllCoroutines();
    }

    IEnumerator WallSpawnRoutine()
    {

        while (true)
        {
            int[] shuffledLanes = (int[])ArrayHelpers.Shuffle(lanes);

            ObjectPoolManager.SpawnObject(wallPrefab, new Vector2(shuffledLanes[0], ySpawnPosition), Quaternion.identity);
            ObjectPoolManager.SpawnObject(wallPrefab, new Vector2(shuffledLanes[1], ySpawnPosition), Quaternion.identity);
            if (UnityEngine.Random.Range(0, 7) == 0) ObjectPoolManager.SpawnObject(wallPrefab, new Vector2(shuffledLanes[3], ySpawnPosition), Quaternion.identity);

            if (isFuelSpawn)
            {
                isFuelSpawn = false;
                ObjectPoolManager.SpawnObject(fuelPrefab, new Vector2(shuffledLanes[2], ySpawnPosition), Quaternion.identity);
            }

            yield return WaitForSecondsPaused(wallSpawnRate);
        }
    }

    IEnumerator RockSpawnRoutine()
    {
        yield return WaitForSecondsPaused(rockSpawnRate);
        bool isDoubleSpawn = false;
        while (true)
        {
            GameObject prefabToSpawn = isCrystalSpawn ? crystalPrefab : rockPrefab;
            int[] shuffledLanes = (int[])ArrayHelpers.Shuffle(lanes);
            ObjectPoolManager.SpawnObject(prefabToSpawn, new Vector2(shuffledLanes[0], ySpawnPosition), Quaternion.identity);
            if (isCrystalSpawn) isCrystalSpawn = false;
            if (isDoubleSpawn) ObjectPoolManager.SpawnObject(rockPrefab, new Vector2(shuffledLanes[1], ySpawnPosition), Quaternion.identity);
            isDoubleSpawn = !isDoubleSpawn;
            yield return WaitForSecondsPaused(rockSpawnRate);
        }
    }

    IEnumerator CrystalSpawnRoutine()
    {
        while (true)
        {
            yield return WaitForSecondsPaused(Random.Range(crystalMinSpawnRate, crystalMaxSpawnRate));
            isCrystalSpawn = true;
        }
    }

    IEnumerator FuelSpawnRoutine()
    {
        while (true)
        {
            isFuelSpawn = true;
            yield return WaitForSecondsPaused(fuelSpawnRate);
        }
    }

    public void SetRockSpawnRate(float newSpawnRate)
    {
        rockSpawnRate = newSpawnRate;
    }

    public void SetWallSpawnRate(float newSpawnRate)
    {
        wallSpawnRate = newSpawnRate;
    }
}
