using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.EventSystems;

public class MiniMap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] RectTransform miniMapRect = null;
    [SerializeField] float mapScale = 20f;
    [SerializeField] float offset = -6f;

    Transform playerCameraTransform;

    private void Update()
    {
        if(playerCameraTransform != null) { return; }

        if(NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRect, mousePos, null, out Vector2 localPoint)) { return; }

        Vector2 lerp = new Vector2((localPoint.x - miniMapRect.rect.x) / miniMapRect.rect.width, (localPoint.y - miniMapRect.rect.y) / miniMapRect.rect.height);

        Vector3 newCameraPos = new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x), playerCameraTransform.position.y, Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }

    
}
