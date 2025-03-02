using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private int worldWidth = 20;
    [SerializeField] private int worldLength = 20;
    [SerializeField] private float tileSize = 1f;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] groundTilePrefabs;
    [SerializeField] private GameObject[] propPrefabs;
    [SerializeField] private float propSpawnChance = 0.3f;

    private void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        // Create parent object for organization
        GameObject worldParent = new GameObject("Generated World");

        // Generate ground tiles
        for (int x = 0; x < worldWidth; x++)
        {
            for (int z = 0; z < worldLength; z++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
                
                // Spawn random ground tile
                GameObject selectedTile = groundTilePrefabs[Random.Range(0, groundTilePrefabs.Length)];
                GameObject tile = Instantiate(selectedTile, position, Quaternion.identity, worldParent.transform);

                // Randomly spawn props
                if (Random.value < propSpawnChance)
                {
                    GameObject selectedProp = propPrefabs[Random.Range(0, propPrefabs.Length)];
                    GameObject prop = Instantiate(selectedProp, position, Quaternion.Euler(0, Random.Range(0, 360), 0), tile.transform);
                }
            }
        }
    }
} 