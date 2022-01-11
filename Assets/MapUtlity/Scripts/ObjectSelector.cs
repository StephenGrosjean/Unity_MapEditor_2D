using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(JsonObjectHandler))]
[RequireComponent(typeof(MapEditor))]
public class ObjectSelector : MonoBehaviour
{
    public class UIButton
    {
        public string Tab;
        public int ID;
        public GameObject Button;
        public UIButton(string tab, int ID, GameObject button) {
            this.Tab = tab;
            this.ID = ID;
            this.Button = button;
        }
    }

    public enum GameState
    {
        Editor,
        Loader
    }
    public enum ToolState
    {
        Placing,
        Erasing
    }

    public GameState currentGameState;
    public ToolState currentToolState;

    [SerializeField] private GameObject objectSelectorUI;
    [SerializeField] private GameObject mapLoaderUI;
    [SerializeField] private GameObject mapLoaderContainerUI;
    [SerializeField] private GameObject tabListUI;
    [SerializeField] private GameObject backgroundListUI;

    [SerializeField] private GameObject mapButtonUI;
    [SerializeField] private GameObject objectContainerUI; //Container (Content UI scroll)
    [SerializeField] private GameObject objectPrefabUI; //UI button that will be instantiated
    [SerializeField] private GameObject tabPrefabUI; //UI Tab Button
    [SerializeField] private GameObject backgroundPrefabUI;
    [SerializeField] private GameObject gridObject;

    [Header("Editor Features")]
    [SerializeField] private GameObject buttonsUI;
    [SerializeField] private TMP_InputField mapNameInputField;
    [SerializeField] private Button eraseToolButton;
    [SerializeField] private Button gridToolButton;
    [SerializeField] private Color disabledToolColor;
    [SerializeField] private Color enabledToolColor;
    [SerializeField] private Color enabledEraserColor;

    private bool isGridEnabled;
    private string currentTab;
    private List<UIButton> buttons = new List<UIButton>();
    //Components
    private JsonObjectHandler jsonObjectHandler; 
    private JsonTabHandler jsonTabHandler;
    private MapEditor mapEditor;

    private void Awake() {
        //Assign the components
        jsonObjectHandler = GetComponent<JsonObjectHandler>();
        jsonTabHandler = GetComponent<JsonTabHandler>();
        mapEditor = GetComponent<MapEditor>();

        //Init the objectPlacement
        mapEditor.Init(this, jsonObjectHandler);
    }
    void Start()
    {
        
        Populate(); //Populate the UI
        SetUIState();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E) && currentToolState != ToolState.Erasing) {
            switch (currentGameState) {
                case GameState.Editor:
                    ObjectSelectorUIActiveState(!objectSelectorUI.transform.gameObject.activeInHierarchy);

                    //If we are currently placing an object, cancel its placement
                    if (mapEditor.PlacingObject) {
                        mapEditor.CancelPlacement();
                    }
                    break;
                case GameState.Loader:
                    MapLoaderUIActiveState(!mapLoaderUI.activeInHierarchy);
                    break;
                default:
                    break;
            }

        }
    }

    /// <summary>
    /// Populate the UI
    /// </summary>
    private void Populate() {
        buttons.Clear();
        foreach (Transform child in mapLoaderContainerUI.transform) {
            Destroy(child.gameObject);
        }

        foreach (Transform child in objectContainerUI.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in tabListUI.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in backgroundListUI.transform) {
            Destroy(child.gameObject);
        }

        //Create Tab Buttons
        foreach (JsonTabHandler.Tab tab in jsonTabHandler.Tabs) {
            GameObject tabObject = Instantiate(tabPrefabUI, tabListUI.transform);
            tabObject.GetComponentInChildren<TextMeshProUGUI>().text = tab.tab;
            tabObject.GetComponent<Button>().onClick.AddListener(delegate { SetCurrentTab(tab.tab); });
        }


        //Create Background Buttons
        foreach(JsonObjectHandler.Background b in jsonObjectHandler.Backgrounds) {
            GameObject backgroundObject = Instantiate(backgroundPrefabUI, backgroundListUI.transform);
            backgroundObject.GetComponent<Image>().sprite = b.Sprite;
            backgroundObject.GetComponent<Button>().onClick.AddListener(delegate { mapEditor.SetBackground(b.Name); });
        }

        switch (currentGameState) {
            case GameState.Editor:
                foreach (JsonObjectHandler.ObjectToInstantiate o in jsonObjectHandler.LoadedObjects) {
                    GameObject uiElement = Instantiate(objectPrefabUI, objectContainerUI.transform);
                    uiElement.GetComponent<Button>().image.sprite = o.Sprite;
                    uiElement.GetComponent<Button>().image.color = o.ObjectData.SpriteTint;
                    uiElement.GetComponentInChildren<TextMeshProUGUI>().text = o.ObjectData.ObjectName;
                    uiElement.GetComponent<Button>().onClick.AddListener(delegate { mapEditor.InstantiateObject(o); });

                    UIButton button = new UIButton(o.ObjectData.ObjectType, o.ObjectData.ID, uiElement);
                    buttons.Add(button);
                }
                break;

            case GameState.Loader:
                foreach(string s in jsonObjectHandler.MapPaths) {
                    string mapName = jsonObjectHandler.GetMapListFromJson(s).MapName;
                    GameObject uiElement = Instantiate(mapButtonUI, mapLoaderContainerUI.transform);
                    uiElement.GetComponentInChildren<TextMeshProUGUI>().text = mapName;
                    uiElement.GetComponent<Button>().onClick.AddListener(delegate 
                                                                        { 
                                                                            mapEditor.LoadMap(mapName); 
                                                                            MapLoaderUIActiveState(false);
                                                                            SetGameState(GameState.Editor);
                                                                            mapNameInputField.text = mapName;
                                                                        });
                }
                break;
        }
        
    }

    public void MapLoaderUIActiveState(bool state) {
        mapLoaderUI.SetActive(state);
    }

    public void ObjectSelectorUIActiveState(bool state) {
        objectSelectorUI.SetActive(state);
    }
    private void SetUIState() {
        switch (currentGameState) {
            case GameState.Editor:
                MapLoaderUIActiveState(false);
                buttonsUI.SetActive(true);
                break;

            case GameState.Loader:
                ObjectSelectorUIActiveState(false);
                MapLoaderUIActiveState(true);
                buttonsUI.SetActive(false);
                break;
        }
    }

    private void ResetToolState() {
        currentToolState = ToolState.Placing;
    }

    private void ToggleTabContent() {
        foreach(UIButton b in buttons) {
            b.Button.SetActive(b.Tab == currentTab);
        }
    }

    public bool IsMenuOpen() {
        return objectSelectorUI.activeInHierarchy;
    }

    //UI Buttons
    public void EditMap() {
        SetGameState(GameState.Editor);
    }

    public void SaveAndStopEdit() {
        mapEditor.SaveMap(mapNameInputField.text);
        jsonObjectHandler.GetMapPaths();
        mapEditor.ClearMap();
        SetGameState(GameState.Loader);
    }

    public void CancelAndStopEdit() {
        SetGameState(GameState.Loader);
        mapEditor.ClearMap();
    }

    private void SetGameState(GameState gameState) {
        currentGameState = gameState;
        ResetToolState();
        SetUIState();
        Populate();
        SetCurrentTab(jsonTabHandler.Tabs[0].tab);
    }

    public void EraseToolClick() {
        if(currentToolState != ToolState.Erasing) {
            currentToolState = ToolState.Erasing;
            eraseToolButton.GetComponent<Image>().color = enabledEraserColor;
        }
        else {
            currentToolState = ToolState.Placing;
            eraseToolButton.GetComponent<Image>().color = disabledToolColor;
        }
    }

    public void ToggleGrid() {
        isGridEnabled = !isGridEnabled;
        gridToolButton.GetComponent<Image>().color = isGridEnabled ? enabledToolColor : disabledToolColor;
        gridObject.SetActive(isGridEnabled);
    }
    
    private void SetCurrentTab(string tabName) {
        currentTab = tabName;
        ToggleTabContent();
    }

    public void UpdateLocks() {
        foreach(JsonObjectHandler.ObjectToInstantiate i in jsonObjectHandler.LoadedObjects) {
            if (i.ObjectData.MaxPerScene == 0) continue;
            int instancesInScene = 0;
            UIButton uiButton = null;
            foreach(UIButton b in buttons) {
                if(b.ID == i.ObjectData.ID) {
                    uiButton = b;
                }
            }

            foreach(MapEditor.MapObject m in mapEditor.objectsInMap) {
                if (m.ObjectID == i.ObjectData.ID) {
                    instancesInScene++;
                }
            }

            uiButton.Button.transform.Find("Lock").gameObject.SetActive(instancesInScene >= i.ObjectData.MaxPerScene);
            uiButton.Button.GetComponent<Button>().interactable = !(instancesInScene >= i.ObjectData.MaxPerScene);

        }
    }

    public bool CanPlaceObject(int id) {
        foreach (UIButton b in buttons) {
            if (b.ID == id) {
                return b.Button.GetComponent<Button>().IsInteractable();
            }
        }
        return true;
    }
}
