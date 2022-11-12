using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoringManager : MonoBehaviour
{
    public List<int> playerScoreList = new List<int>();
    public int playerFragBonus = 10;
    public int playerFragPenalty = 10;
    public int NPCFragBonus = 1;

    public List<GameObject> scoreTextList;
    public List<GameObject> killUIList;
    // Start is called before the first frame update
    void Start()
    {
        int playerNrCount = FindObjectOfType<MainGameManager>().playerCount;
        for (int i = 0, playerN = 1; i < playerNrCount; ++i, ++playerN)
        {
            playerScoreList.Add(0);
            scoreTextList.Add(GameObject.Find("Player" + playerN + "Score"));
            //killUIList.Add(GameObject.Find("Player" + playerN + "killUI"));
        }
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
    }

    public void FragPlayer(int toScoreIndex)//, int toDeductIndex)
    {
        playerScoreList[toScoreIndex - 1] += playerFragBonus;
        //playerScoreList[toDeductIndex] -= playerFragPenalty;
    }

    public void Kamikaze(int kamikazeIndex)
    {
        playerScoreList[kamikazeIndex - 1] -= playerFragPenalty;
    }

    public void ResetScores()
    {
        for (int i = 0; i < playerScoreList.Count; ++i)
            playerScoreList[i] = 0;
    }
}
