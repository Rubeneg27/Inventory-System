using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Toggles debug mode")]
    [SerializeField] private bool _debug = false;

    [Header("Slot Properties")]
    [Tooltip("Indicates if the slot is occupied")]
    public bool isOccupied = false;
    [Tooltip("Indicates if the slot is selected")]
    public bool isItemSelected = false;
    [Tooltip("Offset to move the item when selected")]
    [SerializeField] private float _slotOffset = 0.5f;
    [Tooltip("Item attached to the slot")]
    public GameObject _attachedItem;

    private ItemManager _itemManager;
    private Transform _childTransform;

    private void Start()
    {
        _childTransform = gameObject.GetComponentInChildren<Transform>();
        _itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();

        if (_itemManager == null)
        {
            UnityEngine.Debug.LogError("ItemManager not set in ItemSlot");
        }
    }

    private void Update()
    {
        CheckRetrieveItem();
    }

    /// <summary>
    /// Check if the item is being retrieved by targeting mouse input
    /// </summary>
    void CheckRetrieveItem()
    {
        if (Input.GetMouseButtonUp(0) && isItemSelected)
        {
            Item itemComponent = _attachedItem.GetComponent<Item>();
            if (itemComponent != null)
            {
                _itemManager.RetrieveItem(itemComponent);
            }
            else
            {
                UnityEngine.Debug.LogWarning("El objeto en el slot no tiene un componente Item.");
            }
            ReleaseAttachedItem();
        }
    }

    /// <summary>
    /// Set the occupied status of the slot
    /// </summary>
    /// <param name="occupied">Receive occupied status</param>
    /// <param name="item">Item to store in the slot</param>
    public void SetOccupied(bool occupied, GameObject item)
    {
        isOccupied = occupied;
        _attachedItem = item;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_debug) UnityEngine.Debug.Log("Pointer over item slot");
        isItemSelected = true;
        SetRetrievingItem();
        if (_childTransform != null)
        {
            _childTransform.position = new Vector3(
                _childTransform.position.x,
                _childTransform.position.y + _slotOffset,  // Aumentar en 1 unidad en Y
                _childTransform.position.z
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_debug) UnityEngine.Debug.Log("Pointer left item slot");
        if (_childTransform != null)
        {
            _childTransform.position = new Vector3(
                _childTransform.position.x,
                _childTransform.position.y - _slotOffset,  // Disminuir en 1 unidad en Y
                _childTransform.position.z
            );
        }

        //Release item
        isItemSelected = false;
    }

    /// <summary>
    /// Set in the itemManager the item to be retrieved
    /// </summary>
    public void SetRetrievingItem()
    {
        if (_itemManager != null && _attachedItem != null)
        {
            _itemManager.UpdateRetrievingItem(_attachedItem);
        }
        else if (_attachedItem == null) {
            if (_debug) UnityEngine.Debug.Log("No item attached to slot");
        }
        else
        {
            UnityEngine.Debug.LogError("ItemManager not set in ItemSlot");
        }
    }

    /// <summary>
    /// Clear the item being retrieved
    /// </summary>
    void ClearRetrievingItem()
    {
        _itemManager.retrievingItem = null;
    }

    /// <summary>
    /// Release the item from the slot, update the layout visibility and reset the item properties
    /// </summary>
    public void ReleaseAttachedItem()
    {
        // Liberar el item del slot
        isOccupied = false;
        isItemSelected = false;
        Transform _attachedItemTransform = _attachedItem.GetComponent<Transform>();
        _attachedItemTransform.SetParent(null);  // Libera al objeto hijo del padre (ItemSlot)
        _attachedItemTransform.gameObject.layer = LayerMask.NameToLayer("Default"); // Restaurar la capa del objeto
        _attachedItemTransform.GetComponent<Rigidbody>().useGravity = true; // Volver a habilitar la gravedad
        _attachedItemTransform.GetComponent<Rigidbody>().isKinematic = false; // Volver a deshabilitar kinematic

        _itemManager.ToggleItemsLayout(false);

        ClearRetrievingItem();

        if (_debug) UnityEngine.Debug.Log("Releasing item from slot: " + _attachedItem.name);
    }
}
