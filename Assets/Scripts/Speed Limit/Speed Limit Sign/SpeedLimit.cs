using System;
using UnityEngine;
using UnityEngine.UI;

public class SpeedLimit : MonoBehaviour
{
    private Array speedLimitValues;

    private Text speedLimitText;

    public static int SpeedLimitValue { get; set; }

    private void Awake()
    {
        SetSpeedLimitTextValue();
    }

    private void SetSpeedLimitTextValue()
    {
        speedLimitText = GetComponentInChildren<Text>();
        speedLimitValues = Enum.GetValues(typeof(SpeedLimitEnum));
        SpeedLimitValue = (int)speedLimitValues.GetValue(UnityEngine.Random.Range(0, speedLimitValues.Length));
        speedLimitText.text = SpeedLimitValue.ToString();
    }
}
