using System.Collections.Generic;
using UnityEngine;

namespace EnjoyJob.Battle
{
    // 亚比数据库：从 Resources/Data/pets.json 读取亚比品种配置，并按ID查询。
    public sealed class PetDatabase : MonoBehaviour
    {
        private static PetDatabase instance;

        [SerializeField] private string petTableResourcePath = "Data/pets";

        private readonly Dictionary<int, PetRecord> petsById = new Dictionary<int, PetRecord>();
        private bool loaded;

        public IReadOnlyDictionary<int, PetRecord> PetsById => petsById;

        public static PetDatabase GetOrCreate()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<PetDatabase>();
            if (instance != null)
            {
                return instance;
            }

            GameObject databaseObject = new GameObject("PetDatabase");
            instance = databaseObject.AddComponent<PetDatabase>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                return;
            }

            instance = this;
            Load();
        }

        public void Load()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            petsById.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(petTableResourcePath);
            if (jsonAsset == null)
            {
                Debug.LogError($"Pet table not found in Resources: {petTableResourcePath}");
                return;
            }

            PetTable table = JsonUtility.FromJson<PetTable>(jsonAsset.text);
            if (table == null || table.pets == null)
            {
                Debug.LogError($"Pet table is invalid: {petTableResourcePath}");
                return;
            }

            foreach (PetRecord record in table.pets)
            {
                if (record == null)
                {
                    continue;
                }

                if (petsById.ContainsKey(record.id))
                {
                    Debug.LogWarning($"Duplicate pet id skipped: {record.id}");
                    continue;
                }

                petsById.Add(record.id, record);
            }
        }

        public bool TryGetPet(int id, out PetRecord pet)
        {
            Load();
            return petsById.TryGetValue(id, out pet);
        }
    }
}
