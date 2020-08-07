using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    public int score { get; set; }

    private TextMesh text;

    private void Awake()
    {
        text = GetComponent<TextMesh>();
        text.text = "0";
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator ChangScore()
    {
        yield return StartCoroutine(OnChangeStart());
        text.text = score.ToString();
        yield return StartCoroutine(OnChangeFinished());
    }

    private IEnumerator OnChangeStart()
    {
        while (gameObject.transform.localScale.x < 1.3f && gameObject.transform.localScale.y > 0f)
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x + 0.3f / 10, gameObject.transform.localScale.y - 1.0f / 10, 1f);
            yield return null;
        }
        gameObject.transform.localScale = new Vector3(1.3f, 0.0f, 1f);
    }

    private IEnumerator OnChangeFinished()
    {
        while (gameObject.transform.localScale.x > 1f && gameObject.transform.localScale.y < 1f)
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - 0.3f / 10, gameObject.transform.localScale.y + 1.0f / 10, 1f);
            yield return null;
        }
        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
