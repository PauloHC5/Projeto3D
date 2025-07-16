using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCharacterCombatController))]
public class PlayerCharacterCombatControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var controller = (PlayerCharacterCombatController)target;

        // Sync WeaponsPrefabs with WeaponTypes enum
        if (controller.WeaponsPrefabs != null)
        {
            for (int i = 0; i < controller.WeaponsPrefabs.Count; i++)
            {
                var weaponPrefab = controller.WeaponsPrefabs[i].prefab;
                if (weaponPrefab != null)
                {
                    // Copy struct, modify, then assign back to the list
                    var weaponPrefabEntry = controller.WeaponsPrefabs[i];
                    weaponPrefabEntry.name = weaponPrefab.WeaponType.ToString();
                    controller.WeaponsPrefabs[i] = weaponPrefabEntry;
                }
            }            
        }

        // Sync gunsAmmo with AmmoTypes enum        
        if (controller.GunsAmmoInitializer != null)
        {
            // Create a new array to avoid modifying the property directly
            string[] names = Enum.GetNames(typeof(AmmoTypes));
            GunAmmo[] updatedGunsAmmo = new GunAmmo[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                // Ensure each ammo type is set correctly
                updatedGunsAmmo[i] = new GunAmmo
                {
                    AmmoType = (AmmoTypes)Enum.Parse(typeof(AmmoTypes), names[i])
                };

                // Preserve existing ammoAmount if possible
                if (i < controller.GunsAmmoInitializer.Length)
                {
                    updatedGunsAmmo[i].AmmoAmount = controller.GunsAmmoInitializer[i].AmmoAmount;
                }
            }

            // Assign the updated array back to the property
            controller.GunsAmmoInitializer = updatedGunsAmmo;
        }

        // Draw all properties except 'gunsAmmo'
        DrawPropertiesExcluding(serializedObject, "GunsAmmoInitializer");

        // Show GunsAmmo array with ammoType as read-only and ammoAmount editable
        SerializedProperty gunsAmmo = serializedObject.FindProperty("GunsAmmoInitializer");
        if (gunsAmmo != null && gunsAmmo.isArray)
        {
            for (int i = 0; i < gunsAmmo.arraySize; i++)
            {
                SerializedProperty element = gunsAmmo.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(GUI.skin.box);

                // Show ammoType as read-only
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("AmmoType"));
                EditorGUI.EndDisabledGroup();

                // Allow editing ammoAmount
                EditorGUILayout.PropertyField(element.FindPropertyRelative("AmmoAmount"));

                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("GunsAmmo array is not set or is not an array.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(controller);
        }
    }
}
