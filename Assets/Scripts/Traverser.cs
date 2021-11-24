using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphData {
	[System.Serializable]
	public class TodeSerial
	{
		public string name;
		public string dl;
		public string[] keys;
		public string[] vals;
		public string[] outn;
		public string[] outdl;
	}

	[System.Serializable]
	public class TodeDataContainer
	{
		public TodeSerial[] data;
		public string[] preq;
		public string callfunction;
		public string[] outgraphs;
	}

	[System.Serializable]
	public class DialoguePresetSerial
	{
		public string presetname;
		public string charname;
		public string dlsound;
		public string dlspeed;
		public string font;
	}

	[System.Serializable]
	public class PresetCollectionSerial
	{
		public DialoguePresetSerial[] data;
	}

	public class DialoguePreset
	{
		public string charname;
		public string dlsound;
		public float dlspeed;
		public string font;
		public DialoguePreset(string charnamei, string dlsoundi, string dlspeedi, string fonti) {
			charname = charnamei;
			dlsound = dlsoundi;
			dlspeed = float.Parse(dlspeedi);
			font = fonti;
		}
	}

	public class PresetCollection
	{
		public Dictionary<string,DialoguePreset> data;
		public PresetCollection(string presetFilePath) {
			TextAsset jsonFile = Resources.Load<TextAsset>(presetFilePath);
			PresetCollectionSerial result = JsonUtility.FromJson<PresetCollectionSerial>(jsonFile.text);
			DialoguePresetSerial[] arr = result.data;
			data = new Dictionary<string,DialoguePreset>();
			foreach (DialoguePresetSerial x in arr) {
				data.Add(x.presetname, new DialoguePreset(x.charname, x.dlsound, x.dlspeed, x.font));
			}
		}
		public DialoguePreset Get(string name) {
			return data[name];
		}
	}

	public class TConverter
	{
		public static Dictionary<string, string> todedatatypes = new Dictionary<string, string>() {
			{"preset","string"},
			{"money","int"},
			{"adbackground","string"},
			{"preq","string[]"},
			{"charportraits","string[]"},
			{"preq","string[]"},
			{"effect","string[]"}
		};

		public static int[] ToIntArray(string s) {
			s = s.Substring(1);
			string s2 = s.Remove(s.Length-1).Replace(" ", string.Empty);
			s = s2;
			string[] a = s.Split(",").ToArray();
			return Array.ConvertAll(a, int.Parse);
		}

		public static float[] ToFloatArray(string s) {
			s = s.Substring(1);
			string s2 = s.Remove(s.Length-1).Replace(" ", string.Empty);
			s = s2;
			string[] a = s.Split(",").ToArray();
			return Array.ConvertAll(a, float.Parse);
		}

		public static string[] ToStrArray(string s) {
			List<string> arrList = new List<string>();
			bool insideQuote = false;
			char insideMarker = '\n';
			int insideStart = 0;
			int insideLen = 0;
			char[] sarr = s.ToCharArray();
			//note: for loop excludes opening and closing brackets
			for (int i = 1; i < s.Length-1; i++) {
				if (!insideQuote) {
					if (sarr[i] == '\'') {
						insideQuote = true;
						insideMarker = '\'';
						insideStart = i+1;
					}
					else if (sarr[i] == '"') {
						insideQuote = true;
						insideMarker = '"';
						insideStart = i+1;
					}
				} else {
					if (sarr[i] == '\'' && insideMarker == '\'') {
						arrList.Add(s.Substring(insideStart, insideLen));
						insideQuote = false;
						insideLen = 0;
					}
					else if (sarr[i] == '"' && insideMarker == '"') {
						arrList.Add(s.Substring(insideStart, insideLen));
						insideQuote = false;
						insideLen = 0;
					} else {
						insideLen += 1;	
					}
					
				}
			}
			return arrList.ToArray();
		}

		public static object Convert(string s, string typename) {
			object res;
			if (typename == "int") {
				res = int.Parse(s);
			}
			else if (typename == "int[]") {
				res = TConverter.ToIntArray(s);
			}
			else if (typename == "float") {
				res = float.Parse()
			}
			else if (typename == "float[]") {
				res = TConverter.ToFloatArray(s);
			}
			else if (typename == "string[]") {
				res = TConverter.ToStringArray(s);
			}
			return res;
		}
	}

	public class Tode
	{
		public string name;
		public string dl;
		public Dictionary<string, object> data;
		public Tode[] outn;
		public string[] outdl;

		public static Dictionary<string, object> datadefault = new Dictionary<string,object>();

		public Tode(string namei, string dli, Dictionary<string, object> datai, string[] outdli) {
			name = namei;
			dl = dli;
			data = datai;
			outdl = outdli;
		}

		//note: doesn't initialize outn
		public static Tode SerialToTode(TodeSerial t) {
			Dictionary<string, object> d = new Dictionary<string, object>();
			for (int i = 0; i < t.keys.Length; i++) {
				d.Add(t.keys[i], TConverter.Convert(t.vals[i],TConverter.todedatatypes[t.keys[i]]));
			}
			return new Tode(t.name, t.dl, d, t.outdl);
		}

		public object GetVal(string key) {
			if (key == "name") {
				return name;
			}
			else if (key == "dl") {
				return dl;
			}
			else if (key == "outn") {
				return outn;
			}
			else if (key == "outdl") {
				return outdl;
			}
			else if (data.ContainsKey(key)) {
				return data[key];
			}
			else {
				Debug.Log("bad");
				return null;
			}
		}

	}

	public class DialogueGraph
	{
		public Tode start;
		public Tode current;
		public string[] preq;
		public string callfunction;
		public string[] outgraphs;
		public Dictionary<string, object> CurrentData;
		public Dictionary<string, object> PreviousData;

		public DialogueGraph(string filepath) {
			TextAsset jsonFile = Resources.Load<TextAsset>(filepath);
			Debug.Log(jsonFile.text);
			TodeDataContainer todecontainer = JsonUtility.FromJson<TodeDataContainer>(jsonFile.text);
			preq = todecontainer.preq;
			callfunction = todecontainer.callfunction;
			outgraphs = todecontainer.outgraphs;
			TodeSerial[] todeserials = todecontainer.data;

			//make all TodeSerials into Todes
			Tode[] graph = new Tode[todeserials.Length];
			for (int i = 0; i < todeserials.Length; i++) {
				TodeSerial t = todeserials[i];
				graph[i] = Tode.SerialToTode(t);
				graph[i].outn = new Tode[t.outn.Length];
				if (t.name == "start") {
					start = graph[i];
				}
			}
			current = start;

			//make hashmap by name
			Dictionary<string,Tode> graphnames = new Dictionary<string,Tode>();
			foreach (Tode t in graph) {
				graphnames.Add(t.name, t);
			}

			//go through and populate the .outn fields with other Tode references rather than strings
			//todo make performance better
			for (int st = 0; st < todeserials.Length; st++) {
				for (int i = 0; i < todeserials[st].outn.Length; i++) {
					//graphnames[st.name] = graphnames[st.outn[i]];
					graph[st].outn[i] = graphnames[todeserials[st].outn[i]];
				}
			}
			
			//setting up vals/valsarr by copying them from start
	        CurrentData = new Dictionary<string,object>();

	        foreach(KeyValuePair<string,string> entry in start.data) {
	            CurrentData.Add(entry.Key, entry.Value);
	        }

	        PreviousData = new Dictionary<string,object>(CurrentData);
		}

		//evaluates the string prerequisites for advancing to a node
		public bool EvaluatePrerequisite(ref GameContext ctx, string[] preq) {
			if (preq.Length == 0) {
				return true;
			}
			if (preq[0] == "true") {
				return true;
			}
			else if (preq[0] == "false") {
				return false;
			}
			int i = 0;
			bool totalTrue = true;
			string ctxvar = "";
			string operation = "";
			string val = "";
			string combineType = "and";
			bool res = true;
			while (i < preq.Length) {
				if (preq[i] == "and") {
					i += 1;
					combineType = "and";
				}
				else if (preq[i] == "or") {
					i += 1;
					combineType = "or";
				}
				else {
					combineType = "";
				}
				ctxvar = preq[i];
				operation = preq[i+1];
				val = preq[i+2];
				string vartype = GameContext.GetTypeString(ctxvar);
				if (operation == "in") {
					//testing in an array, aleph
					if (vartype == "string[]") {
						res = false;
						string[] ctxvali = (string[])ctx.GetVal(ctxvar);
						string vali = (string)val;
						foreach (string k in ctxvali) {
							if (k == val) {
								res = true;
							}
						}
					}

				}
				else if (operation == "inventory") {
					//structure is X inventory Y => item with uid X has inventory Y or greater. aleph
				}
				else if (vartype == "int") {
					int vali = (int)val;
					int ctxvali = (int)ctx.GetVal(ctxvar);
					switch(operation) {
						case "=":
							res = ctxvali == vali;
							break;
						case ">":
							res = ctxvali > vali;
							break;
						case "<":
							res = ctxvali < vali;
							break;
						case ">=":
							res = ctxvali >= vali;
							break;
						case "<=":
							res = ctxvali <= vali;
							break;
						default:
							res = ctxvali == vali;
							break;
					}
				}
				else if (vartype == "float") {
					float valf = (float)val;
					float ctxvalf = (float)ctx.GetVal(ctxvar);
					switch(operation) {
						case "=":
							res = ctxvalf == valf;
							break;
						case ">":
							res = ctxvalf > valf;
							break;
						case "<":
							res = ctxvalf < valf;
							break;
						case ">=":
							res = ctxvalf >= valf;
							break;
						case "<=":
							res = ctxvalf <= valf;
							break;
						default:
							res = ctxvalf == valf;
							break;
					}
				}
				else if (vartype == "string") {
					res = (string)val == (string)ctx.GetVal(ctxvar);
				}
				else if (vartype == "int[]") {
					res = (int[])val == (int[])ctx.GetVal(ctxvar);
				}
				else if (vartype == "float[]") {
					res = (float[])val == (float[])ctx.GetVal(ctxvar);
				}				
				else if (vartype == "string[]") {
					res = (string[])val == (float[])ctx.GetVal(ctxvar);
				}

				
				i += 3;
				if (combineType == "") {
					totalTrue = res;
				}
				else if (combineType == "and") {
					totalTrue = totalTrue && res;
				}
				else if (combineType == "or") {
					totalTrue = totalTrue || res;
				}
			}
			return totalTrue;
		}

		public void EvaluateEffect(ref GameContext ctx, string[] effect) {
			if (effect.Length == 0) {
				return;
			}
			//must be of the form [var,operator,value,var,...]
			//length must be divisible by 3
			else if (effect.Length % 3 == 0) {
				for (int i = 0; i < effect.Length; i += 3) {
					string ctxvar = effect[i];
					string operation = effect[i+1];
					string val = effect[i+2];
					char fr = val.ToCharArray()[0];
					if (Char.IsDigit(fr)) {
						//set float with float
						if (Char.IsDigit(ctx.data[ctxvar].ToCharArray()[0])) {
							switch (operation) {
								case "+":
									ctx.data[ctxvar] = (float.Parse(ctx.data[ctxvar]) + float.Parse(val)).ToString();
									break;
								case "-":
									ctx.data[ctxvar] = (float.Parse(ctx.data[ctxvar]) - float.Parse(val)).ToString();
									break;
								case "*":
									ctx.data[ctxvar] = (float.Parse(ctx.data[ctxvar]) * float.Parse(val)).ToString();
									break;
								case "/":
									ctx.data[ctxvar] = (float.Parse(ctx.data[ctxvar]) / float.Parse(val)).ToString();
									break;
								case "=":
									ctx.data[ctxvar] = val;
									break;
								default:
									ctx.data[ctxvar] = val;
									break;
							}
						} else {
							ctx.data[ctxvar] = val;
						}
						
					}
					else if (val == "true" || val == "false") {
						//set bool
						ctx.data[ctxvar] = val;
					}
					else {
						//set string
						ctx.data[ctxvar] = val;
					}
				}
			}
		}

		//proceeds to the next node by evaluating prerequisites without
		//addressing user option choice
		//returns whether this is possible
		public bool Next(ref GameContext ctx) {
			foreach (Tode ot in current.outn) {
				if (ot.data.ContainsKey("preq")) {
					if (EvaluatePrerequisite(ref ctx, (string[])ot.data["preq"])) {
						current = ot;
						UpdateDialogueVals();
						if (current.data.ContainsKey("effect")) {
							EvaluateEffect(ref ctx, (string[])current.data["effect"]);
						}
						return true;
					}
				}
				//aleph why is this here?
				/*else if (CurrentData.ContainsKey("preq")) {
					if (EvaluatePrerequisite(ref ctx, CurrentData["preq"])) {
						current = ot;
						UpdateDialogueVals();
						if (current.data.ContainsKey("effect")) {
							EvaluateEffect(ref ctx, (string[])current.data["effect"]);
						}
						return true;
					}
				}*/
			}
			return false;
		}

		//NOTE: which is 1,2,3,...
		public bool ChooseOption(ref GameContext ctx, int which) {
			Debug.Log("choosing option " + which);
			Debug.Log(current.outn.Length);
			if (current.outn[which].data.ContainsKey("preq")) {
				if (EvaluatePrerequisite(ref ctx, (string[])current.outn[which].data["preq"]) {
					current = current.outn[which];
					if (current.data.ContainsKey("effect")) {
						EvaluateEffect(ref ctx, (string[])current.data["effect"]);
					}
					UpdateDialogueVals();
					return true;
				}
				return false;
			}
			//aleph why is this here?
			/*else if (CurrentData.ContainsKey("preq")) {
				if (EvaluatePrerequisite(ref ctx, (string[])CurrentData["preq"])) {
					current = current.outn[which];
					if (current.data.ContainsKey("effect")) {
						EvaluateEffect(ref ctx, (string[])current.data["effect"]);
					}
					UpdateDialogueVals();
					return true;
				}
				return false;
			}*/
			return false;
		}

	    void UpdateDialogueVals() {
	        //updates CurrentData by overwriting entries where relevant
	        //but first, copy currentdata to previousvals
	        PreviousData = new Dictionary<string,object>(CurrentData);
	        foreach(KeyValuePair<string, object> entry in current.data) {
	            if (CurrentData.ContainsKey(entry.Key)) {
	                CurrentData[entry.Key] = entry.Value;
	            } else {
	                CurrentData.Add(entry.Key, entry.Value);
	            }
	        }
	        Debug.Log("updated dialogue data");
	    }
	    //gets the current value of anything in the dialogue graphe
	    //it normally asks DGraph.current for it, but if that fails it looks to the previous
	    //node in the graph, that is DGraph
	    public object GetCurrent(string key) {
	        if (CurrentData.ContainsKey(key)) {
	            return CurrentData[key];
	        } else if (Tode.datadefault.ContainsKey(key)) {
	            return Tode.datadefault[key];
	        } else {
	        	return null;
	        }
	    }

	    public string GetPrevious(string key) {
	        if (PreviousData.ContainsKey(key)) {
	            return PreviousVals[key];
	        } else if (Tode.datadefault.ContainsKey(key)) {
	            return Tode.datadefault[key];
	        } else {
	        	return null;
	        }
	    }
	}
}