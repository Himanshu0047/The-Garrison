using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] GameObject gameOverDisplayParent = null;
    [SerializeField] TMP_Text winnerNameText = null;

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = winner + " has won !";
        gameOverDisplayParent.SetActive(true);
    }

    public void LeaveGame()
    {
        // if host
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // if client
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
