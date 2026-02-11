using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static event EventHandler OnGameWin;

    public static void GameWin()
    {
        OnGameWin?.Invoke(null, EventArgs.Empty);
    }
}
