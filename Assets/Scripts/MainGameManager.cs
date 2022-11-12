using Mighty;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;

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
    private Physics physics;
    [ReadOnly] public List<bool> cursorMovedList = new List<bool>();
    [ReadOnly] public List<bool> cursorStartedMovingList = new List<bool>();
    private List<MightyTimer> cursorDelayTimerList = new List<MightyTimer>();
    //private List<MightyTimer> triggerTimerList = new List<MightyTimer>();
    private ScoringManager scoringManager;

    void Start()
    {
        brain = MightyGameBrain.Instance;
        npcSpawning = GetComponent<NPCSpawning>();
        scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();

        // Initiate init cursor state
        cursorMovedList = Enumerable.Repeat(false, playerCount).ToList();
        cursorStartedMovingList = Enumerable.Repeat(false, playerCount).ToList();
        for (int i = 0; i < playerCount; i++)
        {
            cursorDelayTimerList.Add(Utils.InitializeTimer("CursorDelayTimer" + i, 0.05f, 0.05f));
            //triggerTimerList.Add(Utils.InitializeTimer("TriggerTimerList" + i, 0.2f, 0.2f));
        }
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
        for (int i = 0; i < playerCount; i++)
        {
            // CHECK IF PLAYER WAS KILLED
            if (!playerList[i])
            {
                SelectNewPlayer(i);
            }
            GameObject player = playerList[i];
            DebugExtension.DebugWireSphere(player.transform.position, colors[playerList.IndexOf(player)], 2f);
            controllerNumber = i+1; // 1 offset as gamepads start from 1 not zero
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
    void HandleShootSelection() 
    {
        List<GameObject> newSelected = new List<GameObject>();
        for (int sel_id = 0; sel_id < playerShootSelectionList.Count; ++sel_id)
        {
            // CHECK IF selection was killed
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

                float cursorMagnitude = cursorDirection.magnitude;
                //Debug.Log(cursorDirection);
                DebugExtension.DebugArrow(playerShootSelectionList[sel_id].transform.position, cursorDirection * 10, colors[sel_id]);

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
                    if (!cursorMovedList[sel_id])
                    {
                        if (!cursorStartedMovingList[sel_id])
                        {
                            cursorDelayTimerList[sel_id].RestartTimer();
                            cursorDelayTimerList[sel_id].PlayTimer();
                            cursorStartedMovingList[sel_id] = true;
                        }

                        if (cursorStartedMovingList[sel_id] && cursorDelayTimerList[sel_id].finished)
                        {
                            playerShootSelectionList[sel_id] = closestSelection; // CHANGING SELECTION TO CLOSEST NEW !!!!
                            Debug.Log("moving cursor");

                            cursorMovedList[sel_id] = true;
                        }
                        else
                        {
                            cursorMovedList[sel_id] = false;
                        }
                    } 
                }
                if (cursorStartedMovingList[sel_id] && cursorMagnitude == 0)
                {
                    cursorDelayTimerList[sel_id].StopTimer();
                    cursorDelayTimerList[sel_id].RestartTimer();
                    cursorStartedMovingList[sel_id] = false;
                    cursorMovedList[sel_id] = false;
                }
            }
        }
    }

    void HandlePlayerInput()
    {
        for (int i = 0; i < playerList.Count; ++i)
        {
            int controllerNr = i + 1;
            //if (Input.GetAxis("Controller" + controllerNr + " Triggers") != 0 && triggerTimerList[i].finished) // TODO: TRIGGERS NOT WORKING
            if (Input.GetButtonDown("Controller" + controllerNr + " X")) // InputManager "Positive Button" must be "joystick <nr> button <button nr>"
            {
                Debug.Log("Controller " + controllerNr + " Poof");
                if (!playerShootSelectionList[i])
                {
                    Debug.Log("Missing selection"); // TODO: corner case? ignore?
                    return;
                }
                bool killedSomething = false;
                int playerKillingID = i + 1;
                // FOR EACH PLAYER: check if removing a player from the list
                for (int j = 0; j < playerCount; ++j)
                {
                    if (playerList[j] == playerShootSelectionList[i])
                    {
                        int playerKilledID = j + 1;
                        if (playerKilledID == playerKillingID)
                        {
                            killedSomething = true;
                            Debug.Log("Player Killed himself! -X points for player " + playerKilledID);
                            scoringManager.Kamikaze(playerKilledID);
                        }
                        else
                        {
                            killedSomething = true;
                            Debug.Log("Player " + playerKilledID + " killed by " + playerKillingID + " +X points for player " + playerKillingID);
                            scoringManager.FragPlayer(playerKillingID);
                        }

                        playerList[j] = null;
                    }
                }
                if (!killedSomething)
                {
                    killedSomething = true;
                    Debug.Log("NPC Killed by " + playerKillingID + " +X points for player " + playerKillingID);
                    scoringManager.FragNPC(playerKillingID);
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
        scoringManager.ResetScores();
    }

    void SelectNewRandomNPC(int sel_id)
    {
        playerShootSelectionList[sel_id] = npcSpawning.NPCList[Random.Range(0, npcSpawning.NPCList.Count)];
    }

    void SelectNewPlayer(int playerListIndex)
    {
        List<int> idsToSelect = Enumerable.Range(0, npcSpawning.NPCList.Count).ToList();
        for (int i = 0; i < playerCount; ++i)
        {
            // remove player ID from list if not dead (null)
            if (playerList[i])
            {
                idsToSelect.Remove(npcSpawning.NPCList.IndexOf(playerList[i]));
            }
        }
        // TODO CHECK GAME END CONDITION IF NO MORE NPCS LEFT
        if (idsToSelect.Count == 0)
        {
            Debug.Log("No more NPCs left to select. Game over!");
            // TODO CALL STOP GAME!
            return;
        }
        playerList[playerListIndex] = npcSpawning.NPCList[idsToSelect[Random.Range(0, idsToSelect.Count)]];
        playerList[playerListIndex].GetComponent<NPC>().isPosessed = true;
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
