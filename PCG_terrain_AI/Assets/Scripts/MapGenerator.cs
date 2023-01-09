using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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

    //UI elements
    public Text seedTxt;
    public Text heightTxt;
    public Slider heightSlider;
    public Text zoomTxt;
    public Slider zoomSlider;

    //Height/influence of height on noiseMap 
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public AnimationCurve defaultHeightCurve;
    public AnimationCurve desertHeightCurve;
    public AnimationCurve forestHeightCurve;
    public AnimationCurve mountainHeightCurve;
    public AnimationCurve snowyHeightCurve;
    public AnimationCurve waterHeightCurve;

    public bool autoUpdate;
    public bool saveImage = false;

    //Region "palette" for texturing based on heights
    public TerrainType[] regions;
    public TerrainType[] defaultRegion;
    public TerrainType[] desertRegion;
    public TerrainType[] forestRegion;
    public TerrainType[] mountainRegion;
    public TerrainType[] snowyRegion;
    public TerrainType[] waterRegion;

    public void GenerateMap() {
        //Making a noiseMap utilizing a Noise class
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        //Making a color array containing all "chunks" of the map
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        //Looping through all spots on map
        for(int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                //Setting height based on heights from noisemap
                float currentHeight = noiseMap[x, y];

                //Going through a regions "palette" for applying color based on height
                for (int i = 0; i < regions.Length; i++) {
                    if(currentHeight <= regions[i].height) {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        //Sets the seed text
        seedTxt.text = "Seed: " + seed;

        //Sets the displayMode, DrawMode.Mesh is the actual map, while the other two are noiseMap and colorMap
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }

        //Will save entire noisemap as image
        if (saveImage) {
            //Gets the texture for the noisemap and changes it to a byte array in order to save as an image
            Texture2D noiseMapTex = TextureGenerator.TextureFromHeightMap(noiseMap);
            byte[] noiseBytes = noiseMapTex.EncodeToPNG();
            
            //Will save to project folder for testing purposes
            File.WriteAllBytes(Application.dataPath + "/../SavedNoiseMap.png", noiseBytes);

            //Generates object of type Parameters, which is used to make a json-object and saved as a json file.
            Parameters parameters = new Parameters(mapChunkSize, mapChunkSize, levelOfDetail, noiseScale, octaves, persistance, lacunarity, seed, offset, meshHeightMultiplier, regions);
            string parametersInJson = SaveToString(parameters);
            File.WriteAllText(Application.dataPath + "/../SavedParameters.json", parametersInJson);

            saveImage = false;
        }

    }

    //Makes sure that lacunarity and octaves are not below certain values
    void OnValidate() {
        if(lacunarity < 1) {
            lacunarity = 1;
        }
        if(octaves < 0) {
            octaves = 0;
        }
    }

    //Sets the region & and the height cruve based on the toggled box in the menu
    public void SetRegion(GameObject regionToggle) {
        switch (regionToggle.name) {
            case "ToggleDesert":
                regions = desertRegion;
                meshHeightCurve = desertHeightCurve;
                break;
            case "ToggleForest":
                regions = forestRegion;
                meshHeightCurve = forestHeightCurve;
                break;
            case "ToggleMountain":
                regions = mountainRegion;
                meshHeightCurve = mountainHeightCurve;
                break;
            case "ToggleSnow":
                regions = snowyRegion;
                meshHeightCurve = snowyHeightCurve;
                break;
            case "ToggleWater":
                regions = waterRegion;
                meshHeightCurve = waterHeightCurve;
                break;
            default:
                //ToggleDefault
                regions = defaultRegion;
                meshHeightCurve = defaultHeightCurve;
                break;
        }
        GenerateMap();
    }

    public void SetHeightCurveMultiplier() {
        meshHeightMultiplier = heightSlider.value;
        heightTxt.text = "Height Multiplier: " + meshHeightMultiplier.ToString("F2");
        GenerateMap();
    }
    public void SetNoiseScale() {
        noiseScale = zoomSlider.value;
        zoomTxt.text = "Noise Scale: " + noiseScale.ToString("F1");
        GenerateMap();
    }

    //Changes seed
    public void NextSeed() {
        seed++;
        GenerateMap();
    }
    public void RandomSeed() {
        seed = Random.Range(0, 100000);
        GenerateMap();
    }

    //Changes bool to allow for saving as image
    public void ChangeBool() {
        saveImage = true;
        GenerateMap();
    }
    public string SaveToString(Parameters parameters) {
        return JsonUtility.ToJson(parameters);
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

//[System.Serializable]
public class Parameters
{
    public int width;
    public int height;
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public TerrainType[] regions;

    public Parameters(int _width, int _height, int _levelOfDetail, float _noiseScale, int _octaves, float _persistance, float _lacunarity, int _seed, Vector2 _offset, float _meshHeightMultiplier, TerrainType[] _regions) {
        width = _width;
        height = _height;
        levelOfDetail = _levelOfDetail;
        noiseScale = _noiseScale;

        octaves = _octaves;
        persistance = _persistance;
        lacunarity = _lacunarity;

        seed = _seed;
        offset = _offset;

        meshHeightMultiplier = _meshHeightMultiplier;
        regions = _regions;
    }
    
}
