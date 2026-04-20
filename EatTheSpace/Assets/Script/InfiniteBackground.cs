using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    [SerializeField] private Vector2 tileSize = new Vector2(10f, 10f);

    private Transform camTransform;

    /// <summary>
    /// Initializes the camera reference.
    /// </summary>
    void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    /// <summary>
    /// Snaps the background to the camera position based on the tile size.
    /// </summary>
    void LateUpdate()
    {
        if (camTransform == null) return;

        float snapX = Mathf.Round(camTransform.position.x / tileSize.x) * tileSize.x;
        float snapY = Mathf.Round(camTransform.position.y / tileSize.y) * tileSize.y;

        transform.position = new Vector3(snapX, snapY, transform.position.z);
    }
}
