using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konline.Scripts.Serilization {
    public class NetworkManager : GenericSingleton<NetworkManager>
    {
        private int m_NextAvalibleID = 0;

        public Dictionary<int, SerializableObject> SerializableObjects; 

        public override void Awake()
        {
            base.Awake();
            
            SerializableObjects = new Dictionary<int, SerializableObject>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public int GiveNetworkID()
        {
            int netID = m_NextAvalibleID;
            m_NextAvalibleID++;
            return netID;
        }

        public void TrackNetID(SerializableObject obj)
        {
            SerializableObjects.Add(obj.NetworkID, obj);
        }
    }
}
