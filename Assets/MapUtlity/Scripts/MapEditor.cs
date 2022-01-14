using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MapEditor : MonoBehaviour
{
    //Components
    private ObjectSelector objectSelector;
    private JsonObjectHandler jsonObjectHandler;

    //Map object, used to represent the object in the map and is used in the map save
    [Serializable]
    public class MapObject
    {
        [NonSerialized] public GameObject instantiatedObject;
        public int ObjectID;
        public Vector3Int position;

    }

    [Header("Grid Cursor")]
    [SerializeField] private SpriteRenderer gridCursor;
    [SerializeField] private Sprite normalGridCursor;
    [SerializeField] private Sprite eraserGridCursor;

    [Space(10)]
    [SerializeField] private GameObject objectPrefab; //Object prefab that will be instantiated
    [SerializeField] private Transform mapObjectContainer; //The map object container
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private SpriteRenderer mapBackground;

    [Header("Map Validator")]
    [SerializeField] private int playerCount;
    [SerializeField] private int weaponCount;
    [SerializeField] private TMP_InputField playerCountField;
    [SerializeField] private TMP_InputField weaponCountField;


    //Object placement
    private Transform currentObjectInHand;
    private JsonObjectHandler.ObjectToInstantiate currentObjectInHandData;
    private string currentBackground;

    public List<MapObject> objectsInMap = new List<MapObject>(); //List of all objects in the map
    

    public bool PlacingObject; //Are we currently placing an object?

    private void OnDrawGizmos() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int worldPositionOnGrid = OnGrid(new Vector3(worldPosition.x, worldPosition.y, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldPositionOnGrid, Vector3.one);
    }

    /// <summary>
    /// Init and assign the different components
    /// </summary>
    public void Init(ObjectSelector objectSelector, JsonObjectHandler jsonObjectHandler) {
        this.objectSelector = objectSelector;
        this.jsonObjectHandler = jsonObjectHandler;
    }

    private void Update() {
        if(objectSelector.currentToolState != ObjectSelector.ToolState.Placing) {
            CancelPlacement();
        }

        if (Input.GetKeyDown(KeyCode.U)) {
            buttonContainer.SetActive(!buttonContainer.activeInHierarchy);
        }

        //Enable the cursor only on editor mode and if menu is closed
        gridCursor.gameObject.SetActive(objectSelector.currentGameState == ObjectSelector.GameState.Editor && !objectSelector.IsMenuOpen());

        //Get the position of the cursor in world position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Modifiy this position to stick to a grid
        Vector3Int worldPositionOnGrid = OnGrid(new Vector3(worldPosition.x, worldPosition.y, 0));

        //Set the gridCursor to cursorPos on grid
        gridCursor.transform.position = worldPositionOnGrid;

        bool isPlacementValid = IsObjectPlacementValid(worldPositionOnGrid);

        switch (objectSelector.currentToolState) {
            case ObjectSelector.ToolState.Placing:
                gridCursor.sprite = normalGridCursor;
                gridCursor.color = isPlacementValid ? Color.white : Color.red;
                break;

            case ObjectSelector.ToolState.Erasing:
                gridCursor.sprite = eraserGridCursor;
                gridCursor.color = Color.red;

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) {
                    MapObject clickedObject = FindMapObjectByPos(worldPositionOnGrid);

                    if (clickedObject != null) {
                        Destroy(clickedObject.instantiatedObject);
                        objectsInMap.Remove(clickedObject);
                        objectSelector.UpdateLocks();
                    }
                }
                break;
        }

        //Are we placing an object?
        if(PlacingObject) {
            //Set the currentObjectInHand to this cursor position
            currentObjectInHand.transform.position = worldPositionOnGrid;
            currentObjectInHand.GetComponent<SpriteRenderer>().sortingOrder = 1;
            //Check if the placement is correct
            if (isPlacementValid) {
                //If yes set the object sprite to its color
                currentObjectInHand.GetComponent<SpriteRenderer>().color = currentObjectInHandData.ObjectData.SpriteTint;

                //Check if we left click with the mouse
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) {
                    currentObjectInHand.GetComponent<SpriteRenderer>().sortingOrder = 0;

                    //Create a new mapObject and set the different members
                    MapObject mapObject = new MapObject();
                    mapObject.ObjectID = currentObjectInHandData.ObjectData.ID;
                    mapObject.position = worldPositionOnGrid;
                    mapObject.instantiatedObject = Instantiate(currentObjectInHand.gameObject, mapObjectContainer);

                    //Add the mapObject to the list
                    objectsInMap.Add(mapObject);
                    objectSelector.UpdateLocks();

                    if (!objectSelector.CanPlaceObject(currentObjectInHandData.ObjectData.ID)) {
                        CancelPlacement();
                    }
                }
                else if(Input.GetMouseButtonDown(1)) {
                    CancelPlacement();
                }
            }           
        }

        //Check if we are still placing an object
        if (!PlacingObject && currentObjectInHand != null) {
            //If no then set the currentObjectInHand and its data to null
            currentObjectInHand = null;
            currentObjectInHandData = null;
        }
    }

    private GameObject CreateNewObjectFromData(JsonObjectHandler.ObjectToInstantiate toInstantiate) {
        GameObject obj = Instantiate(objectPrefab, Vector3.zero, objectPrefab.transform.rotation); //Instantiate the object at 0,0,0
        obj.transform.SetParent(mapObjectContainer);

        //Set the different components to the toInstantiate data
        obj.name = toInstantiate.ObjectData.ObjectName;
        SpriteRenderer objSpriteRenderer = obj.GetComponent<SpriteRenderer>();
        objSpriteRenderer.sprite = toInstantiate.Sprite;
        objSpriteRenderer.color = toInstantiate.ObjectData.SpriteTint;

        return obj;
    }

    /// <summary>
    /// Instantiate an object in the world using the data from the JsonObjectHandler
    /// </summary>
    public void InstantiateObject(JsonObjectHandler.ObjectToInstantiate toInstantiate) {
        //Check if we are not placing an object
        if (!PlacingObject) {
            GameObject obj = CreateNewObjectFromData(toInstantiate);

            //Set the currentObjectInHand and its data to the object that we just instantiated
            currentObjectInHand = obj.transform;
            currentObjectInHandData = toInstantiate;

            PlacingObject = true;
            objectSelector.ObjectSelectorUIActiveState(false); //Disable the object selection UI
        }
    }

    public GameObject InstantiateObject(JsonObjectHandler.ObjectToInstantiate toInstantiate, Vector3Int position) {
        //Check if we are not placing an object
        if (!PlacingObject) {
            GameObject obj = CreateNewObjectFromData(toInstantiate);

            obj.transform.position = position;
            return obj;
        }
        return null;
    }

    /// <summary>
    /// Cancel the placement of the current object and destroy it
    /// </summary>
    public void CancelPlacement() {
            PlacingObject = false;

            //Check if we have an object in hand
            if(currentObjectInHand != null) {
                Destroy(currentObjectInHand.gameObject); //Kill it!

                //Set currentObjectInHand and its data to null
                currentObjectInHand = null; 
                currentObjectInHandData = null;
            }
        }

    /// <summary>
    /// Align the position on a grid
    /// </summary>
    private Vector3Int OnGrid(Vector3 pos) {
        return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }

    /// <summary>
    /// Check if the placement of the object is correct and not overlaping an other object
    /// </summary>
    private bool IsObjectPlacementValid(Vector3Int pos) {
        foreach(MapObject m in objectsInMap) {
            if(m.position == pos) {
                return false;
            }
        }
        return true;
    }

    private MapObject FindMapObjectByPos(Vector3Int pos) {
        foreach (MapObject m in objectsInMap) {
            if (m.position == pos) {
                return m;
            }
        }
        return null;
    }

    public void SaveMap(string mapName) {
        jsonObjectHandler.SaveMapToJson(objectsInMap, mapName, currentBackground);
    }

    public void LoadMap(string mapName) {
        ClearMap();
        JsonObjectHandler.Map map = jsonObjectHandler.LoadMapFromJson(mapName);
        SetBackground(map.Background);
        objectsInMap = map.MapData;
        List<MapObject> invalidObjects = new List<MapObject>();
        foreach(MapObject o in objectsInMap) {
            JsonObjectHandler.ObjectToInstantiate toCreate = jsonObjectHandler.FindObjectByID(o.ObjectID);
            if(toCreate != null) {
                o.instantiatedObject = InstantiateObject(toCreate, o.position);
            }
            else {
                invalidObjects.Add(o);
                Debug.Log("Warning: Missing blocks found in this map");
            }
        }
        
        foreach(MapObject o in invalidObjects) {
            objectsInMap.Remove(o);
        }

    }

    public void ClearMap() {
        foreach (Transform child in mapObjectContainer.transform) {
            Destroy(child.gameObject);
        }
    }

    public void SetBackground(string background) {
        mapBackground.sprite = jsonObjectHandler.GetBackgroundByName(background).Sprite;
        currentBackground = background;
    }

    public string MapValidator() {
        int playerSpawns = 0;
        int weaponSpawns = 0;
        string returnMessage = "";


        foreach (MapObject o in objectsInMap) {
            string objectName = jsonObjectHandler.FindObjectByID(o.ObjectID).ObjectData.ObjectName;
            if (objectName.Contains("Player") && objectName.Contains("Spawn")) {
                playerSpawns++;
            }

            if(objectName.Contains("Weapon") && objectName.Contains("Spawn")) {
                weaponSpawns++;
            }
        }

        if(playerSpawns < playerCount) {
            returnMessage += "Not enough player spawns, " + " need "+ (playerCount- playerSpawns) + "\n";
        }

        if (weaponSpawns < weaponCount) {
            returnMessage += "Not enough weapon spawns, " + " minimum needed " + (weaponCount - weaponSpawns);
        }

        if (playerSpawns >= playerCount && weaponSpawns >= weaponCount) {
            returnMessage = "Map valid";
        }
        return returnMessage;
    }
}
