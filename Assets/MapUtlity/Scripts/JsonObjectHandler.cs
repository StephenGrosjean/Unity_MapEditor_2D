using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class JsonObjectHandler : MonoBehaviour
{
    //Object Data, used to represent an object and to assign it's json content to
    [Serializable]
    public class ObjectData
    {
        public int ID;
        public string ObjectType;
        public string ObjectName;
        public int SpriteSize;
        public Color32 SpriteTint;
        public string ObjectCollider;
        public int MaxPerScene;
        public string ObjectPack;
    }

    //ObjectToInstantiate, used to represent the object that will be instantiated 

    [Serializable]
    public class ObjectToInstantiate
    {
        public ObjectData ObjectData;
        public Sprite Sprite;
    }

    [Serializable]
    public class Background
    {
        public string Name;
        public Sprite Sprite;
    }

    [Serializable]
    public class Map
    {
        public string MapName;
        public string Background;
        public List<MapEditor.MapObject> MapData;
    }

    private const string objectsFolderName = "Objects"; //Objects folder name inside the StreamingAssets folder
    private const string mapsFolderName = "Maps"; //Maps folder name inside the StreamingAssets folder
    private const string backgroundsFolderName = "Backgrounds"; //Backgrounds folder name inside the StreamingAssets folder

    public List<ObjectToInstantiate> LoadedObjects = new List<ObjectToInstantiate>(); //List of all loaded object inside the Resource folder
    public List<string> MapPaths = new List<string>();
    public List<Background> Backgrounds = new List<Background>();
    public List<string> Packs = new List<string>();

    private void Awake() {
        LoadObjects(); //Load all objects in Objects folder
        LoadBackgrounds(); //Load backgrounds
        GetMapPaths(); //Retrieve all maps in Maps folder
    }

    private void LoadObjects() {
        //Get pack directories
        string[] packDirectories = Directory.GetDirectories(Application.streamingAssetsPath + "/" + objectsFolderName);

        foreach(string p in packDirectories) {
            string[] objectDirectories = Directory.GetDirectories(p);
            string packName = p.Replace(Application.streamingAssetsPath + "/" + objectsFolderName, "").Replace("\\", "");
            Packs.Add(packName);

            foreach (string s in objectDirectories) {
                //Load object using the .json
                string[] jsonFile = Directory.GetFiles(s + "/", "*.json");

                //Check if we got more than one .json or none in the directory
                if (jsonFile.Length > 1) {
                    Debug.LogError("Json: More than one .json in the folder!");
                }
                else if (jsonFile.Length == 0) {
                    Debug.LogError("Json: No .json files in folder");
                }
                else {
                    CreateObjectFromJson(jsonFile[0], packName); //Create the object from the first element on the list (Should be only one) 
                }
            }
        } 
    }

    private void LoadBackgrounds() {
        string[] backgroundPaths = Directory.GetFiles(Application.streamingAssetsPath + "/" + backgroundsFolderName  + "/" ,"*.png");
        
        foreach(string s in backgroundPaths) {
            Texture2D loadedTex = new Texture2D(1, 1);
            loadedTex.LoadImage(File.ReadAllBytes(s));

            Background background = new Background();
            background.Name = s.Replace(Application.streamingAssetsPath + "/" + backgroundsFolderName + "/", "").Replace(".png", "");
            background.Sprite = Sprite.Create(loadedTex, new Rect(0, 0, loadedTex.width, loadedTex.height), Vector2.one * 0.5f, 50);

            Backgrounds.Add(background);
        }

    }

    public void GetMapPaths() {
        MapPaths.Clear();
        string[] mapFiles = Directory.GetFiles(Application.streamingAssetsPath + "/" + mapsFolderName + "/", "*.json");
        for (int i = 0; i < mapFiles.Length; i++) {
            MapPaths.Add(mapFiles[i]);
        }
    }

    /// <summary>
    /// Create an object from its json at the path
    /// </summary>
    private void CreateObjectFromJson(string path, string packName) {
        string spritePath = path.Replace("/Data.json", "");
        //Set the object data from the json file
        ObjectData loadedData = JsonUtility.FromJson<ObjectData>(File.ReadAllText(path));
        loadedData.ObjectPack = packName;
        ObjectToInstantiate objectToInstantiate = new ObjectToInstantiate();
        objectToInstantiate.ObjectData = loadedData;
        Texture2D loadedTex = new Texture2D(1, 1);
        loadedTex.LoadImage(File.ReadAllBytes(Directory.GetFiles(spritePath + "/", "*.png")[0]));

        Texture2D tex = new Texture2D(loadedTex.width, loadedTex.height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(loadedTex.GetPixels());
        tex.Apply();

        objectToInstantiate.Sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one*0.5f ,loadedData.SpriteSize);
        LoadedObjects.Add(objectToInstantiate); //Add the object to the list
    }

    public void SaveMapToJson(List<MapEditor.MapObject> map, string mapName, string backgroundName) {
        Map m = new Map();
        m.MapData = map;
        m.MapName = mapName;
        m.Background = backgroundName;
        string json = JsonUtility.ToJson(m,true);
        File.WriteAllText(Application.streamingAssetsPath + "/" + mapsFolderName + "/" + mapName + ".json", json);
    }

    public Map LoadMapFromJson(string mapName) {
        Map loadedMap = JsonUtility.FromJson<Map>(File.ReadAllText(Application.streamingAssetsPath + "/" + mapsFolderName + "/" + mapName + ".json"));

        foreach(MapEditor.MapObject o in loadedMap.MapData) {
            if(o.ObjectPack == "") {
                o.ObjectPack = "DefaultPack";
            }
        }
        return loadedMap;
    }

    public Map GetMapListFromJson(string path) {
        Map map = JsonUtility.FromJson<Map>(File.ReadAllText(path));
        return map;
    }

    public Background GetBackgroundByName(string name) {
        foreach(Background b in Backgrounds) {
            if(b.Name == name) {
                return b;
            }
        }
        return null;
    }
    public ObjectToInstantiate FindObject(int id, string packName) {
        foreach(ObjectToInstantiate o in LoadedObjects) {
            if(o.ObjectData.ID == id && o.ObjectData.ObjectPack == packName) {
                return o;
            }
        }
        return null;
    }

}
