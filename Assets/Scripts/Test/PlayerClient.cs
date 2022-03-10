#if !SERVER_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;

public partial class Player : SerializableObjectMono
{
    private bool m_AllowMoveC;
    


    override protected void Awake()
    {
        base.Awake();

    }

    private void Update()
    {

        if(Input.GetKey(KeyCode.W))
        {
            m_AllowMoveC = true;
        }
        else
        {
            m_AllowMoveC = false;
        }
        m_AllowMove = m_AllowMoveC;
    }
}
#endif
