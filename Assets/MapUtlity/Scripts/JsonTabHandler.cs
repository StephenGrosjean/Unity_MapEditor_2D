using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
public class JsonTabHandler : MonoBehaviour
{
    private const string tabFolderName = "Tabs";
    public List<Tab> Tabs = new List<Tab>();
    
    [Serializable]
    public class TabWrapper
    {
        public List<Tab> tabList;
    }

    [Serializable]
    public class Tab
    {
       public string tab;
    }

    private void Awake() {
        //GenerateTabFile();
        LoadTabs();
    }

    private void LoadTabs() {
        TabWrapper tabs = JsonUtility.FromJson<TabWrapper>(File.ReadAllText(Application.streamingAssetsPath + "/" + tabFolderName + "/" + "tabs.json"));
        Tabs = tabs.tabList;

        /*//Create Default Tab
        Tab defaultTab = new Tab();
        defaultTab.tab = "Default";
        Tabs.Insert(0, defaultTab);*/
    }

    private void GenerateTabFile() {
        Tab tab1 = new Tab();
        Tab tab2 = new Tab();
        Tab tab3 = new Tab();

        tab1.tab = "Tab_1";
        tab2.tab = "Tab_2";
        tab3.tab = "Tab_3";

        Tabs.Add(tab1);
        Tabs.Add(tab2);
        Tabs.Add(tab3);

        TabWrapper tabWrapper = new TabWrapper();
        tabWrapper.tabList = Tabs;

        File.WriteAllText(Application.streamingAssetsPath + "/" + tabFolderName + "/tabs.json", JsonUtility.ToJson(tabWrapper, true));
    }
}
