using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] Health health;
    [SerializeField] int resourcesPerInterval = 10;
    [SerializeField] float interval = 2f;

    float timer;
    RTSPlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            player.SetResources(player.GetResources + resourcesPerInterval);

            timer = interval;
        }
    }

    void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    void ServerHandleGameOver()
    {
        enabled = false;
    }
}
