using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubManager : MonoBehaviour
{
    private GameManager owner;
    //a string to determine completely how it starts, in addition to game context
    //for dialogue, this is the convo.json file path.
    private string startval;
    private bool isActive;

    public void Initiate(GameManager owneri, string val) {
        owner = owneri;
        startval = val;
        //now set up entire state
        isActive = false;
    }

    void Start() {
        //nothing should be here. only on initiate
    }

    void Update() {
        if (!isActive) {

        }
    }

    public void Pause() {
        isActive = true;
    }

    public void Unpause() {
        isActive = false;
    }

    public void TogglePause() {
        isActive = !isActive;
    }

    public void SelfInitiateClose() {
        //this is to be called by this own object on itself
        //first prepare the state to make it ok to quit
        //notifies the owner
        owner.CloseAlert(this);
    }

    public void OwnerClose() {
        //this is to be called by the owner after recieving alert
        //saves the state then makes it inactive
        SaveState();
        isActive = false;
    }

    public void SaveState() {
        
    }


}
