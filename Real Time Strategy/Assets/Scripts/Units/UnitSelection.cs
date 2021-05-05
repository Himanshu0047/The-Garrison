using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class UnitSelection : MonoBehaviour
{
    Camera mainCamera;
    public List<Unit> selectedUnits { get; } = new List<Unit>();
    [SerializeField] LayerMask layermask = new LayerMask();
    [SerializeField] RectTransform unitSelectionBox = null;
    Vector2 startPosition;
    RTSPlayer player;
    bool isBuilding = false;

    public void SetIsBuilding(bool isbuilding)
    {
        this.isBuilding = isbuilding;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }

        else if(Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }

        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
    }

    void StartSelectionArea()
    {
        // if dragging building UI then dont start selection area
        if(isBuilding) { return; }

        unitSelectionBox.gameObject.SetActive(true);

        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Deselect();
            }
            selectedUnits.Clear();
        }

        startPosition = Mouse.current.position.ReadValue();

        // Start updating the selection box in the same frame
        UpdateSelectionArea();
    }

    void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        // size of the box
        unitSelectionBox.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        // position of the anchors (make the position of the  anchors to the bottom left from unity)
        unitSelectionBox.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    void ClearSelectionArea()
    {
        unitSelectionBox.gameObject.SetActive(false);

        // if it is a single click on single unit
        if (unitSelectionBox.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layermask)) { return; }
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }
            if (!unit.hasAuthority) { return; }

            selectedUnits.Add(unit);
            foreach (Unit selectedUnit in selectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }

        // if multiple units
        Vector2 min = unitSelectionBox.anchoredPosition - (unitSelectionBox.sizeDelta / 2);
        Vector2 max = unitSelectionBox.anchoredPosition + (unitSelectionBox.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if(screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    void AuthorityHandleUnitDespawned(Unit unit)
    {
        selectedUnits.Remove(unit);
    }

    void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }

}
