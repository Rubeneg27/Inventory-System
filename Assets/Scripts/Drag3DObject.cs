using UnityEngine;
using UnityEngine.EventSystems;

public class Drag3DObject : MonoBehaviour
{
    [Tooltip("Toggles debug mode.")]
    [SerializeField] public bool _debug = false;
    
    private bool isDragging = false; // Indicates if the object is being dragged

    [Header("References")]
    private Camera mainCamera;
    [SerializeField] private ItemManager _itemManager;
    public DropZone _dropZone;
    public static Drag3DObject draggedObject = null; // Prevent multiple objects being dragged at the same time

    
    private void Start()
    {
        mainCamera = Camera.main;

        if (_itemManager == null)
        {
            Debug.LogError("ItemManager not set in Drag3DObject");
        }
    }

    private void OnMouseDown()
    {
        if (draggedObject == null)
        {
            isDragging = true;
            _itemManager.isDragging = true;
            draggedObject = this;
            if (_debug) Debug.Log("Dragged object: " + draggedObject.name);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        _itemManager.isDragging = false;

        if (_dropZone.pointerOverBag)
        {
            if (_debug) Debug.Log("Item released over bag");
            _dropZone.DropItem();
        }
        else
        {
            if (_debug) Debug.Log("Item release in 3D space");
        }

        draggedObject = null;
    }

    private void Update()
    {
        MouseFollow();
    }

    /// <summary>
    /// Follow the mouse position in the 3D space.
    /// </summary>
    private void MouseFollow()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 5f;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            transform.position = worldPosition;
        }
    }
}
