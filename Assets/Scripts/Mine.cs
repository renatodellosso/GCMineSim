using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Mine : MonoBehaviour
{
    public bool armed = false, hit = false;

    public float minForceToDetonateLbs = 15f;
    private float MinForceToDetonateN => minForceToDetonateLbs * 4.44822f;

    public Material hitMaterial;

    void OnCollisionEnter(Collision collision)
    {
        float force = collision.impulse.magnitude / Time.fixedDeltaTime;
        print($"Collision with {collision.gameObject.name} at force {force}N");    

        if (force < MinForceToDetonateN)
            return;

        if (armed && !hit)
            Detonate();

        hit = true;
        GetComponent<MeshRenderer>().material = hitMaterial;
    }

    void Detonate()
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
    }
}
