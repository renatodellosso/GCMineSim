using UnityEngine;

public class SimManager : MonoBehaviour
{
    public Vector3 tractorSize = new(4, 4, 6);
    public float safetyMargin = 4f;
    public Vector2 fieldDimensions = new(20, 100);
    public float groundScale = 5;

    public GameObject ground;
    public Camera mainCamera;

    public GameObject tractorPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitSim();
    }

    // Update is called once per frame
    void Update()
    {
        
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

        GameObject leftTractor = Instantiate(tractorPrefab, new Vector3(leftTractorX, tractorSize.y, 0), Quaternion.identity);
        GameObject rightTractor = Instantiate(tractorPrefab, new Vector3(rightTractorX, tractorSize.y, 0), Quaternion.identity);

        leftTractor.transform.localScale = tractorSize;
        rightTractor.transform.localScale = tractorSize;
    }

    private void PositionCamera()
    {
        // Put camera above a corner of the field
        
        float camHeight = 15f;
        mainCamera.transform.SetPositionAndRotation(new Vector3(-GetTotalWidth() / 2, camHeight, 0), Quaternion.Euler(45, 45, 0));
  }
}
