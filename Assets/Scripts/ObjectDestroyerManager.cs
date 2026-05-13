using UnityEngine;
using static PlayerController;

public class ObjectDestroyerManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.activeInHierarchy) 
        {
            ObjectPoolManager.ReturnObjectToPool(other.gameObject);
        }
    }

    public void DestroyAllNonPlayerObjects()
    {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("Crystal");

        foreach (GameObject obj in objectsToDestroy)
        {
            ObjectPoolManager.ReturnObjectToPool(obj);
        }

        objectsToDestroy = GameObject.FindGameObjectsWithTag("Rock");

        foreach (GameObject obj in objectsToDestroy)
        {
            ObjectPoolManager.ReturnObjectToPool(obj);
        }

        objectsToDestroy = GameObject.FindGameObjectsWithTag("Wall");

        foreach (GameObject obj in objectsToDestroy)
        {
            ObjectPoolManager.ReturnObjectToPool(obj);
        }

        objectsToDestroy = GameObject.FindGameObjectsWithTag("Fuel");

        foreach (GameObject obj in objectsToDestroy)
        {
            ObjectPoolManager.ReturnObjectToPool(obj);
        }
    }
}
