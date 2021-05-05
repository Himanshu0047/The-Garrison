using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] Rigidbody rb = null;
    [SerializeField] float destroyAfterSeconds = 5f;
    [SerializeField] float launchForce = 10f;
    [SerializeField] int damageToDeal = 20;

    private void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    #region Server

    public override void OnStartServer()
    {
        base.OnStartServer();
        Invoke(nameof(SelfDestroy), destroyAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            if(networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);
        }

        SelfDestroy();
    }

    [Server]
    void SelfDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion
}
