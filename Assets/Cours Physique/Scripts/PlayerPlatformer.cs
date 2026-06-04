using System;
using UnityEngine;

public class PlayerPlatformer : MonoBehaviour
{
    [SerializeField] private float minY;
    [SerializeField] private KeyCode leftKey;
    [SerializeField] private KeyCode rightKey;
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float jumpAttenuation = 5f;

    private float _velocityX;
    private float _velocityY;
    private bool _onGround = true;

    private void Update()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            if (_onGround)
            {
                _velocityY = jumpVelocity;
                _onGround = false;
            }
        }

        if (Input.GetKeyUp(jumpKey))
        {
            if (_velocityY > 0)
            {
                _velocityY /= jumpAttenuation;
            }
        }

        if (Input.GetKey(leftKey))
        {
            _velocityX = -horizontalSpeed;
        }
        else if (Input.GetKey(rightKey))
        {
            _velocityX = horizontalSpeed;
        }
        else
        {
            _velocityX = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!_onGround)
        {
            _velocityY -= gravityStrength * Time.fixedDeltaTime;
            
            if (transform.position.y < minY)
            {
                _velocityY = 0f;
                _onGround = true;
                transform.position = new Vector3(transform.position.x, minY, 0);
            }
        }
        
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        transform.localPosition = new Vector3(
            transform.localPosition.x + _velocityX * Time.fixedDeltaTime,
            transform.localPosition.y + _velocityY * Time.fixedDeltaTime,
            0f);
    }
}