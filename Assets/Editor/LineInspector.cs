using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor
{
    void OnSceneGUI()
    {
        //The Editor class has a target variable, which is set to the object to be drawn when OnSceneGUI is called.
        Line line = target as Line;

        Transform handleTransform = line.transform;
        //we can use Tools.pivotRotation to determine the current mode and set our rotation accordingly.
        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        //Handles operates in world space while the points are in the local space of the line.
        Vector3 point0 = handleTransform.TransformPoint(line.point0);
        Vector3 point1 = handleTransform.TransformPoint(line.point1);

        Handles.color = Color.green;
        Handles.DrawLine(point0, point1);
        //Besides showing the line, we can also show position handles for our two points.

        //However, as the handle values are in world space we need to convert them back into the line's local space 
        //with the InverseTransformPoint method. Also, we only need to do this when a point has changed.
        //We can use EditorGUI.BeginChangeCheck and EditorGUI.EndChangeCheck for this.
        //The second method tells us whether a change happened after calling the first method.

        //There are two additional issues that need attention. First, we cannot undo the drag operations.
        //This is fixed by adding a call to Undo.RecordObject before we make any changes.
        //Second, Unity does not know that a change was made, so for example won't ask the user to save when quitting.
        //This is remedied with a call to EditorUtility.SetDirty.
        EditorGUI.BeginChangeCheck();
        point0 = Handles.DoPositionHandle(point0, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
            line.point0 = handleTransform.InverseTransformPoint(point0);
        }
        EditorGUI.BeginChangeCheck();
        point1 = Handles.DoPositionHandle(point1, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line, "Move Point");
            EditorUtility.SetDirty(line);
            line.point1 = handleTransform.InverseTransformPoint(point1);
        }
    }
}
