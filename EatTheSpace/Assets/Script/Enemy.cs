using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float minInitialSize = 0.5f;
    [SerializeField] private float maxInitialSize = 1.5f;
    [SerializeField] private int lengthGrantedOnDeath = 1;
    [SerializeField] private int lengthPenaltyOnCollision = 1;

    [Header("Movement")]
    [SerializeField] private Vector2 minMoveDirection = new Vector2(-1f, -1f);
    [SerializeField] private Vector2 maxMoveDirection = new Vector2(1f, 1f);
    [SerializeField] private float minMoveSpeed = 1f;
    [SerializeField] private float maxMoveSpeed = 3f;

    [Header("Rotation")]
    [SerializeField] private float minRotationDirection = -1f;
    [SerializeField] private float maxRotationDirection = 1f;
    [SerializeField] private float minRotationSpeed = 15f;
    [SerializeField] private float maxRotationSpeed = 60f;

    [Header("Despawn Settings")]
    [SerializeField] private float despawnDistance = 50f;

    [Header("Spawn Settings")]
    [SerializeField] private float outsideSpawnMargin = 2f;

    [Header("Length Requirements")]
    [SerializeField] private int lengthToSpawn = 0;
    [SerializeField] private int lengthToEat = 0;

    private Transform mainCamTransform;
    private Rigidbody2D rb;

    public float OutsideSpawnMargin => outsideSpawnMargin;
    public int LengthToSpawn => lengthToSpawn;
    public int LengthGrantedOnDeath => lengthGrantedOnDeath;
    public int LengthPenaltyOnCollision => lengthPenaltyOnCollision;

    /// <summary>
    /// Initializes component references.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }
    }

    /// <summary>
    /// Resets enemy state, size, and velocities when spawned.
    /// </summary>
    void OnEnable()
    {
        if (rb == null) return;

        rb.gravityScale = 0f; 
        float size = Random.Range(minInitialSize, maxInitialSize);
        transform.localScale = Vector3.one * size;
        
        Vector2 moveDirection = new Vector2(Random.Range(minMoveDirection.x, maxMoveDirection.x), Random.Range(minMoveDirection.y, maxMoveDirection.y));
        if (moveDirection != Vector2.zero)
        {
            moveDirection.Normalize();
        }

        float moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        float rotationDirection = Random.Range(minRotationDirection, maxRotationDirection) >= 0 ? 1f : -1f;
        float rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

        rb.linearVelocity = moveDirection * moveSpeed;
        rb.angularVelocity = rotationDirection * rotationSpeed;
    }

    /// <summary>
    /// Handles distance checks for despawning.
    /// </summary>
    void FixedUpdate()
    {
        if (rb == null) return;

        if (mainCamTransform != null && (transform.position - mainCamTransform.position).sqrMagnitude > despawnDistance * despawnDistance)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the logic when the enemy is eaten by the player.
    /// </summary>
    public void EatenByPlayer()
    {
        if (SnakeController.Instance == null) return;

        if (SnakeController.Instance.GetLength() < lengthToEat)
        {
            return;
        }

        for (int i = 0; i < lengthGrantedOnDeath; i++)
        {
            SnakeController.Instance.AddBodyPart();
        }
        SnakeController.Instance.AddScore(lengthGrantedOnDeath * 10);
        
        gameObject.SetActive(false);
    }
}
