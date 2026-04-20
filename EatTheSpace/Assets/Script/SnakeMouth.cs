using UnityEngine;

public class SnakeMouth : MonoBehaviour
{
    /// <summary>
    /// Verifies collider setup on start.
    /// </summary>
    void Start()
    {
        Collider2D mouthCollider = GetComponent<Collider2D>();
        if (mouthCollider == null)
        {
            enabled = false;
        }
    }

    /// <summary>
    /// Eats the enemy on trigger enter.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.EatenByPlayer();
        }
    }
}
