using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Pong : MonoBehaviour
{
    [Serializable]
    private class PlayerProfile
    {
        public Image paddle;
        public TMP_Text scoreDisplay;
        public KeyCode upKey;
        public KeyCode downKey;
        public int score;
        
        public float PaddleRadius => paddle.rectTransform.rect.width / 2;
        public float PaddleHeight => paddle.rectTransform.rect.height / 2;
    }
    
    [SerializeField] private Image ball;
    [SerializeField] private float ballSpeed;
    [SerializeField] private float paddleSpeed;
    [SerializeField] private PlayerProfile[] players;
    [SerializeField] private float ballXMinPos;
    [SerializeField] private float ballXMaxPos;
    [SerializeField] private float ballYMinPos;
    [SerializeField] private float ballYMaxPos;
    [SerializeField] private float paddleYMinPos;
    [SerializeField] private float paddleYMaxPos;
    [SerializeField] private float ballAccelerateDelta = 1.01f;

    private Vector2 _ballDirection;
    private float _ballSpeed;
    
    private float BallRadius => ball.rectTransform.rect.width / 2;
    
    private void Start()
    {
        _ballDirection = new Vector2(1, -1).normalized;
        _ballSpeed = ballSpeed;
    }
    
    private void Update()
    {
        PlayerInputs();
        _ballSpeed *= ballAccelerateDelta;
    }

    private void FixedUpdate()
    {
        ball.rectTransform.localPosition += (Vector3)_ballDirection * _ballSpeed;
        CheckBallCollision();
    }

    private void PlayerInputs()
    {
        foreach (PlayerProfile player in players)
        {
            if (Input.GetKey(player.upKey))
            {
                player.paddle.rectTransform.localPosition += Vector3.up * paddleSpeed;
            }
            if (Input.GetKey(player.downKey))
            {
                player.paddle.rectTransform.localPosition += Vector3.down * paddleSpeed;
            }
            
            player.paddle.rectTransform.localPosition =
                new Vector2(
                    player.paddle.rectTransform.localPosition.x,
                    Mathf.Clamp(player.paddle.rectTransform.localPosition.y, paddleYMinPos, paddleYMaxPos));
        }
    }

    private void CheckBallCollision()
    {
        CheckBallWallCollision();
        CheckBallPaddleCollision();
        CheckBallDeathCollision();
    }
    
    private void CheckBallWallCollision()
    {
        if (ball.rectTransform.localPosition.y > ballYMaxPos || ball.rectTransform.localPosition.y < ballYMinPos)
        {
            BounceBallY();
        }
    }

    private void CheckBallPaddleCollision()
    {
        if (_ballDirection.x < 0 && Mathf.Abs(ball.rectTransform.localPosition.x - players[0].paddle.rectTransform.localPosition.x) < players[0].PaddleRadius + BallRadius)
        {
            if (Mathf.Abs(ball.rectTransform.localPosition.y - players[0].paddle.rectTransform.localPosition.y)
                < players[0].PaddleHeight)
            {
                BounceBallX();
            }
        }
        else if (_ballDirection.x > 0 &&Mathf.Abs(ball.rectTransform.localPosition.x - players[1].paddle.rectTransform.localPosition.x) < players[1].PaddleRadius + BallRadius)
        {
            if (Mathf.Abs(ball.rectTransform.localPosition.y - players[1].paddle.rectTransform.localPosition.y)
                < players[1].PaddleHeight)
            {
                BounceBallX();
            }
        }
    }

    private void BounceBallX()
    {
        _ballDirection = new Vector2(-_ballDirection.x, _ballDirection.y);
    }

    private void BounceBallY()
    {
        _ballDirection = new Vector2(_ballDirection.x, -_ballDirection.y);
    }

    private void CheckBallDeathCollision()
    {
        if (ball.rectTransform.localPosition.x > ballXMaxPos)
        {
            RespawnBall();
            players[0].score++;
            players[0].scoreDisplay.text = players[0].score.ToString();
        }
        else if (ball.rectTransform.localPosition.x < ballXMinPos)
        {
            RespawnBall();
            players[1].score++;
            players[1].scoreDisplay.text = players[1].score.ToString();
        }
    }

    private void RespawnBall()
    {
        ball.rectTransform.localPosition = Vector2.zero;
        _ballSpeed = ballSpeed;
    }
}
