using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Networking;
using TMPro;
using UnityEditor;
using Random = System.Random;

public class GameController : MonoBehaviour
{
    private float _timer = 0f;
    public string winWord = "";

    public static GameController Instance { get; private set; } = null;
    
    [Header("Paint canvas ScriptableObject")]
    public PaintCanvas paintCanvas = null;
    public GameObject canvasForDraw = null;
    
    [SerializeField]
    public WebSocketController networkController = new WebSocketController();

    [Header("Main Menu Game Object")] 
    public GameObject mainMenu = null;
    public GameObject mainMenuUI = null;
    
    [Header("Game Room Game Object")] 
    public GameObject gameRoom = null;
    public GameObject gameRoomUI = null;
    public GameObject chatUI = null;
    public GameObject drawUI = null;
    public GameObject chatPanel = null;
    public GameObject chatMessagePref = null;
    public GameObject chatInput = null;
    public GameObject winnerPanel = null;
    
    [Header("Game Backgrounds")] 
    public SpriteRenderer backgroundRenderer = null;
    public Sprite mainMenuBackground = null;
    public Sprite[] backgrounds;

    [Header("Global game state")] public GameState gameState = GameState.MainMenu;
    
    [Header("Nickname player")]
    public string nickName = "player";
    
    [Header("System tick rates")]
    public int tickRate = 33;
    
    private async void OnApplicationQuit()
    {
        await networkController.CloseConnection();
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
        
    }

    // Start is called before the first frame update
    private async void Start()
    {
        // await networkController.Connect();
        GoToMainMenu();
        
        await Task.Delay(1000);
        
        _timer = 0f;
    }

    // Update is called once per frame
    private async void Update()
    {
        networkController.DispatchMessageQueue();
        _timer += Time.deltaTime;

        if (_timer >= 1f / tickRate)
        {
            networkController.Tick();
            _timer = 0f;
        }
        
    }

    public void GoToMainMenu()
    {
        mainMenu.SetActive(true);
        mainMenuUI.SetActive(true);
        
        gameRoom.SetActive(false);
        gameRoomUI.SetActive(false);
        
        canvasForDraw.SetActive(false);
        winnerPanel.SetActive(false);
        
        backgroundRenderer.sprite = mainMenuBackground;
        gameState = GameState.MainMenu;
    }

    public void FindGame()
    {
        TryToConnectGame();
    }

    public async Task TryToConnectGame(int port = 25565)
    {
        // networkController.Connect();
        await networkController.Connect();
    }

    public void GoToGameRoom()
    {
        Random rnd = new Random();
        mainMenu.SetActive(false);
        mainMenuUI.SetActive(false);
        
        gameRoom.SetActive(true);
        gameRoomUI.SetActive(true);

        backgroundRenderer.sprite = backgrounds[rnd.Next(0, 3)];

        ResetGame();

        gameState = GameState.GameRoom;
    }

    public void ResetGame()
    {
        foreach (GameObject msg in chatPanel.transform)
        {
            Destroy(msg);
        }

        chatUI.SetActive(false);
        drawUI.SetActive(false);

        paintCanvas.ClearCanvas();
        canvasForDraw.SetActive(false);
        winnerPanel.SetActive(false);

        winWord = "";

        chatInput.GetComponent<TextMeshPro>().text = "";
    }

    public void SetProgress()
    {
        
    }

    public void MySelectWord(int index)
    {
        
    }

    public void SetSpectate()
    {
        canvasForDraw.SetActive(true);
        
        chatUI.SetActive(false);
        drawUI.SetActive(false);
    }

    public void SelectWord(WebSocketController.SelectWordData packet)
    {
        canvasForDraw.SetActive(true);
        paintCanvas.ClearCanvas();
        
        chatUI.SetActive(!packet.draw);
        drawUI.SetActive(packet.draw);
    }

    public void NewMessage(WebSocketController.ChatData packet)
    {
        GameObject newMsg = Instantiate(chatMessagePref, chatPanel.transform);
        newMsg.GetComponent<TextMeshPro>().text = packet.chatString;
    }

    public void SendChatMessage()
    {
        string msg = chatInput.GetComponent<TextMeshPro>().text;

        if (msg.Trim().Length < 1)
            return;
        
        networkController.SendMsgEvent(new WebSocketController.ChatData()
        {
            chatString = msg,
        });
        
        chatInput.GetComponent<TextMeshPro>().text = "";

        if (msg == winWord)
        {
            winnerPanel.SetActive(true);
            
            chatUI.SetActive(false);
            drawUI.SetActive(false);
        }
    }

    public void FinishThisGame(WebSocketController.FinishData packet)
    {
        if (packet.winner)
        {
            winnerPanel.SetActive(true);
            
            chatUI.SetActive(false);
            drawUI.SetActive(false);
        }
    }

    public enum GameState
    {
        MainMenu,
        GameRoom,
    }
}
