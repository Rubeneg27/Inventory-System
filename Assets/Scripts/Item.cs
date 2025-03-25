using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private bool _debug = false;
    public enum ItemType
    {
        A,
        B,
        C
    }
    public float weight;
    public string itenName;
    public string identifier;
    public ItemType type;

    [Tooltip("This is the location where the item will be placed when folding animation is triggered")]
    [SerializeField] private Transform _bagPackFoldingLocation;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// This method will trigger the folding animation of the item
    /// </summary>
    public void InitFoldingAnimation()
    {
        if (_bagPackFoldingLocation == null)
        {
            if (_debug) Debug.LogWarning("Animation location no found");
            return;
        }
        transform.SetParent(_bagPackFoldingLocation);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _animator.SetTrigger("Fold");

        if (_debug) Debug.Log("Folding animation started");
    }
}
