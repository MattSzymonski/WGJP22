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

    public float CameraAdjustementAngle;

    public List<GameObject> mapPrefabs;
    GameObject currentMap;

    public GameObject guys;

    [Header("Movement")]
    public bool useMouseAndKeyboardInput;   
    public bool useGamePadInput;
    [ShowIf("useGamePadInput")] public int controllerNumber;
    public float movementSpeed = 5.0f;
    public float rotationSpeed = 0.1f;

    // Players Selecting
    [Header("Players")]
    public int playerCount = 2;
    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> playerShootSelectionList = new List<GameObject>();
    public static Color multiSelecton = new Color(1,1,0,1);
    private Physics physics;
    [ReadOnly] public List<bool> cursorMovedList = new List<bool>();
    [ReadOnly] public List<bool> cursorStartedMovingList = new List<bool>();
    private List<MightyTimer> cursorDelayTimerList = new List<MightyTimer>();
    private List<MightyTimer> posessionTimerList = new List<MightyTimer>();

    public float possessionSkillCooldown = 5f;
    private ScoringManager scoringManager;

    [ReadOnly]
    public bool gameOver = false;
    public bool initialized = false;

    private List<Vector3> previousMovementDirectionList = new List<Vector3>();

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
            posessionTimerList.Add(Utils.InitializeTimer("PossessionDelayTimer" + i, possessionSkillCooldown, possessionSkillCooldown));
            previousMovementDirectionList.Add(new Vector3());
        }
        scoringManager.ResetScores();
    }

    void Update()
    {
        HandleInput();
        if (gameOver || !initialized)
            return;
        HandlePlayers();
        UpdateUI();
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
                if (!SelectNewPlayer(i))
                    break;
            }
            GameObject player = playerList[i];
            if (playerList.Count > 0)
            {
                DebugExtension.DebugWireSphere(player.transform.position, Utils.colors[playerList.IndexOf(player)], 2f);
            }
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
                // new approach
                Vector3 movementDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Left Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Left Stick Vertical"));

                if (movementDirection.magnitude < 0.04f) // TODO: fix zero vector errors
                {
                    movementDirection =  Vector3.zero; // previousMovementDirectionList[i];
                }
                else
                {
                    previousMovementDirectionList[i] = movementDirection;

                    movementDirection = Quaternion.AngleAxis(CameraAdjustementAngle, Vector3.up) * movementDirection;
                    Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                    player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                    player.transform.position += player.transform.forward * Time.deltaTime * movementSpeed;
                }
               
                DebugExtension.DebugArrow(player.transform.position, movementDirection * 10, Color.yellow);
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




            DebugExtension.DebugPoint(playerShootSelectionList[sel_id].transform.position, Utils.colors[sel_id], 10f);
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

                cursorDirection = Quaternion.AngleAxis(CameraAdjustementAngle, Vector3.up) * cursorDirection;


                float cursorMagnitude = cursorDirection.magnitude;
                //Debug.Log(cursorDirection);
                DebugExtension.DebugArrow(playerShootSelectionList[sel_id].transform.position, cursorDirection * 10, Utils.colors[sel_id]);

                if (cursorMagnitude > 0.01f)
                {
                    // Project long box in direction of cursor
                    // TODO: cone works, but may be slow
                    //Collider[] closestNPCs = physics.ConeCastLayer(playerShootSelectionList[sel_id].transform.position, 25f, cursorDirection, 50f, 45f, LayerMask.GetMask("NPC"));
                    Collider[] closestNPCs = Physics.OverlapBox(playerShootSelectionList[sel_id].transform.position + cursorDirection * 25f, new Vector3(2.5f, 0, 25f), Quaternion.LookRotation(cursorDirection), LayerMask.GetMask("NPC"));

                    DebugExtension.DebugWireSphere(playerShootSelectionList[sel_id].transform.position + cursorDirection*25f, Utils.colors[sel_id], 5f);
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

                        if (cursorStartedMovingList[sel_id] && cursorDelayTimerList[sel_id].finished) // Change selection to new
                        {
                            int playersOnGhost = HighlightMultiselection(sel_id); // including us
                            if (playersOnGhost > 1 && playersOnGhost < 3) // one more players lasts on this ghost (but less than 2)
                            {
                                for (int i = 0; i < playerCount; i++)
                                {
                                    if (playerShootSelectionList[i] != null && sel_id != i)
                                    {
                                        playerShootSelectionList[sel_id].transform.GetComponentInChildren<Outline>().OutlineColor = Utils.colors[i];
                                        break;
                                    }
                                }
                            }
                            else // Nobody lasts on this ghost
                            {
                                playerShootSelectionList[sel_id].transform.GetComponentInChildren<Outline>().OutlineColor = new Color(0, 0, 0, 0); // Clean outline

                            }

                            playerShootSelectionList[sel_id] = closestSelection; // CHANGING SELECTION TO CLOSEST NEW !!!!

                            if (HighlightMultiselection(sel_id) == 1) // if just us on this ghost, colour with our outline (otherwise leaves yellow)
                            {
                                closestSelection.transform.GetComponentInChildren<Outline>().OutlineColor = Utils.colors[sel_id]; // Add  outline
                            }


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
            if (Input.GetButtonDown("Controller" + controllerNr + " X") && posessionTimerList[i].finished) // InputManager "Positive Button" must be "joystick <nr> button <button nr>"
            {
                Debug.Log("Controller " + controllerNr + " Poof");

                if (!playerShootSelectionList[i])
                {
                    Debug.Log("Missing selection"); // TODO: corner case? ignore?
                    return;
                }

                Utils.ResetTimer(posessionTimerList[i]);

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

                // selected ghost goes poof !!
                {
                    // Remove outline
                    playerShootSelectionList[i].transform.GetComponentInChildren<Outline>().OutlineColor = new Color(0, 0, 0, 0);
       

                    // Animation
                    playerShootSelectionList[i].transform.GetChild(0).GetComponent<Animator>().SetTrigger("Explode");

                }

                npcSpawning.NPCList.Remove(playerShootSelectionList[i]);
                //playerShootSelectionList[i].AddComponent<NPCDying>();
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

    void SpawnMap()
    {
        GameObject mapToSpawn = mapPrefabs[Random.Range(0, mapPrefabs.Count)];
        GameObject map = GameObject.Instantiate(mapToSpawn, Vector3.zero, Quaternion.identity) as GameObject;
        currentMap = map;



    }

    void UpdateUI()
    {
        for (int i = 0; i < cursorDelayTimerList.Count; ++i) // TODO: add cursor movement delay
        {
            scoringManager.SetPossessSkillFill(i, Mathf.Clamp(posessionTimerList[i].currentTime / posessionTimerList[i].targetTime, 0f, 1f));
        }
    }

    public void SpawnLevel()
    {
        SpawnMap();
        npcSpawning.Spawn();
        gameOver = false;
        initialized = true;
    }
    
    public void ClearLevel()
    {
        Destroy(currentMap.gameObject);
        scoringManager.ResetScores();
        npcSpawning.Clear();
        initialized = false;
    }

    void SelectNewRandomNPC(int sel_id)
    {
        playerShootSelectionList[sel_id] = npcSpawning.NPCList[Random.Range(0, npcSpawning.NPCList.Count)];
        playerShootSelectionList[sel_id].transform.GetComponentInChildren<Outline>().OutlineColor = Utils.colors[sel_id]; // Add outline

        HighlightMultiselection(sel_id);
    }

    int HighlightMultiselection(int sel_id) // returns how many players on this ghost
    {
        int count = 1; // including ourself
        for (int i = 0; i < playerCount; i++)
        {
            if (playerShootSelectionList[i] != null)
            {
                if (playerShootSelectionList[i] == playerShootSelectionList[sel_id] && sel_id != i) // Same selected ghosys but not by the same player
                {
                    playerShootSelectionList[sel_id].transform.GetComponentInChildren<Outline>().OutlineColor = multiSelecton;
                    ++count;
                }
            }
        }

        return count;
    }

    bool SelectNewPlayer(int playerListIndex)
    {
        List<int> idsToSelect = Enumerable.Range(0, npcSpawning.NPCList.Count).ToList();
        for (int i = 0; i < playerCount; ++i)
        {
            // remove player ID from list if not dead (null)
            if (playerList[i])
            {
                if (playerListIndex == i)
                    playerList[i].GetComponent<NPC>().isPosessed = false;
                idsToSelect.Remove(npcSpawning.NPCList.IndexOf(playerList[i]));
            }
        }
        // TODO CHECK GAME END CONDITION IF NO MORE NPCS LEFT
        if (idsToSelect.Count == 0)
        {
            Debug.Log("No more NPCs left to select. Game over!");
            scoringManager.GameOver();
            brain.TransitToNextGameState("GameOver");
            gameOver = true;
            return false;
        }
        playerList[playerListIndex] = npcSpawning.NPCList[idsToSelect[Random.Range(0, idsToSelect.Count)]];
        playerList[playerListIndex].GetComponent<NPC>().isPosessed = true;
        return true;
    }
    // --- MightyGameBrain callbacks ---

    // This is called by MightyGameBrain on every game state enter (you decide to handle it or not)
    public override IEnumerator OnEnterGameState(string enteringGameState, string exitingGameState)
    {
        //if (exitingGameState == "GameOver") // Transition panel when leaving GameOver state
        //    yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", false, true)); // TODO hangs here

        //if (exitingGameState == "MainMenu") // Transition panel when leaving GameOver state

        if (enteringGameState == "MainMenu")
        {
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", true, true));
            guys?.SetActive(true);
            // Show guys
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", false, false));

            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("MainMenuPanel", true, false));
        }

        if (enteringGameState == "Playing")
        {
            SpawnLevel();
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel(enteringGameState + "Panel", true, false));
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", false, true));
        }

        if (enteringGameState == "GameOver")
        {
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("GameOverPanel", true, true));




            //yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", false, true));
            ClearLevel();
        }
    }

    // This is called by MightyGameBrain on every game state exit (you decide to handle it or not)
    public override IEnumerator OnExitGameState(string exitingGameState, string enteringGameState)
    {
        if (exitingGameState == "GameOver") // TO PLAYING
        {
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("GameOverPanel", false, false));

        }

        if (exitingGameState == "MainMenu") // TO PLAYING
        {
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", true, true));
            guys?.SetActive(false);
            // Hide guys
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("MainMenuPanel", false, true));
        }

        if (exitingGameState == "Playing")
        {
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("PlayingPanel", false, true));
        }
        //if (exitingGameState == "GameOver") // Transition panel when leaving GameOver state
        //   yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", true, false));

    }
}
