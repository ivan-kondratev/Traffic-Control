using UnityEngine;

public class PolicePath : Path
{
    [SerializeField]
    private int changePointIndex, stopPointIndex;

    public bool Occupied { get; set; }

    public int ChangePointIndex {
        get => changePointIndex;
        set => changePointIndex = value;
    }

    public int StopPointIndex {
        get => stopPointIndex;
        set => stopPointIndex = value;
    }

    public Vector3 ChangePoint => GetControlPoint(ChangePointIndex);

    public Vector3 StopPoint => GetControlPoint(StopPointIndex);
}
