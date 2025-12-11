using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElementSpawner : NetworkBehaviour
{
    public static ElementSpawner Instance;

    [SerializeField] private List<ElementSpawnPoint> spawnPoints;
    [SerializeField] private List<NetworkObject> elementPrefabs;

    [SerializeField] private float spawnInterval = 8f;
    private float timer;
    private bool active = false;

    private void Awake()
    {
        Instance = this;
        timer = spawnInterval;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            enabled = false;

        active = true;
    }

    private void Update()
    {
        if (!active) return;
        
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnRandomElement();
            timer = spawnInterval;
        }
    }

    private void SpawnRandomElement()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        ElementSpawnPoint sp = spawnPoints[spawnIndex];

        if (sp.spawned)
            return;

        NetworkObject prefab = elementPrefabs[Random.Range(0, elementPrefabs.Count)];
        NetworkObject obj = Instantiate(prefab, sp.transform.position, sp.transform.rotation);
        obj.GetComponent<SpawnedElement>().Init(spawnIndex);

        obj.Spawn();

        sp.spawned = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void FreeSpawnPointServerRpc(int spawnIndex)
    {
        spawnPoints[spawnIndex].spawned = false;
    }
}