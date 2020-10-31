using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public static PlayerController _instance = null;

    public float speed = 10.0f;
    public Transform movePoint;

    public LayerMask _playArea;

    public Tilemap tilemap;

    public Tile wall;

    public Tile _powerUp;

    public Transform _playerSprite;

    public List<Vector3Int> _tileBasesPos = new List<Vector3Int>();

    public Vector3Int _randomItemPos;

    public List<Vector3Int> _wallTilePos = new List<Vector3Int>();

    public int gridHeight;
    public int gridWidth;

    int totalGameLives = 3;
    public int currentGameLives = 3;

    public bool PowerUp = false;

    float powerUpTimer = 5f;

    public TMP_Text _life;
    public TMP_Text _progress;

    public GameObject _reset;

    public VariableJoystick fixedJoystick;

    IDictionary<Vector3, Vector3> nodeParents = new Dictionary<Vector3, Vector3>();

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this.GetComponent<PlayerController>();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
        _playerSprite.localEulerAngles = Vector3.zero;
        GetAllPos();

        _life.text = "Life : " + totalGameLives;
        _reset.SetActive(false);

        Invoke("GeneratePowerUp", 10f);
    }

    void GetAllPos()
    {
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                Vector3Int localPos = new Vector3Int(i, j, 0);
                if (i == 0 || j == 0 || i == (gridWidth - 1) || j == (gridHeight - 1))
                {
                    tilemap.SetTile(localPos, wall);
                    _wallTilePos.Add(localPos);
                }
                else
                {
                    _tileBasesPos.Add(localPos);
                }
            }
        }

        _progress.text = "Progress : " + AreaCoveredInPercentage().ToString() + "/80";
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position,movePoint.position, speed * Time.deltaTime);

        Vector3Int cell = tilemap.WorldToCell(transform.position);
        Debug.Log(tilemap.size);

        Tile tile = tilemap.GetTile<Tile>(cell);

        if(tile == null)
        {
            tilemap.SetTile(cell, wall);
            _wallTilePos.Add(cell);
            _tileBasesPos.Remove(cell);
            Debug.Log(AreaCoveredInPercentage());
            _progress.text = "Progress : " + AreaCoveredInPercentage().ToString() + "/80";
        }
        else if(tile == _powerUp)
        {
            tilemap.SetTile(cell, wall);
            _wallTilePos.Add(cell);
            _tileBasesPos.Remove(cell);
            PowerUp = true;
        }

        if (Vector3.Distance(transform.position,movePoint.position) <= 0.05f)
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Mathf.Abs(fixedJoystick.Horizontal) == 1)
            {
                if (Physics2D.OverlapCircle(movePoint.position + new Vector3(fixedJoystick.Horizontal, 0, 0), 0.2f, _playArea))
                {
                    movePoint.position += new Vector3(fixedJoystick.Horizontal, 0, 0);
                    if (fixedJoystick.Horizontal > 0)
                    {
                        _playerSprite.localEulerAngles = Vector3.zero;
                    }
                    else
                    {
                        _playerSprite.localEulerAngles = new Vector3(0,0,180f);
                    }
                }
            }

            else if (Mathf.Abs(fixedJoystick.Vertical) == 1)
            {
                if (Physics2D.OverlapCircle(movePoint.position + new Vector3(0, fixedJoystick.Vertical, 0), 0.2f, _playArea))
                {
                    movePoint.position += new Vector3(0, fixedJoystick.Vertical, 0);

                    if (fixedJoystick.Vertical > 0)
                    {
                        _playerSprite.localEulerAngles = new Vector3(0, 0, 90f);
                    }
                    else
                    {
                        _playerSprite.localEulerAngles = new Vector3(0, 0, 270f);
                    }
                }
            }

#else
            if (Mathf.Abs(Input.GetAxis("Horizontal")) == 1)
            {
                if (Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.GetAxis("Horizontal"), 0, 0), 0.2f, _playArea))
                {
                    movePoint.position += new Vector3(Input.GetAxis("Horizontal"), 0, 0);
                    if (Input.GetAxis("Horizontal") > 0)
                    {
                        _playerSprite.localEulerAngles = Vector3.zero;
                    }
                    else
                    {
                        _playerSprite.localEulerAngles = new Vector3(0, 0, 180f);
                    }
                }
            }
            else if (Mathf.Abs(Input.GetAxis("Vertical")) == 1)
            {
                if (Physics2D.OverlapCircle(movePoint.position + new Vector3(0, Input.GetAxis("Vertical"), 0), 0.2f, _playArea))
                {
                    movePoint.position += new Vector3(0, Input.GetAxis("Vertical"), 0);

                    if (Input.GetAxis("Vertical") > 0)
                    {
                        _playerSprite.localEulerAngles = new Vector3(0, 0, 90f);
                    }
                    else
                    {
                        _playerSprite.localEulerAngles = new Vector3(0, 0, 270f);
                    }
                }
            }

#endif


        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (PowerUp && powerUpTimer > 0)
        {
            powerUpTimer -= Time.deltaTime;
        }
        else
        {
            PowerUp = false;
        }

        if(totalGameLives <= 0)
        {

        }
    }

    void GeneratePowerUp()
    {
        if (PowerUp)
        {
            Invoke("GeneratePowerUp", 5);
        }
        else
        {
            Tile tile = tilemap.GetTile<Tile>(_randomItemPos);
            if (tile == _powerUp)
            {
                tilemap.SetTile(_randomItemPos, null);
            }

            int _randomNumber = Random.Range(0, _tileBasesPos.Count);
            _randomItemPos = _tileBasesPos[_randomNumber];
            tilemap.SetTile(_tileBasesPos[_randomNumber], _powerUp);
            powerUpTimer = 5f;
            Invoke("GeneratePowerUp", 10);
        }
    }

    public int AreaCoveredInPercentage()
    {
        float requiredTiles = (((float)gridHeight * (float)gridWidth));
        int currentTiles = Mathf.FloorToInt(((float)_wallTilePos.Count / requiredTiles) * 100);
        if (currentTiles >= 80)
        {
            _reset.SetActive(true);
            Time.timeScale = 0;
        }
        return currentTiles;
    }

    public void LifeUpdate()
    {
        _life.text = "Life : " + --currentGameLives;
        if(currentGameLives <= 0)
        {
            _life.text = "Life : 0";
            _reset.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void RestartTheGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    //void FindClosedPolygon(Vector3Int endPoint , Vector3Int direction)
    //{
    //    Vector3Int StartPoint = endPoint + direction;

    //    Tile tile = tilemap.GetTile<Tile>(StartPoint);

    //    List<Vector3Int> ClosedPolygons1= new List<Vector3Int>();
    //    List<Vector3Int> ClosedPolygons2 = new List<Vector3Int>();
    //    List<Vector3Int> ClosedPolygons3 = new List<Vector3Int>();
    //    List<Vector3Int> ClosedPolygons4 = new List<Vector3Int>();

    //    if (tile == wall)
    //    {
    //        if(direction != Vector3Int.up)
    //        {

    //        }
    //    }

        

    //}

    //Vector3 FindShortestPathBFS(Vector3 startPosition, Vector3 goalPosition)
    //{
    //    uint nodeVisitCount = 0;
    //    float timeNow = Time.realtimeSinceStartup;

    //    Queue<Vector3> queue = new Queue<Vector3>();
    //    HashSet<Vector3> exploredNodes = new HashSet<Vector3>();
    //    queue.Enqueue(startPosition);

    //    while (queue.Count != 0)
    //    {
    //        Vector3 currentNode = queue.Dequeue();
    //        nodeVisitCount++;

    //        if (currentNode == goalPosition)
    //        {

    //            return currentNode;
    //        }

    //        List<Vector3Int> nodes = _wallTilePos.ToList<Vector3Int>();

    //        foreach (Vector3 node in nodes)
    //        {
    //            if (!exploredNodes.Contains(node))
    //            {
    //                //Mark the node as explored
    //                exploredNodes.Add(node);

    //                //Store a reference to the previous node
    //                nodeParents.Add(node, currentNode);

    //                //Add this to the queue of nodes to examine
    //                queue.Enqueue(node);
    //            }
    //        }
    //    }

    //    return startPosition;
    //}
}
