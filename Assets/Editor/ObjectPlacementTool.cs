using UnityEngine;
using UnityEditor;
using System.Linq;

public class ObjectPlacementTool : EditorWindow
{
    private Vector3 targetPosition = Vector3.zero;
    private enum ArrangeAxis { X, Y, Z }
    private ArrangeAxis axis = ArrangeAxis.X;

    private enum SpacingMode { FixedInterval, ByObjectSize }
    private SpacingMode spacingMode = SpacingMode.FixedInterval;
    private float fixedSpacing = 1.0f;
    private float sizeBasedSpacingGap = 0.1f;

    [MenuItem("Tools/Object Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<ObjectPlacementTool>("Object Placement");
    }

    private void OnGUI()
    {
        GUILayout.Label("Object Placement Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetPosition = EditorGUILayout.Vector3Field("Target Coordinate", targetPosition);

        EditorGUILayout.Space();
        
        DrawMoveSection();

        EditorGUILayout.Space();
        // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        DrawArrangeSection();
    }

    private void DrawMoveSection()
    {
        GUILayout.Label("1. Move Single Object", EditorStyles.boldLabel);
        if (GUILayout.Button("Move Selected to Coordinate"))
        {
            MoveSelectedObject();
        }

        if (Selection.activeGameObject == null)
        {
            EditorGUILayout.HelpBox("Select a single GameObject in the Hierarchy to move it.", MessageType.Info);
        }
    }

    private void DrawArrangeSection()
    {
        GUILayout.Label("2. Arrange Multiple Objects", EditorStyles.boldLabel);

        axis = (ArrangeAxis)EditorGUILayout.EnumPopup("Arrangement Axis", axis);
        spacingMode = (SpacingMode)EditorGUILayout.EnumPopup("Spacing Mode", spacingMode);

        if (spacingMode == SpacingMode.FixedInterval)
        {
            fixedSpacing = EditorGUILayout.FloatField("Fixed Spacing", fixedSpacing);
        }
        else
        {
            sizeBasedSpacingGap = EditorGUILayout.FloatField("Gap Between Objects", sizeBasedSpacingGap);
        }

        if (GUILayout.Button("Arrange Selected Objects"))
        {
            ArrangeSelectedObjects();
        }

        if (Selection.gameObjects.Length < 2)
        {
            EditorGUILayout.HelpBox("Select two or more GameObjects in the Hierarchy to arrange them.", MessageType.Info);
        }
    }

    private void MoveSelectedObject()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            ShowNotification(new GUIContent("No GameObject selected."));
            return;
        }

        Undo.RecordObject(selectedObject.transform, "Move Object");
        selectedObject.transform.position = targetPosition;
        Debug.Log($"Moved '{selectedObject.name}' to {targetPosition}");
    }

    private void ArrangeSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length < 2)
        {
            ShowNotification(new GUIContent("Select at least two GameObjects to arrange."));
            return;
        }
        
        // For consistent ordering, sort by instance ID
        var sortedObjects = selectedObjects.OrderBy(go => go.GetInstanceID()).ToArray();

        Undo.SetCurrentGroupName("Arrange Objects");
        int group = Undo.GetCurrentGroup();

        Vector3 axisVector = GetAxisVector(axis);
        float totalSize = 0;

        // Calculate total size along the axis
        if (spacingMode == SpacingMode.ByObjectSize)
        {
            totalSize = sortedObjects.Sum(go => GetObjectSize(go, axis)) + (sortedObjects.Length - 1) * sizeBasedSpacingGap;
        }
        else // FixedInterval
        {
            totalSize = (sortedObjects.Length - 1) * fixedSpacing;
        }

        Vector3 currentPosition = targetPosition - axisVector * (totalSize / 2f);

        foreach (var obj in sortedObjects)
        {
            Undo.RecordObject(obj.transform, "Arrange Object");

            float objectSize = GetObjectSize(obj, axis);
            float halfObjectSize = objectSize / 2f;

            if (spacingMode == SpacingMode.ByObjectSize)
            {
                obj.transform.position = currentPosition + axisVector * halfObjectSize;
                currentPosition += axisVector * (objectSize + sizeBasedSpacingGap);
            }
            else // FixedInterval
            {
                obj.transform.position = currentPosition;
                currentPosition += axisVector * fixedSpacing;
            }
        }

        Undo.CollapseUndoOperations(group);
        Debug.Log($"Arranged {sortedObjects.Length} objects around {targetPosition} along the {axis}-axis.");
    }

    private float GetObjectSize(GameObject obj, ArrangeAxis arrangeAxis)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null)
        {
            // If no renderer, check children
            rend = obj.GetComponentInChildren<Renderer>();
        }
        
        if (rend == null)
        {
            // If still no renderer, treat as having size 1 for positioning purposes.
            Debug.LogWarning($"Object '{obj.name}' has no renderer. Using default size of 1 for arrangement.");
            return 1.0f;
        }

        Bounds bounds = rend.bounds;
        switch (arrangeAxis)
        {
            case ArrangeAxis.X: return bounds.size.x;
            case ArrangeAxis.Y: return bounds.size.y;
            case ArrangeAxis.Z: return bounds.size.z;
            default: return 0;
        }
    }

    private Vector3 GetAxisVector(ArrangeAxis arrangeAxis)
    {
        switch (arrangeAxis)
        {
            case ArrangeAxis.X: return Vector3.right;
            case ArrangeAxis.Y: return Vector3.up;
            case ArrangeAxis.Z: return Vector3.forward;
            default: return Vector3.zero;
        }
    }
}
