using UnityEditor;
using UnityEngine;
using Biocrowds.Core;

[CustomPropertyDrawer(typeof(Group))]
public class GroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty idProp = property.FindPropertyRelative("_id");
        int id = idProp != null ? idProp.intValue : -1;
        GUIContent header = new GUIContent($"Grupo {id}");
        EditorGUI.PropertyField(position, property, header, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
