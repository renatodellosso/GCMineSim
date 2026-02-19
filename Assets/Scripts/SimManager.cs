using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SimManager : MonoBehaviour
{
    private Vector3 tractorSize;
    public float safetyMargin = 4f;
    public Vector2 fieldDimensions = new(20, 100);
    public float groundScale = 5;

    public float tractorSpeed = 1f;

    public int numMines = 10;
    public float mineArmedChance = 0.25f;

    public float cameraSpeed = 5f, zoomSpeed = 100f, cameraHeight = 20f, cameraAngle = 75f;

    public GameObject ground;
    public Transform cameraParent;
    public Camera mainCamera;
    public TMP_Text infoText;

    public GameObject tractorPrefab;
    public GameObject chainLinkPrefab;
    public GameObject weightPrefab;
    public GameObject minePrefab;

    public bool simulationStarted = false;
    private GameObject leftTractor, rightTractor;

    private readonly List<Mine> mines = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tractorSize = tractorPrefab.transform.localScale;

        InitSim();

        simulationStarted = true;
    }

    void FixedUpdate()
    {
        if (!simulationStarted)
            return;

        MoveTractors();
        HandleInput();
        UpdateUi();
    }

    private void MoveTractors()
    {
        leftTractor.transform.Translate(Time.fixedDeltaTime * tractorSpeed * Vector3.forward);
        rightTractor.transform.Translate(Time.fixedDeltaTime * tractorSpeed * Vector3.forward);        
    }

    private void HandleInput()
    {
        float horizontalMovement = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        float verticalMovement = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;
        float rotation = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
        float zoom = Input.mouseScrollDelta.y;

        cameraParent.transform.Translate(Time.fixedDeltaTime * horizontalMovement * cameraSpeed * Vector3.forward);
        cameraParent.transform.Translate(Time.fixedDeltaTime * verticalMovement * cameraSpeed * Vector3.up);
        cameraParent.transform.Rotate(Time.fixedDeltaTime * rotation * cameraSpeed * Vector3.up);
        mainCamera.orthographicSize += zoom * zoomSpeed * Time.fixedDeltaTime; // Zoom in/out with mouse scroll
    }

    private void UpdateUi()
    {
        int armedMines = mines.Count(m => m.armed);
        int hitMines = mines.Count(m => m.hit);

        infoText.text = $"Mines: {hitMines} hit / {armedMines} armed / {numMines} total";
    }

    private void InitSim()
    {
        SetUpGround();
        SpawnTractors();
        SpawnMines();
        PositionCamera();
    }

    private float GetTotalWidth()
    {
        return 2 * (safetyMargin + tractorSize.x / 2) + fieldDimensions.x;
    }

    private void SetUpGround()
    {
        float trueLength = fieldDimensions.y / 2 + tractorSize.z * 2;
        ground.transform.localScale = new Vector3(GetTotalWidth() / groundScale, 1, trueLength / groundScale);
        ground.transform.localPosition = new Vector3(0, 0, trueLength - tractorSize.z * 2); // Not sure why 5, but it puts the tractors at the start
    }

    private void SpawnTractors()
    {
        float leftTractorX = fieldDimensions.x / 2 + safetyMargin + tractorSize.x / 2;
        float rightTractorX = -leftTractorX;

        leftTractor = Instantiate(tractorPrefab, new Vector3(leftTractorX, tractorSize.y, 0), Quaternion.identity);
        rightTractor = Instantiate(tractorPrefab, new Vector3(rightTractorX, tractorSize.y, 0), Quaternion.identity);

        leftTractor.transform.localScale = tractorSize;
        rightTractor.transform.localScale = tractorSize;

        CreateChain();
    }

    private void CreateChain()
    {
        float minWeightX = rightTractor.transform.position.x + tractorSize.x / 2 + safetyMargin;
        float maxWeightX = leftTractor.transform.position.x - tractorSize.x / 2 - safetyMargin;

        GameObject leftHitch = leftTractor.transform.Find("Hitch").gameObject;
        GameObject rightHitch = rightTractor.transform.Find("Hitch").gameObject;

        GameObject chain = new("Chain");

        GameObject firstLink = null;

        Transform currentLink = leftHitch.transform.Find("Next");
        while (currentLink.position.x > rightHitch.transform.position.x)
        {
            GameObject newLink = Instantiate(chainLinkPrefab, currentLink.position, Quaternion.LookRotation(new Vector3(0, 0, -1)));
            newLink.transform.SetParent(chain.transform);

            // Transform prevLink2 = currentLink.parent.Find("Link 2");
            // if (prevLink2 != null)
            //     prevLink2.GetComponent<HingeJoint>().connectedBody = newLink.transform.Find("Link 1").GetComponent<Rigidbody>();

            if (firstLink == null)
                firstLink = newLink;

            Transform loop = newLink.transform.Find("Loop");
            if (loop.position.x > minWeightX && loop.position.x < maxWeightX)
            {
                GameObject weight = Instantiate(weightPrefab, loop.position, Quaternion.identity);
                weight.transform.SetParent(chain.transform);
            }

            currentLink = newLink.transform.Find("Next");
        }

        leftTractor.GetComponent<HingeJoint>().connectedBody = firstLink.transform.Find("Link 1").GetComponent<Rigidbody>();
        rightTractor.GetComponent<HingeJoint>().connectedBody = currentLink.parent.Find("Link 2").GetComponent<Rigidbody>();
    }

    private void PositionCamera()
    {
        // Put camera above a corner of the field

        cameraParent.position = new Vector3(-GetTotalWidth() / 2, 0, -fieldDimensions.y / 2);
        mainCamera.transform.SetPositionAndRotation(new Vector3(0, cameraHeight, 0), Quaternion.Euler(cameraAngle, cameraAngle, 0));
    }

    private void SpawnMines()
    {
        float minMineX = rightTractor.transform.position.x + tractorSize.x / 2 + safetyMargin;
        float maxMineX = leftTractor.transform.position.x - tractorSize.x / 2 - safetyMargin;

        GameObject mineParent = new("Mines");

        for (int i = 0; i < numMines; i++)
        {
            float x = Random.Range(minMineX, maxMineX);
            float y = minePrefab.transform.localScale.y + 0.1f;
            float z = Random.Range(0, fieldDimensions.y);

            bool armed = Random.value < mineArmedChance;
            SpawnMine(new Vector3(x, y, z), armed, mineParent);
        }
    }

    private void SpawnMine(Vector3 position, bool armed, GameObject parent)
    {
        GameObject mine = Instantiate(minePrefab, position, Quaternion.identity);
        mine.transform.SetParent(parent.transform);

        Mine mineScript = mine.GetComponent<Mine>();
        mineScript.armed = armed;
        mines.Add(mineScript);
    }
}
