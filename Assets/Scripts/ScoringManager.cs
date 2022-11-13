using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoringManager : MonoBehaviour
{
    public List<int> playerScoreList = new List<int>();
    public int playerFragBonus = 10;
    public int playerFragPenalty = 10;
    public int NPCFragBonus = 1;

    public List<GameObject> scoreTextList;
    public List<GameObject> possessUIList;
    public GameObject gameOverText;
    public GameObject finalScoreText;
    // Start is called before the first frame update
    void Start()
    {
        int playerNrCount = FindObjectOfType<MainGameManager>().playerCount;
        for (int i = 0, playerN = 1; i < playerNrCount; ++i, ++playerN)
        {
            playerScoreList.Add(0);
            scoreTextList.Add(GameObject.Find("Player" + playerN + "Score"));
            possessUIList.Add(GameObject.Find("Player" + playerN + "possessUI"));
        }
        gameOverText = GameObject.Find("GameOverText");
        finalScoreText = GameObject.Find("FinalScoreText");
    }

    // Update is called once per frame
    void Update()
    {
        // scoring logic
        /* + 10 points for player kill
         * + 1 for every shot
         * shot respawns every 5 seconds 
         * it is better to find the player than to shoot dumbly
         * - 10 for killing yourself or more
         * 2 ghosts remain - either stop or shootoff
         * or we can stop at nr_players + 1
         * 
         * feedback to player when you kill (screenshake juices, +10 points appearing next to head)
         * 
         * teleportation to another guy
         * when the player reappears, visual cue and pad shake (particles on score or whatever)
         * 
         * RANDOM features:
         * reveal all players for 2 seconds
         * reveal for example 3 ghosts - player is one of them
         * ghosts change movement patterns
         * 
         */
        for (int i = 0; i < playerScoreList.Count; ++i)
        {
            scoreTextList[i].GetComponent<Text>().text = playerScoreList[i].ToString(); // TODO: nice juicy updating with animation etc screenshake twoja stara

        }
    }

    public void FragNPC(int toScoreIndex)
    {
        playerScoreList[toScoreIndex - 1] += NPCFragBonus;
        scoreTextList[toScoreIndex - 1].GetComponent<Mighty.TransformJuicer1>().StartJuicing();
    }

    public void FragPlayer(int toScoreIndex)//, int toDeductIndex)
    {
        playerScoreList[toScoreIndex - 1] += playerFragBonus;
        //playerScoreList[toDeductIndex] -= playerFragPenalty;
        scoreTextList[toScoreIndex - 1].GetComponent<Mighty.TransformJuicer1>().StartJuicing();

    }

    public void Kamikaze(int kamikazeIndex)
    {
        playerScoreList[kamikazeIndex - 1] -= playerFragPenalty;
        scoreTextList[kamikazeIndex - 1].GetComponent<Mighty.TransformJuicer1>().StartJuicing();

    }

    public void ResetScores()
    {
        for (int i = 0; i < playerScoreList.Count; ++i)
        {
            playerScoreList[i] = 0;
            SetPossessSkillFill(i, 1f);
        }
    }

    public void SetPossessSkillFill(int playerIndex, float fill)
    {
        possessUIList[playerIndex].GetComponent<RectTransform>().transform.localScale = new Vector3(fill, fill, fill); 
    }

    public void GameOver()
    {
        int winningIndex = playerScoreList.IndexOf(playerScoreList.Max()) + 1; // TODO: draw conditions?
        gameOverText.GetComponent<Text>().text = Utils.colorNames[winningIndex - 1] + " player wins!";
        string scoreString = "Score: \n";
        // sort biggest score first
        //List<Tuple<int, int>> sortedScores; // TODO: add sorting scores
        
        for (int i = 0; i < playerScoreList.Count; ++i)
        {
            scoreString += Utils.colorNames[i] + " : " + playerScoreList[i] + "\n";
        }

        finalScoreText.GetComponent<Text>().text = scoreString;
        Debug.Log(scoreString);
    }
}
