using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konline.Scripts.ObjectReplication
{
    [CreateAssetMenu(menuName = "ScriptableObject/ClassStorage")]
    public class ClassStorage : ScriptableObject
    {
        public PrefabPair[] ClassList;
        private Dictionary<string, PrefabPair> m_HashTable;


        public void Init()
        {
            m_HashTable = new Dictionary<string, PrefabPair>();
            for (int i = 0; i < ClassList.Length; i++)
            {
                m_HashTable.Add(ClassList[i].ClassName, ClassList[i]);
            }

        }

        public bool HasPrefab(string prefabName)
        {
            if (m_HashTable.ContainsKey(prefabName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public GameObject GiveServerPrefab(string PrefabName)
        {
            if (m_HashTable.ContainsKey(PrefabName))
            {
                return m_HashTable[PrefabName].ServerPrefab;
            }
            else
            {
                Debug.Log("prefab doesn't exist!");
                return null;
            }
        }

        public GameObject GiveClientPrefab(string PrefabName)
        {
            if (m_HashTable.ContainsKey(PrefabName))
            {
                return m_HashTable[PrefabName].ClientPrefab;
            }
            else
            {
                Debug.Log("prefab doesn't exist!");
                return null;
            }
        }

    }

    [System.Serializable]
    public class PrefabPair
    {
        public string ClassName;
        public GameObject ClientPrefab;
        public GameObject ServerPrefab;



        public PrefabPair()
        {

        }

    }
}
