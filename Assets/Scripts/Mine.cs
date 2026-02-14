using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Mine : MonoBehaviour
{
    public bool armed = false, hit = false;

    public float MinDetonationImpulse = 100f;

    public Material hitMaterial;

    void OnCollisionStay(Collision collision)
    {
        if (hit || collision.gameObject.name == "Ground")
            return;

        print($"Collision with {collision.gameObject.name} at impulse {collision.impulse.magnitude}");    

        if (collision.impulse.magnitude < MinDetonationImpulse)
            return;

        if (armed)
            Detonate();

        hit = true;
        GetComponent<MeshRenderer>().material = hitMaterial;
    }

    void Detonate()
    {
        GetComponent<Collider>().enabled = false;
    }
}
