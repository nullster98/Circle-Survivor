using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float speedMul = 1f;
    public int damage = 1;
    private Vector2 dir;

    public void SetDirection(Vector2 d)
    {
        dir = d.normalized;
    }

    void Update()
    {
        // 물리 반응 없이 직접 이동 (편향 없음)
        transform.Translate(dir * speed * Time.deltaTime, Space.World);

        // 화면 밖이면 제거
        var v = Camera.main.WorldToViewportPoint(transform.position);
        if (v.x < -0.1f || v.x > 1.1f || v.y < -0.1f || v.y > 1.1f)
            Destroy(gameObject);
    }

    // Trigger 충돌: 벽/적에 맞으면 파괴
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 벽에 맞으면 파괴
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // 적이면 데미지 주고 삭제
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            dmg.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
