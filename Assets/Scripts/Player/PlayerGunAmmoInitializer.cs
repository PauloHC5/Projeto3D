using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public struct GunAmmo
{    
    public AmmoTypes ammoType;
    [SerializeField] private Int32 ammoAmount;

    public int AmmoAmount => ammoAmount;
}

[Serializable]
public enum AmmoTypes
{    
    Acorn,
    Banana,    
    Spikes,
    Corn,
    Coconut,
}

[ExecuteInEditMode]
public class PlayerGunAmmoInitializer : MonoBehaviour
{
    [SerializeField]
    private GunAmmo[] gunsAmmo;

    public GunAmmo[] WeaponAmmoList
    {
        get { return gunsAmmo; }        
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(AmmoTypes));                
        Array.Resize(ref gunsAmmo, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            gunsAmmo[i].ammoType = (AmmoTypes)Enum.Parse(typeof(AmmoTypes), names[i]);
        }
    }
#endif

}

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
