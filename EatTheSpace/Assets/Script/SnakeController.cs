using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class SnakeController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float bodyFollowSpeed = 10f;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float baseCameraOrthographicSize = 5f;
    [SerializeField] private float cameraZoomPerBodyPart = 0.1f;
    [SerializeField] private float maxCameraOrthographicSize = 20f;
    [SerializeField] private float cameraZoomSpeed = 2f;

    [Header("Camera Follow Settings")]
    [SerializeField] private float cameraFollowSpeed = 5f;

    [Header("Body Settings")]
    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private int initialBodyCount = 10;
    [SerializeField] private float distanceBetweenNodes = 0.5f;

    [Header("Body Scaling Settings")]
    [SerializeField] private float baseBodyScale = 1f;
    [SerializeField] private float bodyScalePerBodyPart = 0.05f;
    [SerializeField] private float maxBodyScale = 3f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 2f;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int targetScore = 800;

    [Header("Loop Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int collisionBodyCount = 5;

    private Rigidbody2D rb;
    private Camera mainCam;
    private int currentScore = 0;
    private float currentBodyScale = 1f;
    private bool canTakeDamage = true;
    private GameObject loopColliderObj;
    private List<Vector2> pathHistory = new List<Vector2>();
    private List<Transform> bodyParts = new List<Transform>();

    public static SnakeController Instance { get; private set; }

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        canTakeDamage = true;
    }

    /// <summary>
    /// Initializes the snake and camera settings.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        
        currentBodyScale = baseBodyScale;
        transform.localScale = Vector3.one * currentBodyScale;
        mainCam.orthographicSize = baseCameraOrthographicSize;
        
        mainCam.transform.position = new Vector3(transform.position.x, transform.position.y, mainCam.transform.position.z);

        pathHistory.Add(transform.position);
        bodyParts = new List<Transform>();

        for (int i = 0; i < initialBodyCount; i++)
        {
            AddBodyPart();
        }
        UpdateBodyScales();

        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    /// <summary>
    /// Updates the movement and camera in fixed time steps.
    /// </summary>
    void FixedUpdate()
    {
        MoveHead();
        RecordHistory();
        MoveBody();
        UpdateCameraZoom();
        UpdateCameraPosition();
    }

    /// <summary>
    /// Moves the snake head towards the mouse position.
    /// </summary>
    private void MoveHead()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        Vector2 newPos = Vector2.MoveTowards(rb.position, mouseWorldPos, moveSpeed * Time.fixedDeltaTime);
        Vector2 lookDir = mouseWorldPos - rb.position;
        
        if (lookDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }

        rb.MovePosition(newPos);
    }

    /// <summary>
    /// Records the path history for the body parts to follow.
    /// </summary>
    private void RecordHistory()
    {
        if (Vector2.Distance(transform.position, pathHistory[0]) >= distanceBetweenNodes)
        {
            pathHistory.Insert(0, transform.position);

            if (pathHistory.Count > bodyParts.Count * 2 + 10) 
            {
                pathHistory.RemoveAt(pathHistory.Count - 1);
            }
        }
    }

    /// <summary>
    /// Moves the body parts along the recorded path.
    /// </summary>
    private void MoveBody()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            int targetHistoryIndex = (i + 1); 

            if (targetHistoryIndex < pathHistory.Count)
            {
                Vector2 targetPos = pathHistory[targetHistoryIndex];
                bodyParts[i].position = Vector2.Lerp(bodyParts[i].position, targetPos, bodyFollowSpeed * Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// Updates the camera orthographic size based on the snake's length.
    /// </summary>
    private void UpdateCameraZoom()
    {
        float targetOrthographicSize = baseCameraOrthographicSize + bodyParts.Count * cameraZoomPerBodyPart;
        targetOrthographicSize = Mathf.Min(targetOrthographicSize, maxCameraOrthographicSize);
        mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, targetOrthographicSize, Time.fixedDeltaTime * cameraZoomSpeed);
    }

    /// <summary>
    /// Smoothly moves the camera to follow the snake head.
    /// </summary>
    private void UpdateCameraPosition()
    {
        Vector3 targetCameraPos = new Vector3(transform.position.x, transform.position.y, mainCam.transform.position.z);
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, targetCameraPos, Time.fixedDeltaTime * cameraFollowSpeed);
    }

    /// <summary>
    /// Updates the scale of all body parts based on the total length.
    /// </summary>
    private void UpdateBodyScales()
    {
        float newTargetScale = baseBodyScale + bodyParts.Count * bodyScalePerBodyPart;
        newTargetScale = Mathf.Min(newTargetScale, maxBodyScale);

        if (Mathf.Abs(currentBodyScale - newTargetScale) > 0.001f)
        {
            currentBodyScale = newTargetScale;
            transform.localScale = Vector3.one * currentBodyScale;

            foreach (Transform part in bodyParts)
            {
                part.localScale = Vector3.one * currentBodyScale;
            }
        }
    }

    /// <summary>
    /// Adds a new body part to the snake.
    /// </summary>
    public void AddBodyPart()
    {
        Vector2 spawnPos = transform.position;
        if (bodyParts.Count > 0)
        {
            spawnPos = bodyParts[bodyParts.Count - 1].position;
        }

        GameObject newPart = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
        newPart.GetComponent<BodyNode>().nodeIndex = bodyParts.Count;
        newPart.transform.localScale = Vector3.one * currentBodyScale;
        bodyParts.Add(newPart.transform);
        UpdateBodyScales();
    }

    /// <summary>
    /// Gets the current length of the snake.
    /// </summary>
    public int GetLength()
    {
        return bodyParts.Count;
    }

    /// <summary>
    /// Deducts a specified amount of body parts from the snake.
    /// </summary>
    public void DeductBodyParts(int amount)
    {
        if (!canTakeDamage)
        {
            return;
        }

        canTakeDamage = false;
        StartCoroutine(DamageCooldownCoroutine());

        for (int i = 0; i < amount; i++)
        {
            if (bodyParts.Count > 0)
            {
                Transform lastPart = bodyParts[bodyParts.Count - 1];
                bodyParts.RemoveAt(bodyParts.Count - 1);
                Destroy(lastPart.gameObject);
            }
            else
            {
                break;
            }
        }
        
        UpdateBodyScales();
    }

    /// <summary>
    /// Adds score and loads the end scene if the target is reached.
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }

        if (currentScore >= targetScore)
        {
            SceneManager.LoadScene("EndScene");
        }
    }

    /// <summary>
    /// Handles the damage cooldown timer.
    /// </summary>
    private IEnumerator DamageCooldownCoroutine()
    {
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    /// <summary>
    /// Detects self-collision to create a closed loop.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        BodyNode hitNode = collision.GetComponent<BodyNode>();
        
        if (hitNode != null && hitNode.nodeIndex > collisionBodyCount) 
        {
            CreateCollapseArea(hitNode.nodeIndex);
        }
    }

    /// <summary>
    /// Creates a polygon collider representing the enclosed area.
    /// </summary>
    private void CreateCollapseArea(int hitIndex)
    {
        List<Vector2> loopPoints = new List<Vector2>();
        
        for (int i = 0; i <= hitIndex; i++)
        {
            loopPoints.Add(bodyParts[i].position);
        }
        loopPoints.Add(transform.position);

        if (loopColliderObj == null) 
        {
            loopColliderObj = new GameObject("TempLoopCollider");
            loopColliderObj.AddComponent<PolygonCollider2D>().isTrigger = true;
        }
        
        PolygonCollider2D poly = loopColliderObj.GetComponent<PolygonCollider2D>();
        poly.points = loopPoints.ToArray();

        CheckEnemiesInLoop(poly);
    }

    /// <summary>
    /// Checks for and absorbs enemies within the created loop.
    /// </summary>
    private void CheckEnemiesInLoop(PolygonCollider2D poly)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useLayerMask = true;

        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCollider(poly, filter, results);

        foreach (var enemy in results)
        {
            enemy.gameObject.SetActive(false); 
            
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            int lengthToAdd = enemyScript != null ? enemyScript.LengthGrantedOnDeath : 1;
            
            for (int i = 0; i < lengthToAdd; i++)
            {
                AddBodyPart(); 
            }
            
            AddScore(lengthToAdd * 10);
        }
    }
}
