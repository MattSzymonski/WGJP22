using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawning : MonoBehaviour
{

    public GameObject SpawnCenter;
    public GameObject NPCPrefab;
    [Range(0, 100)]
    public float SpawnRadius = 10;
    public int SpawnCount = 30;
    public float NPC_min_distance = 1f;

    private int playingFieldMask;
    private int NPCMask;
    private int layerMask;
    private int currentSpawnCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        int max_iterations = 10000;
        int iterations = 0;
        playingFieldMask = 1 << LayerMask.NameToLayer("PlayingField");
        NPCMask = 1 << LayerMask.NameToLayer("NPC");
        layerMask = (playingFieldMask ); // Only check for collisions with PlayingField
        Debug.Log(layerMask);
        while(currentSpawnCount < SpawnCount)
        {
            Vector3 spawnPos = SpawnCenter.transform.position + Random.insideUnitSphere * SpawnRadius;
            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100, layerMask))
            {
                if(!Physics.CheckSphere(hit.point, NPC_min_distance, NPCMask))
                {
                    
                    Vector3 npcPos = hit.point;
                    npcPos.y = 0.5f;
                    Instantiate(NPCPrefab, npcPos, Quaternion.identity);
                    currentSpawnCount++;
                }
            }
            iterations++;
            if(iterations > max_iterations)
            {
                Debug.LogError("Max iterations reached before all NPCs were spawned");
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(SpawnCenter.transform.position, SpawnRadius);
    }
}
