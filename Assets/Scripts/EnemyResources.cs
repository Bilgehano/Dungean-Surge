using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyResources : MonoBehaviour
{
    [Serializable]
    public class DropEntry
    {
        public GameObject prefab;
        [Min(0)] public int minCount = 1;
        [Min(0)] public int maxCount = 1;
        [Range(0f, 1f)] public float dropChance = 1f;
    }

    [Header("Drops")]
    [SerializeField] private List<DropEntry> drops = new List<DropEntry>();
    [SerializeField] private float dropScatterRadius = 0.25f;
    [SerializeField] private bool logDrops = true;

    private bool hasDropped;

    private void OnEnable()
    {
        hasDropped = false;
    }

    public void DropAll()
    {
        if (hasDropped)
        {
            return;
        }

        Vector3 origin = transform.position;
        int totalSpawned = 0;

        if (drops == null || drops.Count == 0)
        {
            if (logDrops)
            {
                Debug.LogWarning("EnemyResources: No drop entries configured.", this);
            }

            hasDropped = true;
            return;
        }

        for (int i = 0; i < drops.Count; i++)
        {
            DropEntry entry = drops[i];
            if (entry == null || entry.prefab == null)
            {
                if (logDrops)
                {
                    Debug.LogWarning("EnemyResources: Drop entry is missing prefab at index " + i + ".", this);
                }
                continue;
            }

            float roll = UnityEngine.Random.value;
            if (roll > entry.dropChance)
            {
                if (logDrops)
                {
                    Debug.Log("EnemyResources: Skipped drop " + entry.prefab.name + " due to chance. Roll=" + roll + " Chance=" + entry.dropChance, this);
                }
                continue;
            }

            int min = Mathf.Min(entry.minCount, entry.maxCount);
            int max = Mathf.Max(entry.minCount, entry.maxCount);
            int spawnCount = UnityEngine.Random.Range(min, max + 1);

            if (logDrops)
            {
                Debug.Log("EnemyResources: Spawning " + spawnCount + " x " + entry.prefab.name + ".", this);
            }

            for (int j = 0; j < spawnCount; j++)
            {
                Vector2 offset2D = UnityEngine.Random.insideUnitCircle * dropScatterRadius;
                Vector3 spawnPos = origin + new Vector3(offset2D.x, offset2D.y, 0f);
                GameObject spawned = Instantiate(entry.prefab, spawnPos, Quaternion.identity);
                if (spawned != null && !spawned.activeSelf)
                {
                    spawned.SetActive(true);
                }

                totalSpawned++;
            }
        }

        if (logDrops)
        {
            Debug.Log("EnemyResources: DropAll finished. Total spawned=" + totalSpawned + " at " + origin + ".", this);
        }

        hasDropped = true;
    }
}
