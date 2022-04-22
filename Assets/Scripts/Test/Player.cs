using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;

public partial class Player : SerializableObjectMono
{
    [Shared]
    public float[] Position = new float[3];

    [Shared]
    public bool m_AllowMove = false;

    [Shared]
    public Human Human;
}
