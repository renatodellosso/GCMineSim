using UnityEngine;

public class SimManager : MonoBehaviour
{
    private Vector3 tractorSize;
    public float safetyMargin = 4f;
    public Vector2 fieldDimensions = new(20, 100);
    public float groundScale = 5;

    public float tractorSpeed = 1f;

    public GameObject ground;
    public Camera mainCamera;

    public GameObject tractorPrefab;
    public GameObject chainLinkPrefab;
    public GameObject weightPrefab;

    public bool simulationStarted = false;
    private GameObject leftTractor, rightTractor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tractorSize = tractorPrefab.transform.localScale;

        InitSim();
        Debug.Break();

        simulationStarted = true;
    }

    void FixedUpdate()
    {
        if (!simulationStarted)
            return;

        leftTractor.transform.Translate(Time.fixedDeltaTime * tractorSpeed * Vector3.forward);
        rightTractor.transform.Translate(Time.fixedDeltaTime * tractorSpeed * Vector3.forward);
    }

    private void InitSim()
    {
        SetUpGround();
        SpawnTractors();
        PositionCamera();
    }

    private float GetTotalWidth()
    {
        return 2 * (safetyMargin + tractorSize.x / 2) + fieldDimensions.x;
    }

    private void SetUpGround()
    {
        float trueLength = fieldDimensions.y + tractorSize.z * 2;
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

        Transform currentLink = leftHitch.transform.Find("Next");
        while (currentLink.position.x > rightHitch.transform.position.x)
        {
            GameObject newLink = Instantiate(chainLinkPrefab, currentLink.position, Quaternion.LookRotation(new Vector3(0, 0, -1)));
            newLink.transform.SetParent(chain.transform);

            Transform loop = newLink.transform.Find("Loop");
            if (loop.position.x > minWeightX && loop.position.x < maxWeightX)
            {
                GameObject weight = Instantiate(weightPrefab, loop.position, Quaternion.identity);
                weight.transform.SetParent(chain.transform);
            }

            currentLink = newLink.transform.Find("Next");
        }
    }

    private void PositionCamera()
    {
        // Put camera above a corner of the field
        
        float camHeight = 15f;
        mainCamera.transform.SetPositionAndRotation(new Vector3(-GetTotalWidth() / 2, camHeight, 0), Quaternion.Euler(45, 45, 0));
    }
}
