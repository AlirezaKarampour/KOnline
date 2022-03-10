#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;

public partial class Player : SerializableObjectMono
{

    override protected void Awake()
    {
        base.Awake();

    }

    private void Update()
    {
        Vector3 pos = new Vector3(m_Position[0], m_Position[1], m_Position[2]);
        transform.position = pos;

        if(Input.GetKey(KeyCode.W))
        {
            m_AllowMove = true;
        }
        else
        {
            m_AllowMove = false;
        }
        
    }
}
#endif
