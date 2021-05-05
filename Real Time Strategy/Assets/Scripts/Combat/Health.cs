using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    int currentHealth;

    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void DealDamage(int damageTaken)
    {
        if(currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageTaken, 0);

        if(currentHealth != 0) { return; }

        ServerOnDie?.Invoke();

        Debug.Log("We Died");
    }

    [Server]
    void ServerHandlePlayerDie(int connectionId)
    {
        if(connectionToClient.connectionId != connectionId) { return; }

        DealDamage(currentHealth);
    }

    #endregion

    #region Client

    void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
