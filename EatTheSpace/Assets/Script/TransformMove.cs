using System.Collections;
using UnityEngine;

public class TransformMove : MonoBehaviour
{
[Header("Movement Settings")]
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform endTransform;
    [SerializeField] private float duration = 3f;

    void Start()
    {
        if (startTransform == null || endTransform == null) return;

        if (duration <= 0f) return;

        StartCoroutine(MoveOverTime());
    }

    private IEnumerator MoveOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startTransform.position, endTransform.position, t);
            transform.rotation = Quaternion.Slerp(startTransform.rotation, endTransform.rotation, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endTransform.position;
        transform.rotation = endTransform.rotation;
    }
}
