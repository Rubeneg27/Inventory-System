using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Toggles debug mode.")]
    [SerializeField] private bool _debug = false;

    [Header("References")]
    [SerializeField] private ItemManager _itemManager;
    [SerializeField] private GameObject _itemsLayout;
    [SerializeField] private BagPack _bagPack;

    [Tooltip("Cursor over the bagback zone")]
    public bool pointerOverBag = false;

    // Control variables
    private bool isHoldingMLB = false;
    private bool itemsLayoutActive = false;

    private Vector2 _initialRectTransform;

    private void Start()
    {
        _initialRectTransform = gameObject.GetComponent<RectTransform>().offsetMax;
        _bagPack = GameObject.Find("BagPack").GetComponent<BagPack>();
    }

    private void Update()
    {
        CheckLayoutVisibility();
    }

    /// <summary>
    /// Check if items layout should be visible, according to the mouse input, and the pointer position.
    /// </summary>
    void CheckLayoutVisibility()
    {
        isHoldingMLB = Input.GetMouseButton(0);
        itemsLayoutActive = Input.GetMouseButtonUp(0) ? false : itemsLayoutActive;

        // Show items layout when holding left mouse button and pointer over bag
        if (isHoldingMLB && pointerOverBag && !_itemManager.isDragging)
        {
            itemsLayoutActive = true;
            _itemManager.ToggleItemsLayout(true);
        }

        // Hide items layout when not holding left mouse button
        if (!isHoldingMLB && !itemsLayoutActive && _itemManager.retrievingItem == null)
        {
            _itemManager.ToggleItemsLayout(false);
        }
    }

    /// <summary>
    /// Store the dragged object in the item manager.
    /// </summary>
    public void DropItem()
    {
        if (Drag3DObject.draggedObject != null)
        {
            if (_debug) Debug.Log("Dragged object: " + Drag3DObject.draggedObject.name);
            _itemManager.AddItemToFreeSlot(Drag3DObject.draggedObject.gameObject);
            Drag3DObject.draggedObject = null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOverBag = true;
        _bagPack.ReacToCursor(pointerOverBag);
        if (_debug) Debug.Log("Cursor sobre la bolsa.");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOverBag = false;
        _bagPack.ReacToCursor(pointerOverBag);
        if (_debug) Debug.Log("Cursor salió de la bolsa.");
    }
}
