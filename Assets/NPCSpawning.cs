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
    public int LayerMask = 1 << 6;
    private int currentSpawnCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        LayerMask = ~LayerMask; // Everything except the layer 6 (PlayingField)
        while(currentSpawnCount < SpawnCount)
        {
            Vector3 spawnPos = SpawnCenter.transform.position + Random.insideUnitSphere * SpawnRadius;
            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100, LayerMask))
            {
                Instantiate(NPCPrefab, hit.point, Quaternion.identity);
                currentSpawnCount++;
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
