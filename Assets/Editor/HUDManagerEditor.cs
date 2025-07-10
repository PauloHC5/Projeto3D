using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HUDManager))]
public class HUDManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target HUDManager
        HUDManager hudManager = (HUDManager)target;

        // Get the serialized object and property for _weaponCrosshairs
        serializedObject.Update();
        SerializedProperty weaponCrosshairsProp = serializedObject.FindProperty("_weaponsUI");

        // Sync each crosshair's name with its WeaponType
        for (int i = 0; i < weaponCrosshairsProp.arraySize; i++)
        {
            SerializedProperty crosshairProp = weaponCrosshairsProp.GetArrayElementAtIndex(i);
            SerializedProperty weaponTypeProp = crosshairProp.FindPropertyRelative("WeaponType");
            SerializedProperty nameProp = crosshairProp.FindPropertyRelative("name");

            // Set the name to the WeaponType's string representation
            if (weaponTypeProp != null && nameProp != null)
            {
                nameProp.stringValue = weaponTypeProp.enumDisplayNames[weaponTypeProp.enumValueIndex];
            }
        }

        // Sync _weaponSlotsInspectors.name with WeaponSlot
        SerializedProperty weaponSlotsInspectorsProp = serializedObject.FindProperty("_weaponSlotsInspector");
        if (weaponSlotsInspectorsProp != null)
        {
            for (int i = 0; i < weaponSlotsInspectorsProp.arraySize; i++)
            {
                SerializedProperty slotInspectorProp = weaponSlotsInspectorsProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = slotInspectorProp.FindPropertyRelative("name");
                if (nameProp != null)
                {
                    nameProp.stringValue = $"Slot {i + 1}";
                }
            }
        }

        // Draw the default inspector
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}
