using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para manejar eventos de UI

public class BagPack : MonoBehaviour
{
    [Tooltip("Toggles debug mode.")]
    [SerializeField] private bool _debug = false;

    [Header("References")]
    private ItemManager _itemManager;
    private DropZone _dropZone;
    private Animator _animator;

    [Header("Bagpack spin contro variables")]
    [Tooltip("Indicates if the bagpack model will spin")]
    public bool willSpin = false;
    [Tooltip("Bagpack model spin speed")]
    [SerializeField] private float _bagPositionOffset = 0.4f;
    // Bagpack original position in layout
    private Vector3 _originalPosition;

    private void Start()
    {
        _itemManager = FindAnyObjectByType<ItemManager>();
        _dropZone = FindAnyObjectByType<DropZone>();
        _animator = GetComponent<Animator>();
        _originalPosition = transform.position;

    }
    private void Update()
    {
        SpinBagPackModel();
    }

    /// <summary>
    /// Spin the bagpack model when dragging an item
    /// </summary>
    void SpinBagPackModel()
    {
        willSpin = _itemManager.isDragging && _dropZone.pointerOverBag;
        if (!willSpin) return;
        transform.Rotate(Vector3.up, 1f);
    }

    /// <summary>
    /// React to the cursor over the bagpack and move the model a little bit up
    /// </summary>
    /// <param name="isPointerOverBag">Receive if bagpack is being pointed or not</param>
    public void ReacToCursor(bool isPointerOverBag)
    {
        if (_debug) Debug.Log("Reacting to cursor over bag");
        if (isPointerOverBag)
            transform.position = _originalPosition + new Vector3(0, _bagPositionOffset, 0);
        else
            transform.position = _originalPosition;
    }

    /// <summary>
    /// Run the wobble animation when item is placed or retrieved from the bagpack
    /// </summary>
    public void RunWobbleAnimation()
    {
        if (_debug) Debug.Log("Running wobble animation");
        _animator.SetTrigger("Wobble");
    }
}
