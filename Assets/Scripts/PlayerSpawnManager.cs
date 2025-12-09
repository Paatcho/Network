using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    
    [SerializeField] private List<Transform> spawnPositions;

    private int _spawnIndex = -1;

    private void Awake()
    {
        networkManager.ConnectionApprovalCallback += ConnectionApproval;
    }
    
    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Using connection approval to set player spawn pos.
        print("wfqaipjo");
        response.CreatePlayerObject = true;
        response.Position = GetNextSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.Approved = true;
    }

    private Vector3 GetNextSpawnPosition()
    {
        _spawnIndex = (_spawnIndex + 1) % spawnPositions.Count;
        return spawnPositions[_spawnIndex].position;
    }
}
