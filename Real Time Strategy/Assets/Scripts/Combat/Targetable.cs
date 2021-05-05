using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targetable : NetworkBehaviour
{
    [SerializeField] Transform aimAtPoint;

    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
}
