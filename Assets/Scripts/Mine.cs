using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Mine : MonoBehaviour
{
    public bool armed = false, hit = false;

    public float MinDetonationImpulse = 100f;
    public float explosionForce = 100f, explosionRadius = 5f;

    public Material hitMaterial;

    void OnCollisionStay(Collision collision)
    {
        if (hit || collision.gameObject.name == "Ground")
            return;

        // print($"Collision with {collision.gameObject.name} at impulse {collision.impulse.magnitude}");    

        if (collision.impulse.magnitude < MinDetonationImpulse)
            return;

        if (armed)
            Detonate();

        hit = true;
        GetComponent<MeshRenderer>().material = hitMaterial;
    }

    void Detonate()
    {
        // Get all colliders in the explosion radius
        Rigidbody[] affected = Physics.OverlapSphere(transform.position, explosionRadius)
                                        .Select(other => other.GetComponent<Rigidbody>() != null ? other.GetComponent<Rigidbody>() : other.GetComponentInParent<Rigidbody>())
                                        .Where(rb => rb != null && rb.gameObject != gameObject).ToArray();
        foreach (Rigidbody other in affected)
        {
            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                // ForceMode.Force is the default, but seems to have problems
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0, ForceMode.Impulse);
            }
        }

        print($"Mine at {transform.position} detonated with {affected.Length} affected RigidBodies ({string.Join(", ", affected.Select(rb => rb.gameObject.name))})");
    }
}
