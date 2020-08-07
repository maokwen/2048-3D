using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int Level { get; set; }

    public TextMesh text;
    public MeshRenderer cubeRenderer;
    public MeshRenderer fontRenderer;

    void Awake()
    {
        ////fontRenderer.material.renderQueue = 2000;
        //cubeRenderer.material.renderQueue = 3000;
    }

    private int number;
    private Color32 fontColor;
    private Color32 bgColor;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(GrowUp());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ApplyStyle()
    {
        TileInfo style = TileInfoManager.Instance.tileInfos[Level];
        cubeRenderer.material.color = style.BackgroundColor;
        text.color = style.FontColor;
        text.text = style.Number.ToString();
        text.fontSize = style.FontSize;

        StartCoroutine(ScaleY(style.yScale));
    }

    //private IEnumerator GrowUp()
    //{
    //    while (gameObject.transform.localScale.x < 1.1f && gameObject.transform.localScale.z < 1.1f)
    //    {
    //        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x + 0.005f, gameObject.transform.localScale.y, gameObject.transform.localScale.z + 0.005f);
    //        yield return null;
    //    }

    //    while (gameObject.transform.localScale.x >= 1f && gameObject.transform.localScale.z >= 1f)
    //    {
    //        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - 0.005f, gameObject.transform.localScale.y, gameObject.transform.localScale.z - 0.005f);
    //        yield return null;
    //    }

    //    gameObject.transform.localScale = new Vector3(1, gameObject.transform.localScale.y, 1);
    //}

    private IEnumerator ScaleY(float yScale)
    {
        while (gameObject.transform.localScale.y < yScale)
        {
            gameObject.transform.localScale = new Vector3(1, gameObject.transform.localScale.y + yScale * 10 * Time.deltaTime, 1);
            yield return null;
        }
        gameObject.transform.localScale = new Vector3(1, yScale, 1);
    }
}
