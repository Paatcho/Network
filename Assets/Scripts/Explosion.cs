using System;
using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float animSpeed;

    private void Start()
    {
        StartCoroutine(Explode());
    }

    private void Update()
    {
        transform.LookAt(CameraController.Instance.transform);
    }

    private IEnumerator Explode()
    {
        foreach (Sprite sprite in sprites)
        {
            sr.sprite = sprite;
            yield return new WaitForSeconds(animSpeed);
        }

        Destroy(gameObject);
    }
}
