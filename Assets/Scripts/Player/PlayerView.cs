using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private List<PlayerAnimation> winAnimations;
    [SerializeField] private SpriteRenderer winSpriteRenderer;
    
    private PlayerNetwork _network;
    private Camera _camera;
    private bool _animUp;
    private float _animTimer;

    public int AnimCount => winAnimations.Count;
    
    public void Init(PlayerNetwork network)
    {
        _camera = Camera.main;
        transform.localPosition = Vector3.zero;
        _network = network;
    }

    private void Update()
    {
        if (_network.win.Value) return;
        
        bool yInverted = Vector3.Angle(transform.forward, Vector3.forward) <= 90;
        
        UpdateDirection(_network.movementData.Value.direction);
        UpdateView(_network.movementData.Value.velocity);
        
        spriteRenderer.material.SetTexture(BumpMap, yInverted ? invertedNormalMap : normalMap);
    }

    public void LaunchWinAnimation()
    {
        spriteRenderer.enabled = false;
        winSpriteRenderer.enabled = true;
        winSpriteRenderer.transform.localPosition += Vector3.up * winAnimations[_network.winAnimIndex.Value].height;
        StartCoroutine(WinAnimation());
    }
    
    private IEnumerator WinAnimation()
    {
        while (true)
        {
            foreach (Sprite sprite in winAnimations[_network.winAnimIndex.Value].sprites)
            {
                winSpriteRenderer.sprite = sprite;
                yield return new WaitForSeconds(winAnimations[_network.winAnimIndex.Value].spriteTime);
            }
        }
    }

    private void UpdateView(float velocity)
    {
        _animTimer += velocity * Time.deltaTime;
        
        if (_animTimer > AnimTimerMax)
        {
            _animTimer = 0f;
            _animUp = !_animUp;
            transform.localPosition = _animUp ? walkAnimationPosition : Vector3.zero;
        }

        if (velocity == 0f)
        {
            _animUp = false;
            transform.localPosition = Vector3.zero;
        }
    }

    private void UpdateDirection(Vector3 direction)
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
        spriteRenderer.enabled = false;
        
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
        spriteRenderer.enabled = true;
    }

    #region Direction
    
    private Sprite DirectionToSprite(Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        float yaw = rotation.eulerAngles.y;
        yaw -= _camera.transform.rotation.eulerAngles.y;

        Sprite sprite = YawToSprite(yaw);
        
        return sprite ? sprite : spriteRenderer.sprite;
    }

    private Sprite YawToSprite(float yaw)
    {
        int index = (int)(Mathf.Repeat(yaw + RotationSection / 2f, FullRotation) / RotationSection);

        return index < sprites.Length ? sprites[index] : null;
    }
    
    #endregion
}
