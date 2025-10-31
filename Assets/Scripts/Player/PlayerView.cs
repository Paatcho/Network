using System;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private const int RotationSplit = 8;
    private const int FullRotation = 360;
    private const float AnimTimerMax = 0.3f;
    private const float RotationSection = FullRotation / (float)RotationSplit;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem exhaustionParticleSystem;
    
    [SerializeField] private Sprite[] sprites;
    
    private Camera _camera;

    private bool _animUp;
    private float _animTimer;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(_camera.transform);
    }

    public void UpdateView(float velocity)
    {
        _animTimer += velocity * Time.deltaTime;
        if (_animTimer > AnimTimerMax)
        {
            _animTimer = 0f;
            _animUp = !_animUp;
            SetPosition();
        }

        if (velocity == 0f)
        {
            _animUp = false;
            SetPosition();
        }
    }
    
    private void SetPosition()
    {
        transform.localPosition = _animUp ? new Vector3(0f, 0.1f) : Vector3.zero;
    }

    public void UpdateDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        
        spriteRenderer.sprite = DirectionToSprite(direction);
    }

    public void SetExhausted(bool value)
    {
        exhaustionParticleSystem.gameObject.SetActive(value);
    }

    private Sprite DirectionToSprite(Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        float yaw = rotation.eulerAngles.y;
        yaw -= _camera.transform.rotation.eulerAngles.y;
        return YawToSprite(yaw);
    }

    private Sprite YawToSprite(float yaw)
    {
        int index = (int)(Mathf.Repeat(yaw + RotationSection / 2f, FullRotation) / RotationSection);
        
        return sprites[index];
    }
}
