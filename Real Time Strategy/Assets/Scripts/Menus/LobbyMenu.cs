using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] GameObject lobbyUI = null;
    [SerializeField] Button startGameButton = null;
    [SerializeField] TMP_Text[] playerNamesText = new TMP_Text[4];

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> playersList = ((RTSNetworkManager)NetworkManager.singleton).players;

        for(int i = 0; i < playersList.Count; i++)
        {
            playerNamesText[i].text = playersList[i].GetDisplayName;
        }

        for(int i = playersList.Count; i < playerNamesText.Length; i++)
        {
            playerNamesText[i].text = "Waiting for Player...";
        }

        startGameButton.interactable = playersList.Count >= 2;
    }

    void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }

    }
}
