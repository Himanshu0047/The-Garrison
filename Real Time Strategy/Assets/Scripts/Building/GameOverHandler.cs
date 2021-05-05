using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOverHandler : NetworkBehaviour
{
    List<UnitBase> bases = new List<UnitBase>();

    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDepawned;
    }

    private void OnDestroy()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDepawned;
    }

    [Server]
    void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    void ServerHandleBaseDepawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if(bases.Count != 1) { return; }

        RTSPlayer player = bases[0].connectionToClient.identity.GetComponent<RTSPlayer>();
        RpcGameOver(player.GetDisplayName);

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
