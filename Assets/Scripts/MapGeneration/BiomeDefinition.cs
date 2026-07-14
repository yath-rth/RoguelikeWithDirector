using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BiomeDefinition", menuName = "MapGeneration/BiomeDefinition")]
public class BiomeDefinition : ScriptableObject
{
    public string name;
    public GameObject[] enemies;
    public TileBase[] decor;
    public TileBase[] obstacles;
    public int minDecorations;
    public int maxDecorations;
    public int minObstacles;
    public int maxObstacles;
}
