using System.Collections.Generic;
using UnityEngine;

public class OrbitingOrb : MonoBehaviour
{
    public int damage = 1;
    public float tickInterval = 0.2f;

    readonly Dictionary<Collider2D, float> lastHitAt = new();

    void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    void OnTriggerStay2D(Collider2D other)  { TryHit(other); }

    void TryHit(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        float t = Time.time;
        if (lastHitAt.TryGetValue(other, out float last) && (t - last) < tickInterval) return;

        var d = other.GetComponent<IDamageable>();
        if (d != null) d.TakeDamage(damage);

        lastHitAt[other] = t;
    }

    void OnDisable() { lastHitAt.Clear(); }
}