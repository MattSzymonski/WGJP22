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
    [ReadOnly] public float cursorMagnitude;
    [ReadOnly] public bool cursorMoved;
    [ReadOnly] public bool cursorStartedMoving;
    private MightyTimer cursorDelayTimer;
    void Start()
    {
        brain = MightyGameBrain.Instance;
        npcSpawning = GetComponent<NPCSpawning>();

        // Initiate init cursor state
        cursorMoved = false;
        cursorStartedMoving = false;
        var timeManager = MightyTimersManager.Instance;
        cursorDelayTimer = timeManager.CreateTimer("CursorDelayTimer", 0.005f, 2.0f, false, true); // Create new timer (Not looping, stopped on start)
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
    }

    void HandlePlayerMovement()
    {
        foreach (GameObject player in playerList)
        {
            DebugExtension.DebugWireSphere(player.transform.position, Color.red, 2f);
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
                DebugExtension.DebugArrow(player.transform.position, movementDirection, Color.green);

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
            DebugExtension.DebugWireSphere(playerShootSelectionList[sel_id].transform.position, Color.blue, 2f);

            controllerNumber = sel_id+1; // 1 offset as gamepads start from 1 not zero
            if (useMouseAndKeyboardInput)
            {
                Debug.LogError("Mouse and keyboard input not implemented yet!");
            }
            if (useGamePadInput)
            {
                Vector3 cursorDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Right Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Right Stick Vertical"));
                float cursorMagnitude = cursorDirection.magnitude;
                Debug.Log(cursorDirection);
                DebugExtension.DebugArrow(playerShootSelectionList[sel_id].transform.position, cursorDirection, Color.red);

                if (cursorMagnitude > 0.01f)
                {
                    // Project long box in direction of cursor
                    Collider[] closestNPCs = Physics.OverlapBox(playerShootSelectionList[sel_id].transform.position + new Vector3(0, 0, 25), new Vector3(5, 0, 50), Quaternion.LookRotation(cursorDirection), LayerMask.GetMask("NPC"));
                    GameObject closestSelection = playerShootSelectionList[sel_id];
                    float closestDistance = -1f;
                    foreach (Collider npc in closestNPCs)
                    {
                        if (gameObject != npc.gameObject) // Do not select itself when collided
                        {
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

    // --- MightyGameBrain callbacks ---

    // This is called by MightyGameBrain on every game state enter (you decide to handle it or not)
    public override IEnumerator OnEnterGameState(string enteringGameState, string exitingGameState)
    {
        if (exitingGameState == "GameOver") // Transition panel when leaving GameOver state
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", false, true));

        yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel(enteringGameState + "Panel", true, true));
    }

    // This is called by MightyGameBrain on every game state exit (you decide to handle it or not)
    public override IEnumerator OnExitGameState(string exitingGameState, string enteringGameState)
    {
        if (exitingGameState == "GameOver") // Transition panel when leaving GameOver state
            yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel("TransitionPanel", true, false));

        yield return StartCoroutine(MightyUIManager.Instance.ToggleUIPanel(exitingGameState + "Panel", false, true));
    }
}
