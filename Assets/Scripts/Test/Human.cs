using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Konline.Scripts.Serilization;

public class Human : SerializableObject
{
    [Shared] public string Name;
    [Shared] public int Age;
    
    public Human() : base()
    {

    }
    public Human(string name , int age):base()
    {
        Name = name;
        Age = age;
    }
#if !SERVER_BUILD
    public Human(int NetID):base(NetID)
    {
        
    }
#endif
}
