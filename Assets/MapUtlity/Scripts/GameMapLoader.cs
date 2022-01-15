using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(JsonObjectHandler))]
public class GameMapLoader : MonoBehaviour
{
    private JsonObjectHandler jsonObjectHandler;

    [SerializeField] private GameObject mainUI;
    [SerializeField] private Transform mapSelectContainerUI;
    [SerializeField] private GameObject mapButtonUI;

    [SerializeField] private GameObject objectPrefab; //Object prefab that will be instantiated
    [SerializeField] private Transform mapObjectContainer; //The map object container
    [SerializeField] private SpriteRenderer mapBackground;

    private void Awake() {
        jsonObjectHandler = GetComponent<JsonObjectHandler>();
    }

    private void Start() {
        Populate();
    }

    private void Populate() {
        foreach(string s in jsonObjectHandler.MapPaths) {
           string mapName = jsonObjectHandler.GetMapListFromJson(s).MapName;
            GameObject g = Instantiate(mapButtonUI, mapSelectContainerUI);
            g.GetComponentInChildren<TextMeshProUGUI>().text = mapName;
            g.GetComponent<Button>().onClick.AddListener(delegate { Play(mapName); });
        }
    }

    private void Play(string mapName) {
        JsonObjectHandler.Map map = jsonObjectHandler.LoadMapFromJson(mapName);
        mapBackground.sprite = jsonObjectHandler.GetBackgroundByName(map.Background).Sprite;

        foreach (MapEditor.MapObject o in map.MapData) {
            CreateNewObjectFromData(jsonObjectHandler.FindObject(o.ObjectID, o.ObjectPack), o.position);
        }

        StartCoroutine("GenerateMapCollisionNextFrame");

        mainUI.SetActive(false);
    }

    private GameObject CreateNewObjectFromData(JsonObjectHandler.ObjectToInstantiate toInstantiate, Vector3Int position) {
        GameObject obj = Instantiate(objectPrefab, Vector3.zero, objectPrefab.transform.rotation); //Instantiate the object at 0,0,0
        obj.transform.SetParent(mapObjectContainer);

        //Set the different components to the toInstantiate data
        obj.name = toInstantiate.ObjectData.ObjectName;
        SpriteRenderer objSpriteRenderer = obj.GetComponent<SpriteRenderer>();
        objSpriteRenderer.sprite = toInstantiate.Sprite;
        objSpriteRenderer.color = toInstantiate.ObjectData.SpriteTint;

        Collider2D objectCollider = null;

        //Add the colliders if needed
        if (toInstantiate.ObjectData.ObjectCollider == "BoxCollider") {
            objectCollider = obj.AddComponent<BoxCollider2D>();
        }
        else if (toInstantiate.ObjectData.ObjectCollider == "CircleCollider") {
            objectCollider = obj.AddComponent<CircleCollider2D>();
        }
        else if (toInstantiate.ObjectData.ObjectCollider == "PolygonCollider") {
            objectCollider = obj.AddComponent<PolygonCollider2D>();
        }

        if(objectCollider != null) {
            objectCollider.usedByComposite = true;
        }
        obj.transform.position = position;

        return obj;
    }

    IEnumerator GenerateMapCollisionNextFrame() {
        yield return null;
        mapObjectContainer.GetComponent<CompositeCollider2D>().GenerateGeometry();
    }
}
