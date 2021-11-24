using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameContext
{
    //storage of game state variables prefixed in scripting language by %
    public Dictionary<string, object> data;
    public static Dictionary<string, object> datadefault = new Dictionary<string, object>() {
        {"money",0},
        {"name","NO_NAME"},
    };
    public static Dictionary<string, string> datatypes = new Dictionary<string, string>() {
        {"money","int"},
        {"name","string"},
    };

    public GameContext(Dictionary<string, object> datai) {
        data = datai;
    }

    public string GetTypeString(string key) {
        if (datatypes.ContainsKey(key)) {
            return datatypes[key];
        }
        else {
            Debug.Log("cannot find object with key " + key);
            return "";
        }
    }

    public object GetVal(string key) {
        if (data.ContainsKey(key)) {
            return data[key];
        }
        else if (datadefault.ContainsKey(key)) {
            return datadefault[key];
        }
        else {
            Debug.Log("cannot find object with key " + key + " in gamecontext");
            return null;
        }
    }
}
