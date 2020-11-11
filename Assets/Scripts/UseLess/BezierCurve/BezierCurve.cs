using UnityEngine;

public class BezierCurve : MonoBehaviour
{
	public Vector3[] points;

	public void Reset()
	{
		points = new Vector3[] {
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 1f, 2f),
			new Vector3(0f, 0f, 3f),
			new Vector3(0f, -1f, 4f)
		};
	}

	public Vector3 GetPoint(float t)
	{
		return transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], t));
	}

	public Vector3 GetVelocity(float t)
	{
		return transform.TransformPoint(
			Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], t)) - transform.position;
	}

	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}
}
