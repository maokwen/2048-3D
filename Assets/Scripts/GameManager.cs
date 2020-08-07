using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GameStatus
{
    Title,
    Playing,
    GameOver,
    Waiting
}

public class GameManager : MonoBehaviour
{
    [Range(0, 100)]
    public float MoveSpeed;
    private GameStatus Status;

    public GameObject playgrouond;
    public Score score;
    public GameObject titlePanel;
    public GameObject gameoverPanel;
    public GameObject tilePrefab;
    public TextMesh scoreText;

    private readonly Tile[,] tiles = new Tile[4, 4];
    public readonly List<Transform> slots = new List<Transform>();

    Dictionary<Transform, Transform> moveList = new Dictionary<Transform, Transform>();
    Dictionary<Tile, Tile> mergeList = new Dictionary<Tile, Tile>();

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 300;

        // get slots
        foreach (var child in playgrouond.transform)
        {
            slots.Add(child as Transform);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Status = GameStatus.Title;
    }

    // Update is called once per frame
    void Update()
    {
        //    if (Input.GetKeyDown(KeyCode.G))
        //    {
        //        StartCoroutine(GenerateTile());
        //    }

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(GameOver());
        }
    }

    #region tile generate

    private bool CanMove()
    {
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                if (tiles[i, j] == null) return true;

                if (i - 1 >= 0)
                {
                    if (tiles[i - 1, j] == null) return true;
                    if (tiles[i, j].Level == tiles[i - 1, j].Level) return true;
                }
                if (i + 1 <= 3)
                {
                    if (tiles[i + 1, j] == null) return true;
                    if (tiles[i, j].Level == tiles[i + 1, j].Level) return true;
                }
                if (j - 1 >= 0)
                {
                    if (tiles[i, j - 1] == null) return true;
                    if (tiles[i, j].Level == tiles[i, j - 1].Level) return true;
                }
                if (j + 1 <= 3)
                {
                    if (tiles[i, j + 1] == null) return true;
                    if (tiles[i, j].Level == tiles[i, j + 1].Level) return true;
                }
            }
        }
        return false;
    }

    private bool EmptyTileExists()
    {
        foreach (var t in tiles)
        {
            if (t == null)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator GenerateTile()
    {
        if (EmptyTileExists() == false)
        {
            yield break;
        }

        int row, col;
        do
        {
            var i = UnityEngine.Random.Range(0, 16);
            row = i / 4;
            col = i % 4;
        } while (tiles[row, col] != null);

        GameObject newTile = Instantiate(tilePrefab, slots[row * 4 + col].transform);
        Tile t = newTile.GetComponent<Tile>();
        tiles[row, col] = t;
        t.ApplyStyle();
    }

    #endregion

    #region movement

    public IEnumerator DoMovement(MoveDirection direct)
    {
        while (Status != GameStatus.Playing)
        {
            yield return null;
        }

        if (!CanMove())
        {
            StartCoroutine(GameOver());
            yield break;
        }

        Status = GameStatus.Waiting;
        //Debug.Log("Move " + direct);

        moveList.Clear();
        mergeList.Clear();

        switch (direct)
        {
            case MoveDirection.Left:
                MoveLeft(moveList, mergeList);
                break;
            case MoveDirection.Right:
                MoveRight(moveList, mergeList);
                break;
            case MoveDirection.Up:
                MoveUp(moveList, mergeList);
                break;
            case MoveDirection.Down:
                MoveDown(moveList, mergeList);
                break;
            default:
                break;
        }

        if (moveList.Count == 0 && mergeList.Count == 0)
        {
            StartCoroutine(PopPanel());
            yield break;
        }

        List<Coroutine> coroutineList = new List<Coroutine>();

        foreach (var t in moveList)
        {
            coroutineList.Add(StartCoroutine(ShiftTile(t.Key, t.Value)));
        }
        foreach (var t in mergeList)
        {
            coroutineList.Add(StartCoroutine(MergeTile(t.Key, t.Value)));
        }

        foreach (var c in coroutineList)
        {
            yield return c;
        }

        //LogTileSet();

        if (mergeList.Count != 0)
        {
            yield return StartCoroutine(score.ChangScore());
        }

        yield return StartCoroutine(GenerateTile());

        Status = GameStatus.Playing;
    }

    private void MoveLeft(Dictionary<Transform, Transform> moveList, Dictionary<Tile, Tile> mergeList)
    {
        // for each row
        for (int row = 0; row < 4; ++row)
        {
            bool[] isFix = { false, false, false, false };

            // find tile
            for (int i = 0; i < 4; ++i)
            {
                if (tiles[row, i] == null)
                {
                    continue;
                }

                // find next tile not empty
                for (int j = i - 1; j >= 0; --j)
                {
                    if (tiles[row, j] == null)
                    {
                        if (j == 0)
                        {
                            // 0. found reach end
                            tiles[row, j] = tiles[row, i];
                            tiles[row, i] = null;
                            moveList.Add(tiles[row, j].gameObject.transform, slots[row * 4 + j].transform);

                            break;
                        }

                        // 1. skip empty
                        continue;
                    }

                    // 2. found tile can merge, merge and break
                    if (tiles[row, j].Level == tiles[row, i].Level && !isFix[j])
                    {
                        // merge and set fixed
                        tiles[row, j].Level += 1;
                        isFix[j] = true;
                        mergeList.Add(tiles[row, i], tiles[row, j]);
                        tiles[row, i] = null;

                        break;
                    }
                    else // 3. found tile cannot merge, move to its left empty
                    {
                        if (j + 1 == i)
                        {
                            break;
                        }
                        tiles[row, j + 1] = tiles[row, i];
                        tiles[row, i] = null;

                        moveList.Add(tiles[row, j + 1].gameObject.transform, slots[row * 4 + j + 1].transform);

                        break;
                    }
                }
            }
        }
    }

    private void MoveRight(Dictionary<Transform, Transform> moveList, Dictionary<Tile, Tile> mergeList)
    {

        // for each row
        for (int row = 0; row < 4; ++row)
        {
            bool[] isFix = { false, false, false, false };

            // find tile
            for (int i = 3; i >= 0; --i)
            {
                if (tiles[row, i] == null)
                {
                    continue;
                }

                // find next tile not empty
                for (int j = i + 1; j <= 3; ++j)
                {
                    if (tiles[row, j] == null)
                    {
                        if (j == 3)
                        {
                            // 0. found reach end
                            tiles[row, j] = tiles[row, i];
                            tiles[row, i] = null;
                            moveList.Add(tiles[row, j].gameObject.transform, slots[row * 4 + j].transform);

                            break;
                        }

                        // 1. skip empty
                        continue;
                    }

                    // 2. found tile can merge, merge and break
                    if (tiles[row, j].Level == tiles[row, i].Level && !isFix[j])
                    {
                        // merge and set fixed
                        tiles[row, j].Level += 1;
                        isFix[j] = true;
                        mergeList.Add(tiles[row, i], tiles[row, j]);
                        tiles[row, i] = null;

                        break;
                    }
                    else // 3. found tile cannot merge, move to its left empty
                    {
                        if (j - 1 == i)
                        {
                            break;
                        }
                        tiles[row, j - 1] = tiles[row, i];
                        tiles[row, i] = null;
                        moveList.Add(tiles[row, j - 1].gameObject.transform, slots[row * 4 + j - 1].transform);

                        break;
                    }
                }
            }
        }
    }

    private void MoveUp(Dictionary<Transform, Transform> moveList, Dictionary<Tile, Tile> mergeList)
    {
        // for each row
        for (int col = 0; col < 4; ++col)
        {
            bool[] isFix = { false, false, false, false };

            // find tile
            for (int i = 0; i < 4; ++i)
            {
                if (tiles[i, col] == null)
                {
                    continue;
                }

                // find next tile not empty
                for (int j = i - 1; j >= 0; --j)
                {
                    if (tiles[j, col] == null)
                    {
                        if (j == 0)
                        {
                            // 0. found reach end
                            tiles[j, col] = tiles[i, col];
                            moveList.Add(tiles[j, col].gameObject.transform, slots[j * 4 + col].transform);
                            tiles[i, col] = null;

                            break;
                        }

                        // 1. skip empty
                        continue;
                    }

                    // 2. found tile can merge, merge and break
                    if (tiles[j, col].Level == tiles[i, col].Level && !isFix[j])
                    {
                        // merge and set fixed
                        tiles[j, col].Level += 1;
                        isFix[j] = true;
                        mergeList.Add(tiles[i, col], tiles[j, col]);
                        tiles[i, col] = null;

                        break;
                    }
                    else // 3. found tile cannot merge, move to its left empty
                    {
                        if (j + 1 == i)
                        {
                            break;
                        }
                        tiles[j + 1, col] = tiles[i, col];
                        tiles[i, col] = null;
                        moveList.Add(tiles[j + 1, col].gameObject.transform, slots[(j + 1) * 4 + col].transform);

                        break;
                    }
                }
            }
        }
    }

    private void MoveDown(Dictionary<Transform, Transform> moveList, Dictionary<Tile, Tile> mergeList)
    {
        // for each row
        for (int col = 0; col < 4; ++col)
        {
            bool[] isFix = { false, false, false, false };

            // find tile
            for (int i = 3; i >= 0; --i)
            {
                if (tiles[i, col] == null)
                {
                    continue;
                }

                // find next tile not empty
                for (int j = i + 1; j <= 3; ++j)
                {
                    if (tiles[j, col] == null)
                    {
                        if (j == 3)
                        {
                            // 0. found reach end
                            tiles[j, col] = tiles[i, col];
                            tiles[i, col] = null;
                            moveList.Add(tiles[j, col].gameObject.transform, slots[j * 4 + col].transform);

                            break;
                        }

                        // 1. skip empty
                        continue;
                    }

                    // 2. found tile can merge, merge and break
                    if (tiles[j, col].Level == tiles[i, col].Level && !isFix[j])
                    {
                        // merge and set fixed
                        tiles[j, col].Level += 1;
                        isFix[j] = true;
                        mergeList.Add(tiles[i, col], tiles[j, col]);
                        tiles[i, col] = null;

                        break;
                    }
                    else // 3. found tile cannot merge, move to its left empty
                    {
                        if (j - 1 == i)
                        {
                            break;
                        }
                        tiles[j - 1, col] = tiles[i, col];
                        tiles[i, col] = null;
                        moveList.Add(tiles[j - 1, col].gameObject.transform, slots[(j - 1) * 4 + col].transform);

                        break;
                    }
                }
            }
        }
    }

    #endregion

    #region game over

    private IEnumerator GameOver()
    {
        if (Status != GameStatus.Playing && Status != GameStatus.Waiting)
        {
            yield break;
        }

        Status = GameStatus.GameOver;

        scoreText.text = score.score.ToString();

        playgrouond.GetComponent<Animator>().SetTrigger("GameOver");
        while (AnimatorIsPlaying(playgrouond.GetComponent<Animator>(), "PullOut"))
        {
            yield return null;
        }
        gameoverPanel.SetActive(true);
    }

    public IEnumerator Restart()
    {
        if (Status != GameStatus.GameOver && Status != GameStatus.Title)
        {
            yield break;
        }

        StopAllCoroutines();

        if (Status == GameStatus.GameOver)
        {
            foreach (var t in tiles)
            {
                if (t != null)
                {
                    Destroy(t.gameObject);
                }
            }
            Array.Clear(tiles, 0, tiles.Length);
            moveList.Clear();
            mergeList.Clear();
            score.score = 0;
            StartCoroutine(score.ChangScore());

            playgrouond.GetComponent<Animator>().SetTrigger("ReStart");
            while (AnimatorIsPlaying(playgrouond.GetComponent<Animator>(), "PullOut"))
            {
                yield return null;
            }
        }

        titlePanel.SetActive(false);
        gameoverPanel.SetActive(false);

        Status = GameStatus.Playing;
        StartCoroutine(GenerateTile());
    }

    #endregion

    #region debug function

    private void LogTileSet()
    {
        string log = "";
        for (var i = 0; i < 4; ++i)
        {
            for (var j = 0; j < 4; ++j)
            {
                if (tiles[i, j] == null)
                {
                    log += 'x';
                }
                else
                {
                    log += tiles[i, j].Level;
                }
                log += ' ';
            }
            log += '\n';
        }

        Debug.Log(log);
    }

    #endregion

    #region animation

    private bool moveTo(Transform from, Transform to)
    {
        Vector3 dis = to.position - from.position;
        if (dis.magnitude < MoveSpeed * Time.deltaTime)
        {
            from.SetParent(to, false);
            from.localPosition = Vector3.zero;

            return false;
        }
        from.position += dis.normalized * MoveSpeed * Time.deltaTime;
        return true;
    }

    private IEnumerator MergeTile(Tile from, Tile to)
    {
        while (moveTo(from.transform, to.transform))
        {
            yield return null;
        }

        Destroy(from.gameObject);
        to.ApplyStyle();
        to.transform.position = new Vector3(to.transform.position.x, to.transform.position.y, to.transform.position.z);

        score.score += 2 << to.Level;

        if (to.Level == 9)
        {
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator ShiftTile(Transform from, Transform to)
    {
        while (moveTo(from.transform, to.transform))
        {
            yield return null;
        }
    }

    private IEnumerator PopPanel()
    {
        Status = GameStatus.Waiting;
        playgrouond.GetComponent<Animator>().SetTrigger("NotMove");
        while (AnimatorIsPlaying(playgrouond.GetComponent<Animator>(), "Pop"))
        {
            yield return null;
        }
        Status = GameStatus.Playing;
    }

    bool AnimatorIsPlaying(Animator animator, string stateName)
    {
        return AnimatorIsPlaying(animator) && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    bool AnimatorIsPlaying(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).length >
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    #endregion
}
