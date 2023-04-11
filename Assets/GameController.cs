using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; } = null;
    public PaintCanvas paintCanvas = null;
    public NetworkController networkController = new NetworkController();

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
        // await networkController.SendTestMessage();
    }

    // Update is called once per frame
    async void Update()
    {
        networkController.Update();
    }

    private async void OnApplicationQuit()
    {
        await networkController.CloseConnection();
    }
}
