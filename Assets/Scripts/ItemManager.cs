using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ItemManager : MonoBehaviour
{
    // Server connection data
    private string _serverUrl = "https://wadahub.manerai.com/api/inventory/status";
    private string _authorizationToken = "kPERnYcWAY46xaSy8CEzanosAgsWM84Nx7SKM4QBSqPq6c7StWfGxzhxPfDh8MaP";
    // Events to notify when an item is folded or retrieved
    [HideInInspector] public UnityEvent<Item> onItemFolded = new UnityEvent<Item>();
    [HideInInspector] public UnityEvent<Item> onItemRetrieved = new UnityEvent<Item>();

    [Tooltip("Toggles debug mode")]
    [SerializeField] private bool _debug = false;
    [Header("UI Properties")]
    [Tooltip("Name of the layer where the UI elements will be placed")]
    [SerializeField] private string _UILayerName = "UI";
    [Tooltip("Parent object of the item slots")]
    [SerializeField] private GameObject _itemsLayout;
    [Tooltip("Prefab of the item slot")]
    [SerializeField] private GameObject itemSlotPrefab;
    [Tooltip("List of the item layout types")]
    [SerializeField] private List<GameObject> _itemsLayoutTypes = new();

    [Tooltip("Must add as itemslayoutpositions as objet types in scene. Three object types for this project")]
    public List<GameObject> itemsLayoutPositions = new();
    [Tooltip("Number of available items. Each Row will be filled according to available items")]
    [SerializeField] private int availableItems = 5;

    [Header("BagPack Properties")]
    [Tooltip("BagPack object")]
    [SerializeField] private GameObject _bagPack;
    [Tooltip("BagPack component")]
    [SerializeField] private BagPack _bagPackComponent;
    [Tooltip("Duration of the folding animation")]
    [SerializeField] private float _animationDuration = 0.8f;

    [Header("Item dragging properties")]
    [Tooltip("Item being retrieved")]
    public GameObject retrievingItem;
    [Tooltip("Indicates if an item is being dragged")]
    public bool isDragging = false;

    private void Start()
    {
        FillItemSlots();
        _bagPack = GameObject.Find("BagPack");
        _bagPackComponent = _bagPack.GetComponent<BagPack>();
    }

    /// <summary>
    /// Fill the item slots according to the available items.
    /// Each row will be filled with the available items.
    /// </summary>
    private void FillItemSlots()
    {
        itemsLayoutPositions.Clear();

        foreach (GameObject layoutRow in _itemsLayoutTypes)
        {
            for (int i = 0; i < availableItems; i++)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, layoutRow.transform);
                itemsLayoutPositions.Add(itemSlot);
            }
        }

        if (_debug) Debug.Log($"Se han generado {itemsLayoutPositions.Count} slots en {_itemsLayoutTypes.Count} filas.");
    }

    /// <summary>
    /// Add an item to the first available slot according to the item type.
    /// </summary>
    /// <param name="item">GameObject Item to be stored</param>
    public void AddItemToFreeSlot(GameObject item)
    {
        Item itemComponent = item.GetComponent<Item>();

        // Check if the item has the Item component
        if (itemComponent == null)
        {
            Debug.LogWarning("El objeto no tiene un componente Item.");
            return;
        }
        // Check if the BagPack component is available
        if (_bagPackComponent == null)
        {
            Debug.LogWarning("BagPack component not found.");
            return;
        }
        else
        {
            _bagPackComponent.RunWobbleAnimation();
        }
        /*
        itemComponent.InitFoldingAnimation();
        StartCoroutine(WaitforANimation());
        */

        // Get the layout index according to the item type
        int layoutIndex = (int)itemComponent.type;
        if (layoutIndex < 0 || layoutIndex >= _itemsLayoutTypes.Count)
        {
            Debug.LogWarning($"No hay una fila asignada para el tipo {itemComponent.type}");
            return;
        }

        Transform layoutRow = _itemsLayoutTypes[layoutIndex].transform;

        // Check if there is an available slot in the row
        foreach (Transform slot in layoutRow)
        {
            ItemSlot slotComponent = slot.GetComponent<ItemSlot>();
            if (slotComponent != null && !slotComponent.isOccupied)
            {
                slotComponent.SetOccupied(true, item);

                // Set item properties
                item.GetComponent<Rigidbody>().useGravity = false;
                item.GetComponent<Rigidbody>().isKinematic = true;
                item.GetComponent<SphereCollider>().enabled = false;
                item.gameObject.layer = LayerMask.NameToLayer(_UILayerName);
                item.transform.position = slot.position;
                item.transform.SetParent(slot);

                // Fold the item
                FoldItem(itemComponent);

                if (_debug) Debug.Log($"{item.gameObject.name} agregado a {layoutRow.name}");
                return;
            }
        }

        Debug.LogWarning($"No hay slots disponibles en la fila {layoutRow.name}");
    }

    private IEnumerator<WaitForSeconds> WaitforANimation()
    {
        yield return new WaitForSeconds(_animationDuration);
    }

    /// <summary>
    /// Update the item being retrieved
    /// </summary>
    /// <param name="item">Item to be retrieved</param>
    public void UpdateRetrievingItem(GameObject item)
    {
        retrievingItem = item;
        if (_debug) Debug.Log("Setting retrieving item as: " + retrievingItem.name);
    }

    /// <summary>
    /// Toggle the items layout visibility
    /// </summary>
    /// <param name="show">Set visibility on/off</param>
    public void ToggleItemsLayout(bool show)
    {
        _itemsLayout.gameObject.SetActive(show);
    }

    /// <summary>
    /// Send folding event to the server
    /// </summary>
    /// <param name="item">Item to be fold</param>
    public void FoldItem(Item item)
    {
        StartCoroutine(SendItemEvent("folding", item));
        onItemFolded?.Invoke(item);
    }

    /// <summary>
    /// Send retrieving event to the server
    /// </summary>
    /// <param name="item"></param>
    public void RetrieveItem(Item item)
    {
        StartCoroutine(SendItemEvent("retrieving", item));
        onItemRetrieved?.Invoke(item);
    }

    /// <summary>
    /// Send item event to the server
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private IEnumerator SendItemEvent(string eventType, Item item)
    {
        if (item == null)
        {
            Debug.LogError("Not possible tu send null item");
            yield break;
        }

        // Crear JSON con datos del item
        string jsonData = JsonUtility.ToJson(new ItemData(eventType, item.type.ToString(), item.gameObject.GetInstanceID()));

        using (UnityWebRequest request = new UnityWebRequest(_serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + _authorizationToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"{eventType} SUCCESS: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"ERROR {eventType}: {request.error}");
            }
        }
    }

    /// <summary>
    /// Data class to store item information
    /// </summary>
    private class ItemData
    {
        public string eventType;
        public string itemType;
        public int itemID;

        public ItemData(string eventType, string itemType, int itemID)
        {
            this.eventType = eventType;
            this.itemType = itemType;
            this.itemID = itemID;
        }
    }
}
