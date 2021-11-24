using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameContext ctx;
    public DialogueManager DManager;
    public string StartSceneName = "DialogueScene";
    public enum GameStateType {
        Dialogue,
        Battle,
        ClickThrough,
        MainMenu
    }
    public GameStateType CurrentGameState = GameStateType.Dialogue;
    public bool InPauseMenu = false;

    // Start is called before the first frame update
    void Start()
    {
        //instantiate new SubManagers

        //go to first scene
        SceneManager.LoadScene(StartSceneName);
        SaveGameContext("C:/Users/Patrick/Desktop/personal-site/test.txt");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseDownAll() {
        //if external party shuts it down
        //save state and quit entire application
    }

    public void CloseAlert(SubManager caller) {
        //submanagecaller is asking to pause itself
        //and move on to a different section
    }

    public void SaveGameContext(string filePath) {
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine("test");
        writer.Close();
        //string towrite = JsonUtility.ToJson(myObject);
    }

    public void LoadGameContext(string filePath) {
        StreamReader reader = new StreamReader(filePath);
        GameContext loadedctx = JsonUtility.FromJson<GameContext>(reader.ReadToEnd());
        ctx = loadedctx;
    }
}