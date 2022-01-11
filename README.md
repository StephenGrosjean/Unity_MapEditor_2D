

# Unity 2D Map editor
### Features

- Edit maps in 2D within the game
- Tile based
- Mod support

## Requirements
- Unity Engine (Any version should work) [Tested on 2020.3.25f1]
- TextMeshPro package

## How to use

### Editor Scene
The editor scene located under **MapUtlity/Scenes/MapEditor** is the main editor for your maps.

### Game Scene
The games cene located under **MapUtlity/Scenes/GameScene**, with the *GameMapLoader* prefab you will be able to select a map and load it.

## Add content (Modding)
The editor support modding as it load the tiles using *JsonUtility*

###  Objects
You can add objects under the **StreamingAssets/Objects** folder.

#### Objects structure
The objects structure in the **Objects** folder need to be like this, *Sprite.png* can be named differently
```
|--Objects
    |--MyObject
        |--Data.json
        |--Sprite.png
```

#### Objects formating
The Json need to contain the following data:
```json
{
    "ID":0,
    "ObjectType": "Tiles",
    "ObjectName": "Tile_00", 
    "SpriteName": "Tile_00",
    "SpriteSize" :  256,
    "MaxPerScene" : 1,
    "SpriteTint": {
        "r": 255.0,
        "g": 255.0,
        "b": 255.0,
        "a": 255.0
    },
    "ObjectCollider": "BoxCollider"
}
```
**ID** : This is the unique ID of the object, now the tiles for map construction are ranged from **0 to 99** and the special components like *Player spawns* and *Weapon spawns* objects are ranged from **100 to 199**, ***Do not have two objects with the same ID, it will just pick the first one in the map loading process***

**ObjectType** : The object type is used to determine the tab in the editor *(Tabs listed in **StreamingAssets/Tabs**)* 

**SpriteName** : Used to find the sprite name (Maybe it will not be used after)

**SpriteSize** : The tile need to be a square (Try to use a power of 2 for the size) (256 work fine)

**MaxPerScene** : (Optional) You don't need to include it in the json file, but if you do and input a number higher than 0, it will limit the amount of this object in the map editor

**SpriteTint** : The tint of the sprite in RGBA.

**ObjectCollider** : Collider that will be applied to the object when instantiated, are allowed: ***BoxCollider***, ***CircleCollider***, ***PolygonCollider***

### Backgrounds
You can add backgrounds under the **StreamingAssets/Backgrounds** folder.

### Tabs
Tabs are defined under **StreamingAssets/Tabs** folder.

#### Tabs Json format
```json
{
    "tabList": [
        {
            "tab": "Tiles"
        },
        {
            "tab": "Pickups"
        },
        {
            "tab": "Level"
        }
    ]
}
``` 

## Components
#### JsonObjectHandler.cs
This component is used to load the tiles, backgrounds and maps and tabs located under **StreamingAssets/** folder.
This is where the logic of saving/loading takes place.

#### ObjectSelector.cs
This component is for the editor UI.

#### MapEditor.cs
This is where the objects will be instantiated in the scene and the map objects will be logged.

