using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NPCSpawning : MonoBehaviour
{

    MainGameManager mainGameManager;
    public GameObject spawnCenter;
    public GameObject NPCPrefab;
    [Range(0, 100)]
    public float spawnRadius = 10;
    public int spawnCount = 30;
    public float NPC_min_distance = 1f;
    public float spawnHeight = 0.1f;
    public List<GameObject> NPCList = new List<GameObject>();
    

    private int playingFieldMask;
    private int NPCMask;
    private int layerMask;
    public int currentspawnCount = 0;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        DebugExtension.DrawCircle(spawnCenter.transform.position, Vector3.up, Color.yellow, spawnRadius);
    }

    public void Spawn()
    {
        mainGameManager = GetComponent<MainGameManager>();
        int max_iterations = 10000; // TODO Might want to change this to a bigger value
        int iterations = 0;
        playingFieldMask = 1 << LayerMask.NameToLayer("PlayingField");
        NPCMask = 1 << LayerMask.NameToLayer("NPC");
        layerMask = (playingFieldMask ); // Only check for collisions with PlayingField
        while (currentspawnCount < spawnCount)
        {
            Vector3 spawnPos = spawnCenter.transform.position + Random.insideUnitSphere * spawnRadius;
            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100, layerMask))
            {
                if (!Physics.CheckSphere(hit.point, NPC_min_distance, NPCMask))
                {
                    
                    Vector3 npcPos = hit.point;
                    npcPos.y = spawnHeight;
                    GameObject npc = Instantiate(NPCPrefab, npcPos, Quaternion.identity);
                    npc.transform.eulerAngles = new Vector3(0, Random.Range(0, 300), 0);
                    npc.name = "NPC" + iterations;
                    NPCList.Add(npc);
                    currentspawnCount++;
                }
            }
            iterations++;
            if (iterations > max_iterations)
            {
                Debug.LogError("Max iterations reached before all NPCs were spawned");
                break;
            }
        }

        // Select Players
        List<int> idsToSelect = Enumerable.Range(0, NPCList.Count).ToList();
        List<int> takenIdxs = new List<int>();
        for (int i = 0; i < mainGameManager.playerCount; i++)
        {
            int randomIndex = -1;
            while (randomIndex == -1 || takenIdxs.Contains(randomIndex))
            {
                randomIndex = Random.Range(0, idsToSelect.Count);
            }
            GameObject player = NPCList[randomIndex];
            player.GetComponent<NPC>().isPosessed = true;
            mainGameManager.playerList.Add(player);
            takenIdxs.Add(randomIndex);
        }
        // Select Players to shoot
        for (int i = 0; i < mainGameManager.playerCount; i++)
        {
            int randomIndex = Random.Range(0, NPCList.Count);
            GameObject playerTarget = NPCList[randomIndex];
            mainGameManager.playerShootSelectionList.Add(playerTarget);

            // CORNER CASE: CANNOT USE HIGHLIGHT MULTISELECTION SINCE playerShootSelectionList is not full yet
            // Set our own colour, if repeated we will overwrite in the for loop
            playerTarget.transform.GetComponentInChildren<Outline>().OutlineColor = Utils.colors[i];
            //Check if previously selected NPC is the same as the current one (iterate down)
            for (int j = i-1; j >= 0; --j) 
            {
                if (mainGameManager.playerShootSelectionList[j] == playerTarget)
                {
                    playerTarget.transform.GetComponentInChildren<Outline>().OutlineColor = MainGameManager.multiSelecton;
                    break;
                }
            }
        }
    }

    public void Clear()
    {
        foreach (GameObject npc in NPCList)
        {
            Destroy(npc);
        }
        NPCList.Clear();
        currentspawnCount = 0;
        mainGameManager.playerList.Clear();
        mainGameManager.playerShootSelectionList.Clear();
    }
}
