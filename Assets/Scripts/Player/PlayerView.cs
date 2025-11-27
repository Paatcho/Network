using System;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerView : MonoBehaviour
{
    private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
    private const int RotationSplit = 8;
    private const int FullRotation = 360;
    private const float AnimTimerMax = 0.4f;
    private const float RotationSection = FullRotation / (float)RotationSplit;

    [SerializeField] private Texture2D normalMap;
    [SerializeField] private Texture2D invertedNormalMap;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem exhaustionParticleSystem;
    [SerializeField] private CrushedMouse crushedPrefab;
    [SerializeField] private Explosion explosionPrefab;
    
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Vector3 walkAnimationPosition = new(0, 0.05f);
    
    private Camera _camera;
    private bool _animUp;
    private float _animTimer;

    private void Start()
    {
        _camera = Camera.main;
        transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        bool yInverted = Vector3.Angle(transform.forward, Vector3.forward) <= 90;
        
        spriteRenderer.material.SetTexture(BumpMap, yInverted ? invertedNormalMap : normalMap);
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
        transform.localPosition = _animUp ? walkAnimationPosition : Vector3.zero;
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

    public void Die(PlayerNetwork.DeathType deathType)
    {
        switch (deathType)
        {
            case PlayerNetwork.DeathType.Crushed:
                Instantiate(crushedPrefab, transform.position + Vector3.down * 0.24f, Quaternion.identity);
                break;
            case PlayerNetwork.DeathType.Explosion:
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                break;
            case PlayerNetwork.DeathType.Default:
                break;
        }
    }

    public void OnRespawn()
    {
        
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

        Sprite result;
        
        try
        {
            result = sprites[index];
        }
        catch
        {
            Debug.LogError("Invalid sprite index : " + index);
            Debug.LogError(yaw);
            throw;
        }
        
        return result;
    }
}
