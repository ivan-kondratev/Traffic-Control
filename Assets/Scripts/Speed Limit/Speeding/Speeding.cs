using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speeding : MonoBehaviour
{
    private Array speedingValues;

    public static int SpeedingValue { get; set; }

    private Text speedingText;

    private void Awake()
    {
        SetSpeedingValue();
    }

    private void SetSpeedingValue()
    {
        speedingText = GetComponent<Text>();
        speedingValues = Enum.GetValues(typeof(SpeedingEnum));
        SpeedingValue = (int)speedingValues.GetValue(UnityEngine.Random.Range(0, speedingValues.Length));
        speedingText.text = "speeding: " + SpeedingValue;
    }
}
