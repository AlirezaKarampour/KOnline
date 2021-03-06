#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;
using Konline.Scripts.UDP;

public partial class Player : SerializableObjectMono
{
    private bool m_AllowMoveC;
    private Vector3 m_Pos = new Vector3();


    override protected void Awake()
    {
        base.Awake();

    }

    private void Update()
    {
        m_Pos.x = Position[0];
        m_Pos.y = Position[1];
        m_Pos.z = Position[2];
        transform.position = m_Pos;

        if (IsOwner)
        {
            if (Input.GetKey(KeyCode.W))
            {
                m_AllowMoveC = true;
            }
            else
            {
                m_AllowMoveC = false;
            }
            m_AllowMove = m_AllowMoveC;

            byte[] data = BinarySerializer.Serialize(this);
            Packet packet = new Packet(NetworkManagerClient.SERVER_ADDR, NetworkManagerClient.SERVER_PORT, data);
            NetworkManagerClient.Instance.AddToSendQueue(packet);
        }
    }
}
#endif
