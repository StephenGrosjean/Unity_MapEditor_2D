using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
public class TileSetImporter : MonoBehaviour
{
    [SerializeField] private Image tileSetPreview;
    [SerializeField] private int tileSize = 512;
    [SerializeField] private Vector2Int tileCount;
    
    [SerializeField] private TMP_InputField packNameInput;
    [SerializeField] private TMP_InputField tileNameInput;
    [SerializeField] private TMP_InputField tileDimension;
    [SerializeField] private TMP_InputField tilesetSizeX,tilesetSizeY;

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Slider previewScale;
    private Texture2D tileSet;
    private bool loaded;
    public class Tile
    {
        public JsonObjectHandler.ObjectData data;
        public Texture2D texture;
    }

    private void Update() {
        if (loaded) {
            SetPreviewScale((int)previewScale.value);
        }
    }

    public void LoadTileSet() {
        loaded = false;
        GetInputInfo();
        string path = Application.streamingAssetsPath + "/" + "TileSet";
        Texture2D loadedTex = new Texture2D(1, 1);
        loadedTex.LoadImage(File.ReadAllBytes(Directory.GetFiles(path + "/", "*.png")[0]));

        Texture2D tex = new Texture2D(loadedTex.width, loadedTex.height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(loadedTex.GetPixels());
        tex.Apply();

        tileSet = tex;
        tileSetPreview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, tileSize);
        loaded = true;
    }

    public void SaveTileSet() {
        string message = "";
        if (!IsValid(out message)) {
            StartCoroutine(SetMessage(message));
            return;
        }

        List<Tile> tiles = new List<Tile>();
        int tileIndex = 0;

        for(int y = 0; y < tileCount.y; y++) {
            for (int x = 0; x < tileCount.x; x++) {
                Texture2D newTex = new Texture2D(tileSize, tileSize);
                int transparentPixelCount = 0;

                for (int width = 0; width < tileSize; width++) {
                    for (int height = 0; height < tileSize; height++) {
                        Vector2Int coords = new Vector2Int(width + (tileSize * x), height +(tileSize * y));
                        Color pixel = tileSet.GetPixel(coords.x, coords.y);
                        if (pixel.a == 0) {
                            transparentPixelCount++;
                        }
                        newTex.SetPixel(width, height, pixel);
                    }
                }

                if (transparentPixelCount != tileSize*tileSize) {
                    Tile t = new Tile();
                    t.data = new JsonObjectHandler.ObjectData();
                    t.data.ID = tileIndex;
                    t.data.ObjectType = "Tiles";
                    t.data.ObjectName = tileNameInput.text + "_" + tileIndex;
                    t.data.ObjectPack = packNameInput.text;
                    t.data.SpriteSize = tileSize;
                    t.data.SpriteTint = Color.white;
                    t.data.ObjectCollider = "BoxCollider";
                    t.texture = newTex;
                    tiles.Add(t);
                    tileIndex++;
                }
            }
        }

        string packPath = Application.streamingAssetsPath + "/Objects/" + packNameInput.text;
        if (!Directory.Exists(packPath)) {
            Directory.CreateDirectory(packPath);
        }

        foreach (Tile t in tiles) {
            byte[] content = t.texture.EncodeToPNG();
            string tilePath = packPath + "/" + t.data.ObjectName;

            if (!Directory.Exists(tilePath)) {
                Directory.CreateDirectory(tilePath);
            }

            File.WriteAllBytes(tilePath + "/" + "Sprite" + ".png", content);
            File.WriteAllText(tilePath + "/" + "Data.json", JsonUtility.ToJson(t.data,true));
        }

        StartCoroutine(SetMessage(message));
    }

    private bool IsValid(out string message) {
        if(packNameInput.text == "" && tileNameInput.text == "") {
            message = "Error : Pack name and File name can't be empty";
            return false;
        }

        if(packNameInput.text == "") {
            message = "Error: Pack name can't be empty";
            return false;
        }
        else if (tileNameInput.text == "") {
            message = "Error: File name can't be empty";
            return false;
        }

        message = "Saved";
        return true;
    }

    private IEnumerator SetMessage(string message) {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        yield return new WaitForSeconds(3);
        messageText.gameObject.SetActive(false);
    }

    private void GetInputInfo() {
        tileCount = new Vector2Int(int.Parse(tilesetSizeX.text), int.Parse(tilesetSizeY.text));
        tileSize = int.Parse(tileDimension.text);
    }

    private void SetPreviewScale(int scale) {
        tileSetPreview.GetComponent<RectTransform>().sizeDelta = tileCount * scale;
    }
}
