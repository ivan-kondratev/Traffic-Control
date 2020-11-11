using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResources : MonoBehaviour
{
    public static int score;
    public static int health = 3;

    public GameObject[] hearts;
    public Text scoreText;

    public int LastHeartIndex { get; set; }

    private void Awake()
    {
        LastHeartIndex = hearts.Length - 1;
    }

    public static void ResetResources()
    {
        score = 0;
        health = 3;
    }
}
