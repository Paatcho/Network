using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CrushedMouse : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;

    private void Start()
    {
        Vector3 rotation = sprite.transform.localRotation.eulerAngles;

        rotation.z = Random.Range(0f, 360f);

        sprite.transform.localRotation = Quaternion.Euler(rotation);

        StartCoroutine(DisappearCoroutine());

        TweenBounce();
    }

    private IEnumerator DisappearCoroutine()
    {
        yield return new WaitForSeconds(2f);
        
        for (int i = 0; i < 10; i++)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        
        Destroy(gameObject);
    }
    
    private void TweenBounce(float squishDuration = 0.1f)
    {
        Sequence seq = DOTween.Sequence();

        Vector3 squishScale = new Vector3(1.7f, 1f, 1.7f);

        seq.Append(
            transform.DOScale(squishScale, squishDuration).SetEase(Ease.OutQuad)
        );

        seq.Play();
    }
}