using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LapManager : MonoBehaviour
{
    
    public List<Checkpoint> checkpoints;
    public int totalLaps = 3;
    public UIManager ui;
    public GameManager gameManager;

    private List<PlayerRank> playerRanks = new List<PlayerRank>();
    private PlayerRank mainPlayerRank;
    public UnityEvent onPlayerFinished = new UnityEvent();

    // pause + finish
    private IEnumerator FinishDelay()
    {
        yield return new WaitForSeconds(7); // check this in class
       
           SceneManager.LoadScene(0);

        //onPlayerFinished.Invoke();
    }


    void Start()
    {
        // Get players in the scene
        foreach (CarIdentity carIdentity in GameObject.FindObjectsOfType<CarIdentity>())
        {
            playerRanks.Add(new PlayerRank(carIdentity)); // NO RECOGE LAS IDENTIDADES
        }

        ListenCheckpoints(true);

        //string m = "LAPS " ;
        //ui.UpdateLapText(m);
        // Debug.Log(playerRanks[0].lapNumber); expulsa el 1
        ui.UpdateLapText("Lap " + playerRanks[0].lapNumber + " / " + totalLaps);
         mainPlayerRank = playerRanks.Find(player => player.identity.gameObject.tag == "Player");
    }

    private void ListenCheckpoints(bool subscribe)
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            //TODO : refacctor onChekpointEnter event
            
            if (subscribe) checkpoint.onCheckpointEnter.AddListener(CheckpointActivated);
            else checkpoint.onCheckpointEnter.RemoveListener(CheckpointActivated);

        }
    }

    public void CheckpointActivated(CarIdentity car, Checkpoint checkpoint)
    {
        PlayerRank player = playerRanks.Find((rank) => rank.identity == car);
        if (checkpoints.Contains(checkpoint) && player != null)
        {
            // if player has already finished don't do anything
            if (player.hasFinished) {
                //onPlayerFinished.Invoke(); // call on PlayerFinished
                //StartCoroutine(FinishDelay());
                return;
            } 

            int checkpointNumber = checkpoints.IndexOf(checkpoint);
            // first time ever the car reach the first checkpoint
            bool startingFirstLap = checkpointNumber == 0 && player.lastCheckpoint == -1;
            // finish line checkpoint is triggered & last checkpoint was reached
            bool lapIsFinished = checkpointNumber == 0 && player.lastCheckpoint >= checkpoints.Count - 1;
            if (startingFirstLap || lapIsFinished)
            {
                player.lapNumber += 1;
                player.lastCheckpoint = 0;

                // if this was the final lap
                if (player.lapNumber > totalLaps)
                {
                    player.hasFinished = true;
                    // getting final rank, by finding number of finished players
                    player.rank = playerRanks.FindAll(player => player.hasFinished).Count;

                    // if first winner, display its name
                    if (player.rank == 1)
                    {
                        // TODO : create attribute divername in CarIdentity 
                        Debug.Log(player.identity.driverName + " won");
                        ui.UpdateLapText(player.identity.driverName + " won");
                    }
                    else if (player == mainPlayerRank) // display player rank if not winner
                    {
                        ui.UpdateLapText("You finished in " + mainPlayerRank.rank + " place");
                    }

                    if (player == mainPlayerRank) StartCoroutine(FinishDelay());
                    //onPlayerFinished.Invoke();
                }
                else
                {
                    // TODO : create attribute divername in CarIdentity 
                    Debug.Log(player.identity.driverName + ": lap " + player.lapNumber);
                    if (car.gameObject.tag == "Player") ui.UpdateLapText("Lap " + player.lapNumber + " / " + totalLaps);
                }
            }
            // next checkpoint reached
            else if (checkpointNumber == player.lastCheckpoint + 1)
            {
                player.lastCheckpoint += 1;
            }
        }
    }
}

