using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Networking;
using UnityEditor;

public class GameController : MonoBehaviour
{
    private float _timer = 0f;

    public static GameController Instance { get; private set; } = null;
    
    [Header("Paint canvas ScriptableObject")]
    public PaintCanvas paintCanvas = null;
    
    [SerializeField]
    public WebSocketController networkController = new WebSocketController();
    
    [SerializeField]
    public HttpController httpController = new HttpController();

    [Header("Main Menu Game Object")] 
    public GameObject mainMenu = null;
    public GameObject mainMenuUI = null;
    
    [Header("Game Room Game Object")] 
    public GameObject gameRoom = null;
    public GameObject gameRoomUI = null;

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
        
        backgroundRenderer.sprite = mainMenuBackground;
    }

    public void GoToGameRoom()
    {
        
    }

    public enum GameState
    {
        MainMenu,
        GameRoom,
    }
}
