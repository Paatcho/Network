using Unity.Netcode;
using UnityEngine;

public class SpawnedElement : NetworkBehaviour
{
    private int _spawnPointIndex = -1;

    public void Init(int index)
    {
        _spawnPointIndex = index;
    }

    public void OnCollected()
    {
        if (!IsServer || _spawnPointIndex == -1) return;

        ElementSpawner.Instance.FreeSpawnPointServerRpc(_spawnPointIndex);
    }
}