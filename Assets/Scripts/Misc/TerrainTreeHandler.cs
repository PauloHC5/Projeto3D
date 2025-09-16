using UnityEngine;

public class TerrainTreeHandler : MonoBehaviour
{
    public GameObject treePrefabWithScript; // Assign your tree prefab here

    void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null) return;

        TreeInstance[] treeInstances = terrain.terrainData.treeInstances;

        foreach (TreeInstance treeInstance in treeInstances)
        {
            // Calculate world position
            Vector3 treeWorldPosition = Vector3.Scale(treeInstance.position, terrain.terrainData.size) + terrain.transform.position;

            // Instantiate your prefab with script
            Instantiate(treePrefabWithScript, treeWorldPosition, Quaternion.identity); 
        }

        // Optionally, clear original terrain trees if instantiating new GameObjects
        // terrain.terrainData.treeInstances = new TreeInstance[0]; 
    }
}