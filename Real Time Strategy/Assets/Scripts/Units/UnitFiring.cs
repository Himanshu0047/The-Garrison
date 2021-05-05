using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] Targeter targeter = null;
    [SerializeField] GameObject projectile = null;
    [SerializeField] Transform projectileSpawnPoint = null;
    [SerializeField] float firingRange = 6f;
    [SerializeField] float fireRate = 1f;
    [SerializeField] float rotationSpeed = 20f;
    float lastFireTime = 0f;

    #region Server

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if(target == null) { return; }

        if(!CanFireAtTarget()) { return; }

        // Rotate towards target
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);
            GameObject projectileInstance = Instantiate(projectile, projectileSpawnPoint.position, projectileRotation);
            // Spawn on all clients
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            lastFireTime = Time.time;
        }
    }

    bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= firingRange * firingRange;
    }

    #endregion
}
