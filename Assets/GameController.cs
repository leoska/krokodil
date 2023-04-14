using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Networking;

public class GameController : MonoBehaviour
{
    private float _timer = 0f;
    
    public static GameController Instance { get; private set; } = null;
    public PaintCanvas paintCanvas = null;
    public NetworkController networkController = new NetworkController();
    public string nickName = "player";
    public int tickRate = 20;

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
    async void Start()
    {
        await networkController.Connect();
        
        await Task.Delay(1000);
        
        _timer = 0f;
    }

    // Update is called once per frame
    async void Update()
    {
        networkController.DispatchMessageQueue();
        _timer += Time.deltaTime;

        if (_timer >= 1f / tickRate)
        {
            networkController.Tick();
            _timer = 0f;
        }
        
    }

    private async void OnApplicationQuit()
    {
        await networkController.CloseConnection();
    }
}
