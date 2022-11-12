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

    void Start()
    {
        brain = MightyGameBrain.Instance;
        npcSpawning = GetComponent<NPCSpawning>();
    }

    void Update()
    {
        HandleInput();
        HandlePlayers();
    }

    void HandlePlayers()
    {
        foreach (GameObject player in npcSpawning.PlayerList)
        {
             if (useGamePadInput)
            {
                    Vector3 movementDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Left Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Left Stick Vertical")) * movementSpeed;
                    DebugExtension.DebugArrow(player.transform.position, movementDirection);
                    float yVel = 0.0f;//player.GetComponent<Rigidbody>().velocity.y;
                    player.GetComponent<Rigidbody>().velocity = new Vector3(movementDirection.x, yVel, movementDirection.z);
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
