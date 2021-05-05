using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] Transform playerCameraTransform = null;
    [SerializeField] float speed = 20f;
    [SerializeField] float screenBorderThickness = 10f;
    [SerializeField] public Vector2 screenXLimits = Vector2.zero;
    [SerializeField] public Vector2 screenZLimits = Vector2.zero;

    Controls controls;
    Vector2 previousInput;
    

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if(!hasAuthority || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Vector3 position = playerCameraTransform.position;

        if(previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;
            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if(cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z += 1;
            }
            else if(cursorPosition.y <= screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }
            if(cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x += 1;
            }
            else if(cursorPosition.x <= screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }

            position += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            position += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        position.x = Mathf.Clamp(position.x, screenXLimits.x, screenXLimits.y);
        position.z = Mathf.Clamp(position.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = position;
    }

    void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }

}
