using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraphData;

public class DialogueManager : MonoBehaviour
{
	public TMP_Text DLMain;
    public TMP_Text DLCharName;
    public TMP_Text DLBacklogText;
    public TMP_Text DLChooseText;
    public Image DLTextbox;

    public Image DLBacklogBackground;
    public int DLBacklogMaxChars = 70;
    public int DLBacklogMaxDisplayLines = 20;
    public int DLBacklogMaxTotalLines = 100;
    public float DLBacklogScrollTimeMax = 0.1f;
    private float DLBacklogScrollTime = 0;

    public Image DLChooseBackground;
    //pixels of background space for each character/line in the dialogue choose menu
    public int ChooseBackgroundMaxPixPerCharacter = 30;
    public int ChooseBackgroundMaxPixPerLine = 60;

    private enum DLState {
        Dialogue, //in normal dialogue
        Backlog, //looking at backlog
        Choose, //choosing dialogue options
        FadeOut,
        FadeIn,
        Transition //moving to new node
    }
    private DLState CurrentState = DLState.Transition;

    public RawImage DLBackground;
    public AudioSource DLAudioTrack1;
    public AudioSource DLAudioMusic;
    public AudioSource DLAudioBackground;
    public RawImage DLChar1;
    public RawImage DLChar2;
    public RawImage DLChar3;

    //characters currently in
    private float DLProgress = 0f;
    private int DLProgressChar = 0;
    //seconds between characters
    private float DLSpeed = 1f;
    private string DLCurrent = "";
    private int DLLength = 0;
    private int DLNumOptions = 0;
    private float DLPause = 0;
    private float DLPauseAmount = 0.3f;
    private bool DLIsTypewriting = false;
    //true if any text, partial or complete, is being displayed
    private bool DLIsSet = false;
    private List<string> DLBacklog = new List<string>();
    private int DLBacklogScrollUp = 0;
    private string DLCharNameString = "";
    private bool DLOptionsAvailable;
    //if this isn't "" then the string is typewriting inside
    private string DLInsideRTFTag = "";
    private string DLOutsideRTFTag = "";
    private bool DLInRichText = false;
    private int DLRTFOpeningStart = 0;
    private int DLRTFOpeningEnd = 0;
    //choosing dialogue options
    //list of option descriptions
    private string[] DLChooseLines;
    //cooldown before you can choose dialogue options
    private float DLChooseCooldown = 0;
    private float DLChooseCooldownDefault = 0.3f;
    //fading in and out of scenes
    private float[] DLFadeColor = new float[]{0f,0f,0f};
    private float DLFadeInTime = 2f;
    private float DLFadeOutTime = 2f;
    private bool DLFadeAtEnd = false;
    //hovering textbox
    private float DLTextboxMoveProgress = 0f;
    //the row is which character box it corresponds to, the 4 numbers are x,y,w,h
    private float[,] DLTextboxCoords = new float[3, 4]{ {0,0,500,200}, {100,0,500,200}, {200,0,500,200} };
    private float[] DLTextboxDefaultCoords = new float[4]{0,0,500,200};
    //the one you are hovering over
    private int DLChooseSelected = 0;
    //audio related things
    private List<string> MusicBacklog = new List<string>();
    private List<string> BackgroundAudioBacklog = new List<string>();
    private AudioClip ADCurrentSound;
    private AudioClip ADLastMusic;
    private bool ADSoundAvailable = false;

    private GameContext ctx = new GameContext(new Dictionary<string,object>());
    private DialogueGraph DGraph;

    public PresetCollection DPresets;

    public float ADMusicVolume = 0.2f;
    public float ADBackgroundVolume = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        DLMain.text = "sneed";
        //https://docs.unity3d.com/ScriptReference/Resources.Load.html
        DGraph = new DialogueGraph("Graphs/convo3_obj");
        ctx.data.Add("money",105);
        DPresets = new PresetCollection("Graphs/presets");
        DLAudioMusic.loop = true;
        DLAudioMusic.volume = ADMusicVolume;
        //backlog is disabled by default
        TurnOffBacklogView();
        TurnOffChooseView();
        object x = new int[]{15,50};
        int[] y = (int[]) x;
    }

    //void SetText(string s) {
    //	uidltext.text = s;
    //}

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == DLState.Transition) {
            Debug.Log("transitioning");
            InitiateDL();
            CurrentState = DLState.Dialogue;
        }
        if (DLIsTypewriting) {
            DLProgress += Time.deltaTime/DLSpeed;
            DLProgressChar = Mathf.RoundToInt(DLProgress);
            int DLProgressLastChar = Mathf.RoundToInt(DLProgress-Time.deltaTime/DLSpeed);

            if (DLPause <= 0) {
                if (DLProgressChar < DLLength - 1) {
                    if (DLProgressChar == DLProgressLastChar + 1) {
                        //new character was added this frame. play sound
                        if (ADSoundAvailable && DLCurrent[DLProgressChar] != ' ') {
                            DLAudioTrack1.PlayOneShot(ADCurrentSound);
                        }
                        //splitting text into multiple parts using the "&" character
                        if (DLCurrent[DLProgressChar] == '&') {
                            DLIsTypewriting = false;
                        }

                        //adding pause due to "%"
                        if (DLCurrent[DLProgressChar] == '%') {
                            DLPause = DLPauseAmount;
                            //extract the current character and re-merge DLCurrent
                            //reduce dlprogress back one to account
                            DLCurrent = DLCurrent.Substring(0,DLProgressChar-1) + DLCurrent.Substring(DLProgressChar+1);
                            DLProgress -= Time.deltaTime/DLSpeed;
                            DLProgressChar = Mathf.RoundToInt(DLProgress);
                        }

                        //entering richtext. skip to the first char after the opening
                        if (!DLInRichText && DLCurrent[DLProgressChar] == '<') {
                            DLRTFOpeningStart = DLProgressChar;
                            int lengthofopening = DLCurrent.Substring(DLProgressChar).IndexOf('>');
                            DLRTFOpeningEnd = DLProgressChar+lengthofopening;
                            
                            DLInsideRTFTag = DLCurrent.Substring(DLProgressChar,lengthofopening+1);
                            if (DLInsideRTFTag == "<b>") {
                                DLOutsideRTFTag = "</b>";
                            }
                            else if (DLInsideRTFTag.Substring(0,2) == "<c") {
                                DLOutsideRTFTag = "</color>";
                            }
                            else if (DLInsideRTFTag == "<i>") {
                                DLOutsideRTFTag = "</i>";
                            }

                            DLInRichText = true;
                            DLProgress += lengthofopening+1;
                            DLProgressChar += lengthofopening+1;                        }
                        if (DLInRichText && DLCurrent[DLProgressChar] == '<') {
                            int lengthofending = DLCurrent.Substring(DLProgressChar).IndexOf('>');

                            DLInRichText = false;
                            DLProgress += lengthofending+1;
                            DLProgressChar += lengthofending+1;
                        }
                    }
                }

            } else {
                DLPause -= Time.deltaTime/DLSpeed;
            }
            
            if (!DLInRichText) {
                SetMainText(DLCurrent.Substring(0,Mathf.Min(DLLength,DLProgressChar)));
            } else {
                //SetMainText(DLCurrent.Substring(0,Mathf.Min(DLLength,DLProgressChar)));
                SetMainText(DLCurrent.Substring(0,DLRTFOpeningStart) + DLInsideRTFTag + DLCurrent.Substring(DLRTFOpeningEnd+1,DLProgressChar-DLRTFOpeningEnd) + DLOutsideRTFTag);
            }

            if (DLProgress >= DLLength) {
                DLIsTypewriting = false;
            }
        }

        int nextAnd = DLCurrent.IndexOf('&');
        //handling state and input
        if (CurrentState == DLState.Dialogue) {
            if (nextAnd == -1) {
                //no upcoming '&'s. normal functionality
                if (DLOptionsAvailable && !DLIsTypewriting) {
                    CurrentState = DLState.Choose;
                    DLChooseCooldown = DLChooseCooldownDefault;
                    RenderChooseText();
                    TurnOnChooseView();
                    AdjustChooseBackground();
                } else if (DLIsTypewriting) {
                    if (Input.GetKeyDown("z")) {
                        SkipTypewriting();
                    }
                } else if (!DLOptionsAvailable && !DLIsTypewriting) {
                    if (Input.GetKeyDown("z")) {
                        ProceedNoOption();
                    }
                }
            } else {
                if (DLIsTypewriting) {
                    if (Input.GetKeyDown("z")) {
                        SkipTypewritingNextAmpersand();
                    }
                }
                else if (!DLIsTypewriting) {
                    if (Input.GetKeyDown("z")) {
                        StartDLNextAmpersand();
                    }
                }
            }
            //opening backlog
            if (Input.GetKeyDown(KeyCode.Backspace)) {
                CurrentState = DLState.Backlog;
                TurnOnBacklogView();
            }
        }
        else if (CurrentState == DLState.Backlog) {
            if (Input.GetKey("up") && DLBacklogScrollTime <= 0) {
                MakeBacklogScrollUp();
                DLBacklogScrollTime = DLBacklogScrollTimeMax;
            }
            else if (Input.GetKey("up") && DLBacklogScrollTime > 0) {
                DLBacklogScrollTime -= Time.deltaTime;
            }
            else if (Input.GetKey("down") && DLBacklogScrollTime <= 0) {
                MakeBacklogScrollDown();
                DLBacklogScrollTime = DLBacklogScrollTimeMax;
            }
            else {
                DLBacklogScrollTime -= Time.deltaTime;
            }
            //closing backlog
            if (Input.GetKeyDown(KeyCode.Backspace)) {
                CurrentState = DLState.Dialogue;
                TurnOffBacklogView();
            }
        }
        else if (CurrentState == DLState.Choose) {
            if (Input.GetKeyDown("down") && DLChooseSelected < DLNumOptions - 1) {
                DLChooseSelected += 1;
                RenderChooseText();
            }
            else if (Input.GetKeyDown("up") && DLChooseSelected > 0) {
                DLChooseSelected -= 1;
                RenderChooseText();
            }
            else if (DLChooseCooldown <= 0) {
                if (Input.GetKeyDown("z")) {
                    ChooseDialogueOption(DLChooseSelected);
                    TurnOffChooseView();
                }
            }
            else {
                DLChooseCooldown -= Time.deltaTime;
            }
        }
        else if (CurrentState == DLState.FadeIn) {

        }
        else if (CurrentState == DLState.FadeOut) {

        }
    }

    void TurnOffChooseView() {
        DLChooseText.color = new Color32(255, 255, 255, 0);
        DLChooseBackground.enabled = false;
    }

    void TurnOnChooseView() {
        DLChooseText.color = new Color32(255, 255, 255, 255);
        DLChooseBackground.enabled = true;
    }

    //makes the background appropriate width and height for the dialogue options
    void AdjustChooseBackground() {
        int m = 0;
        for (int i = 0; i < DLChooseLines.Length; i++) {
            m = Mathf.Max(m, DLChooseLines[i].Length);
        }
        int w = ChooseBackgroundMaxPixPerCharacter * m;
        int h = ChooseBackgroundMaxPixPerLine * DLChooseLines.Length;
        DLChooseBackground.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        DLChooseBackground.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    void RenderChooseText() {
        string s = "";
        for (int i = 0; i < DLChooseLines.Length; i++) {
            if (i == DLChooseSelected) {
                s += "<color=\"red\">" + DLChooseLines[i] + "</color>";
            }
            else {
                s += DLChooseLines[i];
            }

            if (i < DLChooseLines.Length - 1) {
                s += "\n";
            }
        }
        DLChooseText.SetText(s);
    }

    void SkipTypewritingNextAmpersand() {
        DLIsTypewriting = false;
        DLProgress = DLCurrent.IndexOf('&');
        DLProgressChar = DLCurrent.IndexOf('&');
        //update it cause it wont update itself now that typewriting is off
        SetMainText(DLCurrent.Substring(0,Mathf.Min(DLLength,DLProgressChar)));
    }

    void StartDLNextAmpersand() {
        DLIsTypewriting = true;
        DLCurrent = DLCurrent.Substring(DLCurrent.IndexOf('&')+1);
        DLLength = DLCurrent.Length;
        DLProgress = 0;
        DLProgressChar = 0;
        DLInsideRTFTag = "";
        DLOutsideRTFTag = "";
        DLInRichText = false;
    }

    AudioClip GetSoundEffect(string name) {
        return Resources.Load<AudioClip>("Sounds/" + name);
    }

    AudioClip GetMusic(string name) {
        return Resources.Load<AudioClip>("Music/" + name);
    }

    void LoadDPreset(DialoguePreset dp) {
        DLSpeed = dp.dlspeed;
        DLCharNameString = dp.charname;
        DLCharName.text = dp.charname;
        if (dp.dlsound != "") {
            ADCurrentSound = GetSoundEffect(dp.dlsound);
            ADSoundAvailable = true;
        } else {
            ADSoundAvailable = false;
        }
    }

    //chops up a text into segments according to maximum character length
    private List<string> ChopText(string s, int maxCharLength) {
        List<string> chopped = new List<string>();
        if (s.Length <= maxCharLength) {
            chopped.Add(s);
            return chopped;
        }
        for (int i = 0; i < s.Length; i += maxCharLength) {
            int f = Mathf.Min(s.Length,i+maxCharLength);
            chopped.Add(s.Substring(i,f-i));
        }
        return chopped;
    }

    //inclusive of start and end
    //todo make this better
    private List<string> GetBetweenIndices(List<string> l, int start, int end) {
        string[] a = l.ToArray();
        if (a.Length == 0) {
            return new List<string>();
        }
        List<string> res = new List<string>();
        for (int i = start; i < end; i++) {
            res.Add(a[i]);
        }
        return res;
    }

    //set the backlog text according to current scroll amount
    public void RenderBacklogText() {
        int start = Mathf.Max(0,DLBacklog.Count - DLBacklogScrollUp - DLBacklogMaxDisplayLines);
        int end = Mathf.Min(DLBacklog.Count,DLBacklog.Count - DLBacklogScrollUp);
        List<string> relevantlines = GetBetweenIndices(DLBacklog, start, end);
        string[] relevantlinesarr = relevantlines.ToArray();
        string res = "";
        for (int i = 0; i < relevantlinesarr.Length; i++) {
            res += relevantlinesarr[i];
            if (i < relevantlinesarr.Length - 1) {
                res += "\n";
            }
        }
        SetBacklogText(res);
    }

    public void MakeBacklogScrollUp() {
        if (CurrentState == DLState.Backlog) {
            if (DLBacklog.Count - DLBacklogScrollUp - DLBacklogMaxDisplayLines > 0) {
                DLBacklogScrollUp += 1;
            }
            RenderBacklogText();
        }
    }

    public void MakeBacklogScrollDown() {
        if (CurrentState == DLState.Backlog) {
            if (DLBacklogScrollUp > 0) {
                DLBacklogScrollUp -= 1;
            }
            RenderBacklogText();
        }
    }

    private string RemoveSpecialDLChars(string s) {
        string[] toremove = new string[] {"%","&"};
        foreach (string c in toremove) {
            s = s.Replace(c, string.Empty);
        }
        return s;
    }

    public void SetBacklogText(string s) {
        DLBacklogText.SetText(s);
    }

    public void TurnOnBacklogView() {
        DLBacklogText.color = new Color32(255, 255, 255, 255);
        DLBacklogBackground.enabled = true;
    }

    public void TurnOffBacklogView() {
        DLBacklogText.color = new Color32(255, 255, 255, 0);
        DLBacklogBackground.enabled = false;
    }

    //sets up beginning of new dialogue node
    void InitiateDL() {
        string pres = (string)DGraph.GetCurrent("preset");
        string bckg = (string)DGraph.GetCurrent("background");
        string mus = (string)DGraph.GetCurrent("music");
        LoadDPreset(pres);
        DLCurrent = DGraph.current.dl;
        DLIsTypewriting = true;
        DLProgress = 0f; 
        DLProgressChar = 0;
        DLIsSet = true;
        DLInsideRTFTag = "";
        DLOutsideRTFTag = "";
        DLInRichText = false;
        DLLength = DLCurrent.Length;
        DLNumOptions = DGraph.current.outdl.Length;
        DLOptionsAvailable = DGraph.current.outdl.Length != 0;
        DLChooseLines = DGraph.current.outdl;
        
        SetBackground("Images/" + bckg);

        string[] a = (string[])DGraph.GetCurrent("charportraits");
        int l = a.Length;
        if (l == 0) {
            BlankChar(1);
            BlankChar(2);
            BlankChar(3);
        }
        else if (l == 1) {
            BlankChar(1);
            SetChar(2,"Images/" + a[0]);
            BlankChar(3);
        }
        else if (l == 2) {
            SetChar(1,"Images/" + a[0]);
            SetChar(2,"Images/" + a[1]);
            BlankChar(3);
        }
        else if (l == 3) {
            SetChar(1,"Images/" + a[0]);
            SetChar(2,"Images/" + a[1]);
            SetChar(3,"Images/" + a[2]);
        }

        //setting music and ambience
        MusicBacklog.Add(mus);
        string[] tmplist = MusicBacklog.ToArray();
        
        if (tmplist.Length >= 2) {
            string currentMusic = tmplist[tmplist.Length-1];
            string lastMusic = tmplist[tmplist.Length-2];
            if (currentMusic == "" && lastMusic != "") {
                DLAudioMusic.Stop();
            } else if (currentMusic != "" && lastMusic != currentMusic) {
                DLAudioMusic.clip = GetMusic(mus);
                DLAudioMusic.Play();
            }
        } else {
            if (tmplist[0] != "") {
                DLAudioMusic.clip = GetMusic(mus);
                DLAudioMusic.Play();
            }
        }

        string adbackground = (string)DGraph.GetCurrent("adbackground");
        BackgroundAudioBacklog.Add(adbackground);
        string[] tmplistbackground = BackgroundAudioBacklog.ToArray();
        if (tmplistbackground.Length >= 2) {
            string currentAmbience = tmplistbackground[tmplistbackground.Length-1];
            string lastAmbience = tmplistbackground[tmplistbackground.Length-2];
            if (currentAmbience == "" && lastAmbience != "") {
                DLAudioBackground.Stop();
            } else if (currentAmbience != "" && lastAmbience != currentAmbience) {
                DLAudioBackground.clip = GetMusic(adbackground);
                DLAudioBackground.Play();
            }
        } else {
            if (tmplistbackground[0] != "") {
                DLAudioBackground.clip = GetMusic(adbackground);
                DLAudioBackground.Play();
            }
        }

        //updating dialogue backlog
        DLBacklog.Add(DLCharNameString != "" ? "<color=\"yellow\">" + DLCharNameString + ":</color>" : "");
        List<string> tempbacklogadd = ChopText(RemoveSpecialDLChars(DLCurrent),DLBacklogMaxChars);
        List<string> fullbacklogadd = new List<string>();
        foreach (string t in tempbacklogadd) {
            fullbacklogadd.AddRange(t.Split('\n'));
        }
        DLBacklog.AddRange(fullbacklogadd);
        DLBacklog.Add("");
        RenderBacklogText();
    }

    void SetMainText(string s) {
        DLMain.SetText(s);
    }

    void SetBackground(string filePath) {
        DLBackground.texture = Resources.Load<Texture2D>(filePath);
    }

    void SetChar(int which, string filePath) {
        if (filePath == "" || filePath == "Images/") {
            return;
        }
        switch (which) {
            case 1:
                DLChar1.texture = Resources.Load<Texture2D>(filePath);
                break;
            case 2:
                DLChar2.texture = Resources.Load<Texture2D>(filePath);
                break;
            case 3:
                DLChar3.texture = Resources.Load<Texture2D>(filePath);
                break;
        }
    }

    void BlankChar(int which) {
        switch (which) {
            case 1:
                DLChar1.texture = Resources.Load<Texture2D>("Images/transparent");
                break;
            case 2:
                DLChar2.texture = Resources.Load<Texture2D>("Images/transparent");
                break;
            case 3:
                DLChar3.texture = Resources.Load<Texture2D>("Images/transparent");
                break;
        }
    }

    //resets everything, doesnt restart music if it's the same
    void StartDialogueGraph(string graphFilePath, ref GameContext ctx) {

    }

    //NOTE: 'which' is 1,2,3,... while referencing the DGraph.current.outdl 0,1,2,... indices
    void ChooseDialogueOption(int which) {
        bool didGo = DGraph.ChooseOption(ref ctx, which);
        if (didGo) {
            if (DLFadeAtEnd) {
                CurrentState = DLState.FadeOut;
            } else {
                CurrentState = DLState.Transition;
            }
        } else {
            Debug.Log("Can't choose this dialogue option!");
        }
    }

    //proceed if there are no options available
    void ProceedNoOption() {
        bool didGo = DGraph.Next(ref ctx);
        if (didGo) {
            CurrentState = DLState.Transition;
        } else {
            Debug.Log("Can't proceed in dialogue!");
        }
    }

    void SkipTypewriting() {
        DLIsTypewriting = false;
        DLProgress = 1f;
        DLProgressChar = DLLength;
        SetMainText(DLCurrent);
        DLInsideRTFTag = "";
        DLOutsideRTFTag = "";
        DLInRichText = false;
    }

    void SetTextboxTransform(float xpos, float ypos, float width, float height) {
        //todo
    }

    void InterpolateTextbox() {
        float x,y,w,h;
        float a = DLTextboxMoveProgress;
        int p,c = -1;
        if ((int)DGraph.GetCurrent("textboxpos") != null) {
            c = (int)DGraph.GetCurrent("textboxpos");
        }
        if ((int)DGraph.GetPrevious("textboxpos") != null) {
            p = (int)DGraph.GetPrevious("textboxpos");
        }
        float[,] K = DLTextboxCoords;
        float[] D = DLTextboxDefaultCoords;
        if (!(p == -1) && (c == -1)) {
            x = K[p,0]+a*(D[0]-K[p,0]);
            y = K[p,1]+a*(D[1]-K[p,1]);
            w = K[p,2]+a*(D[2]-K[p,2]);
            h = K[p,3]+a*(D[3]-K[p,3]);
        }
        else if ((p == -1) && !(c == -1)) {
            x = D[0]+a*(K[c,0]-D[0]);
            y = D[1]+a*(K[c,1]-D[1]);
            w = D[2]+a*(K[c,2]-D[2]);
            h = D[3]+a*(K[c,3]-D[3]);
        }
        else if (!(p == -1) && !(c == -1)) {
            x = K[p,0]+a*(K[c,0]-K[p,0]);
            y = K[p,1]+a*(K[c,1]-K[p,1]);
            w = K[p,2]+a*(K[c,2]-K[p,2]);
            h = K[p,3]+a*(K[c,3]-K[p,3]);
        }
        else {
            return;
        }
        SetTextboxTransform(x,y,w,h);
    }

    void SetTextboxCurrent() {
        int c = -1;
        if ((int)DGraph.GetCurrent("textboxpos") != null) {
            c = (int)DGraph.GetCurrent("textboxpos");
        }
        if (c == -1) {
            SetTextboxTransform(DLTextboxDefaultCoords[0], DLTextboxDefaultCoords[1], DLTextboxDefaultCoords[2], DLTextboxDefaultCoords[3]);
        }
        else {
            SetTextboxTransform(DLTextboxCoords[c,0], DLTextboxCoords[c,1], DLTextboxCoords[c,2], DLTextboxCoords[c,3]);
        }
    }
}