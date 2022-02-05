using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Konline.Scripts.Serilization;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Human h = new Human(23, 173, "alireza", true);
        Pet p = new Pet("tati", 14);
        h.pet = p;
        
        byte[] data = BinarySerializer.Serialize(h);
        Human h2 = new Human();

        BinarySerializer.Deserialize(h2, data);


        Debug.Log(BitConverter.ToString(data));



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public class Human : SerializableObject
{
    public int age;
    public int height;
    public string name;
    public bool IsMale;
    public Pet pet;

    public Human()
    {

    }

    public Human(int age , int height , string name , bool isMale) : base()
    {
        this.age = age;
        this.height = height;
        this.name = name;
        this.IsMale = isMale;
        
        
    }


}

public class Pet : SerializableObject
{
    public string Name;
    public int Age;

    public Pet(string name , int age)
    {
        this.Name = name;
        this.Age = age;
    }
}
