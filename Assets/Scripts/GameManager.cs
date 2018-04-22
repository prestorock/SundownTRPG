using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*===================================
Project:	SundownTRPG	
Developer:	Preston Rockholt Prockho0@email.cpcc.edu
Company:	DefaultCompany
Date:		27/03/2018 22:46
-------------------------------------
Description:

===================================*/

public class GameManager : MonoBehaviour
{
    public static GameManager _gm; //global self reference. be careful.

    #region Public Variables
    public Board _board { get; internal set; } //let them access the board, but not change it.
    public Pathfinding _pathing { get; internal set; }// ditto
    public GameObject groundGroup;//this is the bottom left corner of our whole map. located at the origin (0,0).
    public Camera mCamera; //use this instead of Camera.main its more efficient.
    [HideInInspector]
    public Vector3 selectPosition = Vector2.zero; //stores the selectionSquares position so we can make it private.
    public bool selectionLocked = false;
    #endregion

    #region Private Variables
    [SerializeField]
    private GameObject mapObjectPrefab; //set in inspector, but no one needs to access it otherwise.
    [SerializeField]
    private Vector2 mapSize; //holds the size of the map for reference by the board and others. this is the master setting.
    [SerializeField]
    private SelectionSquare selectionSquare;
    [SerializeField]
    private Text selectionText; //can change the infobox's text with the methods below
    [SerializeField]
    private GameObject devPanelObj;

    #endregion


    private void Awake()
    {
        _gm = this;
        mCamera = Camera.main;
    }

    private void Start()
    {
        _board = GetComponent<Board>();
        _pathing = GetComponent<Pathfinding>();

        _board.CreateMap(mapSize); //the board holds all the map information now.

        mCamera.transform.position = new Vector3(mapSize.x * 0.5f, mCamera.transform.position.y, mapSize.y * 0.25f); //set the camera at the center of the map.
        selectPosition = new Vector3(Mathf.Floor(mapSize.x * 0.5f), 0.01f, Mathf.Floor(mapSize.y * 0.5f)); //selection set to the very center of the map. (mapSize floored)
    }

    private void Update()
    {
        if (Time.timeScale != 0) //ANYTHING IN THIS BLOCK ADHERES TO PAUSING
        {
            GetInput();//orgainzes input code
            SelectionMenus(); //checks for if the menus should be visible.
        }                       //END OF PAUSING BLOCK

    }

    private void SelectionMenus()
    {
        if(selectionLocked)
        {
            if(!devPanelObj.activeInHierarchy)
            {
                devPanelObj.SetActive(true);
            }
            //show menus
        }
        else
        {
            if (devPanelObj.activeInHierarchy)
            {
                devPanelObj.SetActive(false);
            }
            //hide menus
        }
    }
    //returns the mapsize set in the inspector. this is used to determine any and all things that constrain to the max size like the board or the map.
    public Vector2 MapSize()
    {
        return mapSize;
    }

    #region Spawning MapObjects
    public MapObject SpawnMapObjectAtSelection(MapObject.ObjectType type) //for use in code.
    {
        MapObject mapObject;
        mapObject = Instantiate(mapObjectPrefab).GetComponent<MapObject>();
        mapObject.transform.position = selectPosition;
        mapObject.Initialize(MapObject.ObjectType.Building, (int)selectPosition.x, (int)selectPosition.z);
        _board.LinkToMap((int)selectPosition.x, (int)selectPosition.z, mapObject);
        return mapObject;
    }

    public void SpawnMapObjectAtSelectionButton(string type) //for use with unity scene buttons, you cant pass enum types through so we use a string or similar instead.
    {
        MapObject mapObject;
        if (_board.GetTileAt((int)selectPosition.x, (int)selectPosition.z).GetLinkedObject() != null)
        {
            Debug.Log("A MapObject already exists at that location.");
        }
        else
        {
            mapObject = Instantiate(mapObjectPrefab).GetComponent<MapObject>();
            mapObject.transform.position = selectPosition;

            if (type == "unit")
            {
                mapObject.Initialize(MapObject.ObjectType.Unit, (int)selectPosition.x, (int)selectPosition.z);
            }
            else if (type == "building")
            {
                mapObject.Initialize(MapObject.ObjectType.Building, (int)selectPosition.x, (int)selectPosition.z);

            }
            else
            {
                mapObject.Initialize(MapObject.ObjectType.Environment, (int)selectPosition.x, (int)selectPosition.z);

            }
            _board.LinkToMap((int)selectPosition.x, (int)selectPosition.z, mapObject);
            selectionLocked = false;
        }
    }
    #endregion

    private void GetInput()
    {
        #region Keyboard CAMERA Movement
        
        if (Input.GetKey(KeyCode.D)) //Right direction
        {
            if (selectPosition.x < mapSize.x - 1)
            {
                mCamera.GetComponent<CameraMovement>().MoveCamera( Vector3.right);
            }
        }
        else if (Input.GetKey(KeyCode.A)) // left
        {
            if (selectPosition.x > 0)
            {
                mCamera.GetComponent<CameraMovement>().MoveCamera( Vector3.left);
            }
        }

        if (Input.GetKey(KeyCode.W)) //up
        {
            if (selectPosition.z < mapSize.y - 1)
            {
                mCamera.GetComponent<CameraMovement>().MoveCamera( Vector3.forward);
            }
        }
        else if (Input.GetKey(KeyCode.S)) //down
        {
            if (selectPosition.z > 0)
            {
                mCamera.GetComponent<CameraMovement>().MoveCamera( Vector3.back);
            }
        }

        #endregion

        #region Keyboard SLECTION Movement (Disabled)
        /*
        if (Input.GetKeyDown(KeyCode.D)) //Right direction
        {
            if (selectPosition.x < mapSize.x - 1)
            {
                selectPosition += Vector3.right;
                UpdateInfobox(selectPosition);
            }
        }
        else if (Input.GetKeyDown(KeyCode.A)) // left
        {
            if (selectPosition.x > 0)
            {
                selectPosition += Vector3.left;
                UpdateInfobox(selectPosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.W)) //up
        {
            if (selectPosition.z < mapSize.y - 1)
            {
                selectPosition += Vector3.forward;
                UpdateInfobox(selectPosition);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S)) //down
        {
            if (selectPosition.z > 0)
            {
                selectPosition += Vector3.back;
                UpdateInfobox(selectPosition);
            }
        }
        
        if (Input.GetButtonDown("Submit"))
        {

            //selecting/interacting with menus
            //Debug.Log("Selected: " + tile.ToString());
        }
        */
        #endregion

        #region Mouse SELECTION Movement
        Ray ray = new Ray(mCamera.ScreenToWorldPoint(Input.mousePosition), mCamera.transform.forward);
        RaycastHit hit;
        if (!selectionLocked)
        {
            if (Physics.Raycast(ray, out hit, 1 << 9))
            {
                selectPosition.x = hit.transform.position.x; //could be simplified by storing x and z in a method in the floortile component?
                selectPosition.z = hit.transform.position.z;
                UpdateInfobox(selectPosition);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out hit, 1 << 9))
                {
                    selectPosition.x = hit.transform.position.x; //could be simplified by storing x and z in a method in the floortile component?
                    selectPosition.z = hit.transform.position.z;
                    UpdateInfobox(selectPosition);

                    selectionLocked = true;

                }
                //TODO: Activate selection menus
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (Physics.Raycast(ray, out hit, 1 << 9))
                {
                    selectionLocked = false;

                }
                //TODO: Activate selection menus
            }
        }
        #endregion


    }

    #region InfoBox + Overload Methods
    //updates any info we need whenever called. can be overloaded to take specifics as well if need be. example below.
    void UpdateInfobox() //defaults to selection's position
    {
        FloorTile tile = _board.GetTileAt(Mathf.FloorToInt(selectPosition.x), Mathf.FloorToInt(selectPosition.z));
        selectionText.text = tile.ToString();
    }
    
    public void UpdateInfobox(string s)
    {
        selectionText.text = s;
    }

    void UpdateInfobox(Vector2 v2)
    {
        FloorTile tile = _board.GetTileAt(Mathf.FloorToInt(v2.x), Mathf.FloorToInt(v2.y));
        selectionText.text = tile.ToString();
    }

    void UpdateInfobox(Vector3 v3)
    {
        FloorTile tile = _board.GetTileAt(Mathf.FloorToInt(v3.x), Mathf.FloorToInt(v3.z));
        selectionText.text = tile.ToString();
    }
    #endregion

}
