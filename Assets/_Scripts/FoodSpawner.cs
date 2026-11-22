using UnityEngine;
using Unity.Netcode;
public class FoodSpawner : NetworkBehaviour
{
    public GameObject foodPrefab;
    public int foodCount = 20;
    public Vector2 spawnArea = new Vector2(8f, 4.5f);
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        for (int i = 0; i < foodCount; i++)
        {
            Vector3 position = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            Random.Range(-spawnArea.y, spawnArea.y),
            0f
            );
            GameObject food = Instantiate(
            foodPrefab, position, Quaternion.identity);
            food.GetComponent<NetworkObject>().Spawn();
        }
    }
}