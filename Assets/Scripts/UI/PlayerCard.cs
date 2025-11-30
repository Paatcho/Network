using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerCheeses;
    [SerializeField] private TMP_Text playerLives;

    public void Init(string initName, int initCheeses, int initLives)
    {
        playerName.text = initName;
        UpdatePlayerCheeses(initCheeses);
        UpdatePlayerLives(initLives);
    }

    public void UpdatePlayerCheeses(int newValue)
    {
        playerCheeses.text = newValue.ToString();
    }

    public void UpdatePlayerLives(int newValue)
    {
        playerLives.text = newValue.ToString();
    }
}
