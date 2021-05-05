using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] Health health;
    [SerializeField] Unit unit = null;
    [SerializeField] Transform spawnPosition = null;
    [SerializeField] TMP_Text remainingUnitsText = null;
    [SerializeField] Image unitProgressImage = null;
    [SerializeField] int maxUnitQueue = 5;
    [SerializeField] float spawnMoveRange = 7f;
    [SerializeField] float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    int queuedUnits;
    [SyncVar]
    float unitTimer;

    float progressImageVelocity;


    private void Update()
    {
        if(isServer)
        {
            ProduceUnits();
        }

        if(isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    [Command]
    void CmdSpawnUnit()
    {
        if(queuedUnits == maxUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if(player.GetResources < unit.GetResourceCost) { return; }

        queuedUnits++;
        player.SetResources(player.GetResources - unit.GetResourceCost);
    }

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void ProduceUnits()
    {
        if(queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if(unitTimer < unitSpawnDuration) { return; }

        GameObject spawnedUnit = Instantiate(unit.gameObject, spawnPosition.position, unit.transform.rotation);
        NetworkServer.Spawn(spawnedUnit, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = 0.5f;
        spawnPosition.position = new Vector3(spawnPosition.position.x, 0, spawnPosition.position.z);
        UnitMovement unitMovement = spawnedUnit.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnPosition.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }
        if(!hasAuthority) { return; }
        CmdSpawnUnit();
    }

    void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;
        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion

}
