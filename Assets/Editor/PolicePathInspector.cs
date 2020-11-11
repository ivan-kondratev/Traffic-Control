using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolicePath))]
public class PolicePathInspector : PathInspector
{
    /// <summary>
    /// Police Path Spline
    /// </summary>
    private PolicePath splinePP;

    private new void OnSceneGUI()
    {
        splinePP = target as PolicePath;
        handleTransform = splinePP.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < splinePP.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
        ShowDirections();
    }

    public override void OnInspectorGUI()
    {
        splinePP = target as PolicePath;
        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", splinePP.Loop);
        int changePointIndex = EditorGUILayout.IntField("Change Point Index", splinePP.ChangePointIndex);
        int stopPointIndex = EditorGUILayout.IntField("Stop Point Index", splinePP.StopPointIndex);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(splinePP, "Toggle Loop");
            EditorUtility.SetDirty(splinePP);
            splinePP.Loop = loop;

            Undo.RecordObject(splinePP, "IntField Change Point Index");
            EditorUtility.SetDirty(splinePP);
            splinePP.ChangePointIndex = changePointIndex;

            Undo.RecordObject(splinePP, "IntField Stop Point Index");
            EditorUtility.SetDirty(splinePP);
            splinePP.StopPointIndex = stopPointIndex;
        }
        if (selectedIndex >= 0 && selectedIndex < splinePP.ControlPointCount)
        {
            DrawSelectedPointInspector();
        }
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(splinePP, "Add Curve");
            splinePP.AddCurve();
            EditorUtility.SetDirty(splinePP);
        }
    }

    private new void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", splinePP.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(splinePP, "Move Point");
            EditorUtility.SetDirty(splinePP);
            splinePP.SetControlPoint(selectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)
            EditorGUILayout.EnumPopup("Mode", splinePP.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(splinePP, "Change Point Mode");
            splinePP.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(splinePP);
        }
    }

    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = splinePP.GetPoint(0f);
        Handles.DrawLine(point, point + splinePP.GetDirection(0f) * directionScale);
        int steps = stepsPerCurve * splinePP.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = splinePP.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + splinePP.GetDirection(i / (float)steps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(splinePP.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0 || index == splinePP.ChangePointIndex || index == splinePP.StopPointIndex)
        {
            size *= 2f;
        }
        Handles.color = modeColors[(int)splinePP.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.SphereHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(splinePP, "Move Point");
                EditorUtility.SetDirty(splinePP);
                splinePP.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}
