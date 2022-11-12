using Mighty;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MainGameManager : MightyGameManager
{
    MightyGameBrain brain;
    NPCSpawning npcSpawning;

    [Header("Movement")]
    public bool useMouseAndKeyboardInput;   
    public bool useGamePadInput;
    [ShowIf("useGamePadInput")] public int controllerNumber;
    public float movementSpeed = 5.0f;

    // Players Selecting
    [Header("Players")]
    public int playerCount = 2;
    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> playerShootSelectionList = new List<GameObject>();
    Color[] colors = { Color.red, Color.blue, Color.green };
    [ReadOnly] public float cursorMagnitude;
    [ReadOnly] public bool cursorMoved;
    [ReadOnly] public bool cursorStartedMoving;
    private MightyTimer cursorDelayTimer;
    private Physics physics;

    void Start()
    {
        brain = MightyGameBrain.Instance;
        npcSpawning = GetComponent<NPCSpawning>();

        // Initiate init cursor state
        cursorMoved = false;
        cursorStartedMoving = false;
        Utils.ResetTimer(out cursorDelayTimer, "CursorDelayTimer", 0.1f, 0.1f); // TODO: tweak values?
    }

    void Update()
    {
        HandleInput();
        HandlePlayers();
    }

    void HandlePlayers()
    {
        HandlePlayerMovement();
        HandleShootSelection();
        HandlePlayerInput();
    }

    void HandlePlayerMovement()
    {
        foreach (GameObject player in playerList)
        {
            DebugExtension.DebugWireSphere(player.transform.position, colors[playerList.IndexOf(player)], 2f);
            controllerNumber = playerList.IndexOf(player)+1; // 1 offset as gamepads start from 1 not zero
            if (useMouseAndKeyboardInput)
            {
                Vector3 movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized * movementSpeed;
                DebugExtension.DebugArrow(player.transform.position, movementDirection, Color.green);
                float yVel = 0.0f;//rb.velocity.y;
                player.GetComponent<Rigidbody>().velocity = new Vector3(movementDirection.x, yVel, movementDirection.z);
            }

            if (useGamePadInput)
            {
                Vector3 movementDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Left Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Left Stick Vertical")) * movementSpeed;
                DebugExtension.DebugArrow(player.transform.position, movementDirection * 10, Color.green);

                Rigidbody rb = player.GetComponent<Rigidbody>();
                float yVel = rb.velocity.y;
                rb.velocity = new Vector3(movementDirection.x, yVel, movementDirection.z);
            }
        }
    }

    // Handle Snapping for each player
    void HandleShootSelection() {
        List<GameObject> newSelected = new List<GameObject>();
        for (int sel_id = 0; sel_id < playerShootSelectionList.Count; ++sel_id)
        {
            if (!playerShootSelectionList[sel_id])
            {
                SelectNewRandomNPC(sel_id);
            }

            DebugExtension.DebugPoint(playerShootSelectionList[sel_id].transform.position, colors[sel_id], 10f);

            controllerNumber = sel_id+1; // 1 offset as gamepads start from 1 not zero
            if (useMouseAndKeyboardInput)
            {
                Debug.LogError("Mouse and keyboard input not implemented yet!");
            }
            if (useGamePadInput)
            {
                Vector3 cursorDirection;
                // Check if OSX as OSX uses different axis names (for XBOX controller on Windows, right stick is axis 3 and 4, on OSX it's 2 and 3) 
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    cursorDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Triggers"), 0, -Input.GetAxis("Controller" + controllerNumber + " Right Stick Horizontal")).normalized;
                }
                else
                {
                    cursorDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Right Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Right Stick Vertical")).normalized;
                }

                //Vector3 cursorDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Left Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Left Stick Vertical")).normalized;

                float cursorMagnitude = cursorDirection.magnitude;
                //Debug.Log(cursorDirection);
                DebugExtension.DebugArrow(playerList[0].transform.position, cursorDirection * 100, colors[sel_id]);

                if (cursorMagnitude > 0.01f)
                {
                    // Project long box in direction of cursor
                    // TODO: cone works, but may be slow
                    //Collider[] closestNPCs = physics.ConeCastLayer(playerShootSelectionList[sel_id].transform.position, 25f, cursorDirection, 50f, 45f, LayerMask.GetMask("NPC"));
                    Collider[] closestNPCs = Physics.OverlapBox(playerShootSelectionList[sel_id].transform.position + cursorDirection * 25f, new Vector3(2.5f, 0, 25f), Quaternion.LookRotation(cursorDirection), LayerMask.GetMask("NPC"));

                    DebugExtension.DebugWireSphere(playerShootSelectionList[sel_id].transform.position + cursorDirection*25f, colors[sel_id], 5f);
                    GameObject closestSelection = playerShootSelectionList[sel_id];
                    float closestDistance = -1f;
                    foreach (Collider npc in closestNPCs)
                    {
                        if (playerShootSelectionList[sel_id].gameObject != npc.gameObject) // Do not select itself when collided
                        {
                            DebugExtension.DebugWireSphere(npc.transform.position, Color.yellow, 2f);
                            float distance = Vector3.Distance(playerShootSelectionList[sel_id].transform.position, npc.transform.position);
                            if (closestDistance == -1f || distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestSelection = npc.gameObject;
                            }
                        }
                    }
                    playerShootSelectionList[sel_id] = closestSelection;
                }
            }
        }
    }

    void HandlePlayerInput()
    {
        for (int i = 0; i < playerList.Count; ++i)
        {
            int controllerNr = i + 1;
            //if (Input.GetAxis("Controller" + i + " Triggers") != 0)
            if (Input.GetButtonDown("Controller" + controllerNr + " X"))
            {
                Debug.Log("Poof");
                if (!playerShootSelectionList[i])
                {
                    Debug.Log("Missing selection"); // TODO: corner case? ignore?
                    return;
                }
                // selected ghost goes poof 
                npcSpawning.NPCList.Remove(playerShootSelectionList[i]);
                playerShootSelectionList[i].AddComponent<NPCDying>();
                playerShootSelectionList[i] = null;
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetButtonDown("Escape"))
        {
            MightyAudioManager.Instance.PlaySound("UI_Button_Click");

            if (brain.currentGameStateName == "Playing")
                brain.TransitToNextGameState("Pause");

            if (brain.currentGameStateName == "Pause")
                brain.TransitToNextGameState("Playing");

            if (brain.currentGameStateName == "Options")
                brain.TransitToNextGameState("Pause");
        }

        if (Input.GetButtonDown("ControllerAny Start"))
        {
            MightyAudioManager.Instance.PlaySound("UI_Button_Click");

            if (brain.currentGameStateName == "Playing")
                brain.TransitToNextGameState("Pause");

            if (brain.currentGameStateName == "Pause")
                brain.TransitToNextGameState("Playing");
        }
    }

    public void SpawnLevel()
    {
        npcSpawning.Spawn();

    }
    
    public void ClearLevel()
    {

    }

    void SelectNewRandomNPC(int sel_id)
    {
        playerShootSelectionList[sel_id] = npcSpawning.NPCList[Random.Range(0, npcSpawning.NPCList.Count)];
    }
    // --- MightyGameBrain callbacks ---

    // This is called by MightyGameBrain on every game state enter (you decide to handle it or not)
    public override IEnumerator OnEnterGameState(string enteringGameState, string exitingGameState)
    {
        if (exitingGameState == "GameOver") // Transition panel when leaving GameOver state
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", false, true));

        if (enteringGameState == "Playing")
            SpawnLevel();

        yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel(enteringGameState + "Panel", true, true));
    }

    // This is called by MightyGameBrain on every game state exit (you decide to handle it or not)
    public override IEnumerator OnExitGameState(string exitingGameState, string enteringGameState)
    {
        if (exitingGameState == "GameOver") // Transition panel when leaving GameOver state
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", true, false));

        if (exitingGameState == "Playing")
            ClearLevel();

        yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel(exitingGameState + "Panel", false, true));
    }
}
