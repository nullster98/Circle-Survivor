using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum Type { Heal1, MagnetAllXP }
    public Type type = Type.Heal1;

    [Header("Magnet Settings")]
    public float magnetSpeed = 10f; // 오브 빨려오는 속도

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<Player>();
        if (player == null) return;

        switch (type)
        {
            case Type.Heal1:
                player.Heal(1);
                break;

            case Type.MagnetAllXP:
                // 씬에 존재하는 모든 경험치 오브를 플레이어에게 끌어당김
                var orbs = FindObjectsOfType<ExperienceOrb>();
                foreach (var orb in orbs)
                {
                    orb.BeginMagnet(player.transform, magnetSpeed);
                }
                break;
        }

        Destroy(gameObject);
    }
}
