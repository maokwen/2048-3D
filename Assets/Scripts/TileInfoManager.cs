using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileInfo
{
    public int Number;
    public Color32 FontColor;
    public Color32 BackgroundColor;
    public float yScale;
    public int FontSize;
}

public class TileInfoManager : MonoBehaviour
{
    public static TileInfoManager Instance { get; set; }

    public List<TileInfo> tileInfos;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
