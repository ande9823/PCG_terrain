using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    //Enum for generation modes
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
    public DrawMode drawMode;

    //Mapsize and levels of detail fits with amount of vertices for optimizing LOD
    const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;

    //Layering noisemaps to add detail to generation
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    //Seed and placement on seed
    public int seed;
    public Vector2 offset;

    //Height/influence of height on noiseMap 
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    //Region "palette" for texturing based on heights
    public TerrainType[] regions;
    //public TerrainType[] defaultRegion;
    public TerrainType[] desertRegion;
    public TerrainType[] forestRegion;
    public TerrainType[] mountainRegion;
    public TerrainType[] snowyRegion;
    public TerrainType[] waterRegion;

    public void GenerateMap() {
        //Making a noiseMap utilizing a Noise class.
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        //Making a color array containing all "chunks" of the map
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        //Looping through all spots on map
        for(int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                //Setting height based on heights from noisemap
                float currentHeight = noiseMap[x, y];

                //Change parameters like heights & regions for diversity.
                /*
                 */

                //Going through a regions "palette" for applying color based on height
                for (int i = 0; i < regions.Length; i++) {
                    if(currentHeight <= regions[i].height) {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        } 
        //Sets the displayMode, DrawMode.Mesh is the actual map, while the other two are noiseMap and colorMap.
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
        
    }

    //Makes sure that lacunarity and octaves are not below certain values.
    void OnValidate() {
        if(lacunarity < 1) {
            lacunarity = 1;
        }
        if(octaves < 0) {
            octaves = 0;
        }
    }
}

//Struct for terrainType, to define terrain-altitudes for a "palette"
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
