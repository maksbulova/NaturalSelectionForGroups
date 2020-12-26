using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foodCreator : MonoBehaviour
{
    public float spawnTime;
    public GameObject foodPrefab;
    public Vector2 spawnFrom, spawnTo;
    public int startFood;

    public int rndMinEnergy, rndMaxEnergy;

    private void Start()
    {
        StartFood();

        StartCoroutine(Spawner());            
    }


    private void StartFood()
    {
        GameObject food;
        for (int i = 0; i < startFood; i++)
        {
            rndPos = new Vector3(Random.Range(spawnFrom.x, spawnTo.x), Random.Range(spawnFrom.y, spawnTo.y));
            food = Instantiate(foodPrefab, rndPos, Quaternion.identity);
            food.GetComponent<food>().foodEnergy = Random.Range(rndMinEnergy, rndMaxEnergy);
        }
    }

    Vector3 rndPos;
    IEnumerator Spawner()
    {
        GameObject food;
        while (true)
        {
            rndPos = new Vector3(Random.Range(spawnFrom.x, spawnTo.x), Random.Range(spawnFrom.y, spawnTo.y));
            food = Instantiate(foodPrefab, rndPos, Quaternion.identity);
            food.GetComponent<food>().foodEnergy = Random.Range(rndMinEnergy, rndMaxEnergy);

            yield return new WaitForSeconds(spawnTime);
        }
    }
}
