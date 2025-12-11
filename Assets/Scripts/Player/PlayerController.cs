using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Math = System.Math;

public class PlayerController : MonoBehaviour
{
    public enum PlayerMode
    {
        Play,
        Spectate
    }
    
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider coll;
    [SerializeField] private float baseMoveSpeed = 8f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float staminaRechargeRate = 0.1f;
    [SerializeField] private float staminaDischargeRate = 0.2f;
    [SerializeField] private float exhaustionThreshold = 0.1f;
    [SerializeField] private float exhaustionEndThreshold = 0.5f;
    [SerializeField] private int deathCooldownTime = 5;
    [SerializeField] private Vector3 spawnPosition;
    // [SerializeField] private float jumpStrength = 5;
    [SerializeField] private float horizontalDampingFactor = 5f;

    private CameraController _cam;
    private PlayerNetwork _network;
    private NetworkTransform _networkTransform;
    private float _stamina = 1f;
    private float _currentMoveSpeed;
    private int _spectateIndex = 0;
    
    public MouseHole CurrentHole { get; set; }
    public PlayerMode CurrentMode { get; set; } = PlayerMode.Play;

    private float Stamina
    {
        get => _stamina;
        set
        {
            _stamina = value;
            _cam.UpdateStaminaDisplay(value, _network.exhausted.Value);
        }
    }

    private static bool Run => Input.GetKey(KeyCode.LeftShift);

    public void Init(PlayerNetwork playerNetwork, NetworkTransform networkTransform)
    {
        _cam = CameraController.instance;
        CameraController.instance.LookAt(transform);
        transform.position = spawnPosition;
        _currentMoveSpeed = baseMoveSpeed;
        _network = playerNetwork;
        _networkTransform = networkTransform;
    }

    public void Update()
    {
        if (!_cam || rb.isKinematic) return;

        switch (CurrentMode)
        {
            case PlayerMode.Play:
                PlayUpdate();
                break;
            case PlayerMode.Spectate:
                SpectateUpdate();
                break;
        }
    }

    private void PlayUpdate()
    {
        // Movement.
        Vector3 moveInput = Vector3.zero;
        
        if (_network.dead.Value)
        {
            _network.exhausted.Value = false;
        }
        else
        {
            #region Horizontal Movement
            
            bool isRunning;

            if (Stamina >= exhaustionThreshold)
            {
                if (Stamina >= exhaustionEndThreshold)
                {
                    _network.exhausted.Value = false;
                }

                isRunning = Run && !_network.exhausted.Value;
            }
            else
            {
                _network.exhausted.Value = true;
                isRunning = false;
            }
            
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            moveInput = verticalInput * _cam.transform.forward + horizontalInput * _cam.transform.right;
            moveInput.y = 0;
            moveInput = Vector3.ClampMagnitude(moveInput, 1);

            float speed = _currentMoveSpeed * (isRunning ? runMultiplier : 1f);

            Vector3 velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (velocity.magnitude < speed * 0.9f)
            {
                rb.AddForce(moveInput * (speed * 2f), ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(moveInput * speed, ForceMode.Acceleration);
            }

            if (moveInput != Vector3.zero)
            {
                rb.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveInput),
                    Time.deltaTime * rotationSpeed);
            }

            Stamina = isRunning && moveInput != Vector3.zero
                ? Math.Max(Stamina - staminaDischargeRate * Time.deltaTime, 0f)
                : Math.Min(Stamina + staminaRechargeRate * Time.deltaTime, 1f);
            
            #endregion
            
            // #region Vertical Movement
            //
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            // }
            //
            // #endregion
            
            Vector3 v = rb.linearVelocity;

            float y = v.y;

            // Damp horizontal movement.
            Vector3 horizontal = new Vector3(v.x, 0, v.z);
            horizontal *= horizontalDampingFactor;

            rb.linearVelocity = horizontal + Vector3.up * y;
        }
        
        // Enter mouse hole.
        if (Input.GetKeyDown(KeyCode.E))
        {
            CurrentHole?.Enter(this);
        }

        _network.UpdateMovementData(new PlayerMovementData
        {
            direction = moveInput,
            velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude
        });
    }

    private void SpectateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeSpectatePlayer(-1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeSpectatePlayer(1);
        }
    }

    public void ChangeSpectatePlayer(int increase)
    {
        IReadOnlyList<NetworkClient> clients = NetworkManager.Singleton.ConnectedClientsList;
        
        _spectateIndex = (_spectateIndex + increase + clients.Count) % clients.Count;
        
        while (clients[_spectateIndex].PlayerObject.GetComponent<PlayerNetwork>().lost.Value)
        {
            _spectateIndex = (_spectateIndex + increase + clients.Count) % clients.Count;
        }
        
        CameraController.instance.LookAt(clients[_spectateIndex].PlayerObject.transform);
    }

    public void Die(int lifeCount)
    {
        ResetOnDeath();

        if (lifeCount > 0)
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            _cam.SetTitle("You lost.");
            _cam.SetSubtitle("Spectating.");
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        _network.dead.Value = true;
        EnableController(false);
        
        for (int cooldown = deathCooldownTime; cooldown > 0; cooldown -= 1)
        {
            _cam.SetTitle("You died.");
            _cam.SetSubtitle("Respawning in: " + cooldown);
            if (cooldown == 1)
            {
                EnableController(false);
                Teleport(spawnPosition);
            }
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(Respawn());
    }

    public void Teleport(Vector3 position)
    {
        _networkTransform.Teleport(position, Quaternion.identity, transform.localScale);
    }

    private IEnumerator Respawn()
    {
        _network.dead.Value = false;
        EnableController(false);
        Teleport(spawnPosition);
        yield return null;
        EnableController(true);
        _network.OnRespawnServerRpc();
    }

    public void EnableController(bool value)
    {
        rb.isKinematic = !value;
        coll.enabled = value;
    }

    public void PickUpCollectible(NetworkObjectReference collectible, Collectible.CollectibleType type)
    {
        _network.PickUpCollectible(collectible, type);

        if (type == Collectible.CollectibleType.Cheese)
        {
            ResetStamina();
            _currentMoveSpeed *= 0.9f;
            transform.localScale += Vector3.one * 0.15f;
        }
    }

    public void ResetOnDeath()
    {
        ResetStamina();
        
        // Reset mouse size.
        transform.localScale = Vector3.one;
        _currentMoveSpeed = baseMoveSpeed;
    }

    private void ResetStamina()
    {
        _network.exhausted.Value = false;
        Stamina = 1f;
    }
}