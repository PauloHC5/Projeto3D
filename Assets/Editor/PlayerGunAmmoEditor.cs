using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerGunAmmoInitializer))]
public class PlayerGunAmmoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty gunsAmmo = serializedObject.FindProperty("gunsAmmo");

        for (int i = 0; i < gunsAmmo.arraySize; i++)
        {
            SerializedProperty element = gunsAmmo.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(GUI.skin.box);

            // Show ammoType as read-only
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("ammoType"));
            EditorGUI.EndDisabledGroup();

            // Allow editing ammoAmount
            EditorGUILayout.PropertyField(element.FindPropertyRelative("ammoAmount"));

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
