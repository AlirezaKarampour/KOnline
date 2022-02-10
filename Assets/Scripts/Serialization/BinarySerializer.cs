using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;
using UnityEngine;
using System.Text;

namespace Konline.Scripts.Serilization
{
    public static class BinarySerializer
    {
        public static byte[] Serialize(object targetObject)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                FieldInfo[] fieldInfos = targetObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                List<FieldInfo> fieldInfoslist = new List<FieldInfo>(fieldInfos);

                for (int i = 0; i < fieldInfoslist.Count; i++)
                {
                    Shared att = fieldInfoslist[i].GetCustomAttribute<Shared>();
                    if(att == null)
                    {
                        fieldInfoslist.RemoveAt(i);
                    }

                }



                for (int i = 0; i < fieldInfoslist.Count; i++)
                {
                    if (fieldInfoslist[i].Name == "ClassID")
                    {
                        byte[] data = SerializeField(fieldInfoslist[i], targetObject);
                        ms.Write(data, 0, data.Length);
                        fieldInfoslist.RemoveAt(i);
                        break;
                    }
                }

                for (int i = 0; i < fieldInfoslist.Count; i++)
                {
                    if (fieldInfoslist[i].Name == "NetworkID")
                    {
                        byte[] data = SerializeField(fieldInfoslist[i], targetObject);
                        ms.Write(data, 0, data.Length);
                        fieldInfoslist.RemoveAt(i);
                        break;
                    }
                }



                foreach (FieldInfo f in fieldInfoslist)
                {


                    byte[] data = SerializeField(f, targetObject);

                    //Debug.Log(f.Name);

                    if (data != null)
                    {
                        ms.Write(data, 0, data.Length);
                    }
                }

                ms.Position = 0;
                byte[] data2 = new byte[ms.Length];
                ms.Read(data2, 0, data2.Length);
                return data2;
            }
        }

        private static byte[] SerializeField(FieldInfo field, object targetObject)
        {

            Type fieldType = field.FieldType;
            object fieldValue = field.GetValue(targetObject);


            if (fieldType.IsClass)
            {
                if (fieldValue is SerializableObject)
                {
                    SerializableObject so = (SerializableObject)fieldValue;
                    byte[] data = BitConverter.GetBytes(so.NetworkID);
                    return data;
                }
            }





            if (fieldType.IsGenericType)
            {
                fieldType = fieldType.GetGenericTypeDefinition();
                Debug.Log("not implemented yet !");
                return null;
            }


            if (fieldType.IsArray)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms,Encoding.UTF8))
                    {
                        fieldType = typeof(Array);
                        Array arr = (Array)fieldValue;
                        Type elementType = arr.GetType().GetElementType();
                        if (arr.Rank == 1)
                        {
                            int len = arr.GetLength(0);
                            bw.Write(len);
                            bw.Write(elementType.Name);
                            if (elementType.IsPrimitive || elementType == typeof(string))
                            {
                                if (elementType == typeof(int) || elementType.IsEnum)
                                {
                                    for (int i = 0; i < len; i++)
                                    {
                                        bw.Write((int)arr.GetValue(i));
                                    }

                                    byte[] array = new byte[ms.Length];
                                    ms.Position = 0;
                                    ms.Read(array, 0, array.Length);
                                    return array;

                                }
                                else if (elementType == typeof(float))
                                {
                                    for (int i = 0; i < len; i++)
                                    {
                                        bw.Write((float)arr.GetValue(i));
                                    }

                                    byte[] array = new byte[ms.Length];
                                    ms.Position = 0;
                                    ms.Read(array, 0, array.Length);
                                    return array;
                                }
                                else if (elementType == typeof(bool))
                                {
                                    for (int i = 0; i < len; i++)
                                    {
                                        bw.Write((bool)arr.GetValue(i));
                                    }

                                    byte[] array = new byte[ms.Length];
                                    ms.Position = 0;
                                    ms.Read(array, 0, array.Length);
                                    return array;
                                }
                                else if (elementType == typeof(string))
                                {
                                    for (int i = 0; i < len; i++)
                                    {
                                        bw.Write((string)arr.GetValue(i));
                                    }

                                    byte[] array = new byte[ms.Length];
                                    ms.Position = 0;
                                    ms.Read(array, 0, array.Length);
                                    return array;
                                }


                            }
                            else if (elementType.IsClass)
                            {
                                for (int i = 0; i < len; i++)
                                {
                                    object element = arr.GetValue(i);

                                    if(element is SerializableObject)
                                    {
                                        SerializableObject serObj = (SerializableObject)element;
                                        bw.Write(serObj.NetworkID);                                        
                                    }
                                }

                                byte[] array = new byte[ms.Length];
                                ms.Position = 0;
                                ms.Read(array, 0, array.Length);
                                return array;

                            }


                        }

                    }


                }

            }


            if (fieldType == typeof(int) || fieldType.IsEnum)
            {
                return BitConverter.GetBytes((int)fieldValue);
            }
            else if (fieldType == typeof(float))
            {
                return BitConverter.GetBytes((float)fieldValue);
            }
            else if (fieldType == typeof(bool))
            {
                return BitConverter.GetBytes((bool)fieldValue);
            }
            else if (fieldType == typeof(string))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms,Encoding.UTF8))
                    {
                        using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8))
                        {
                            bw.Write((string)fieldValue);
                            ms.Position = 0;
                            byte[] String = br.ReadBytes((int)ms.Length);
                            return String;
                        }
                        
                    }
                }
            }

            return null;
        }


        public static void Deserialize(SerializableObject obj , byte[] data)
        {
            Type type = obj.GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            List<FieldInfo> fieldInfoList = new List<FieldInfo>(fieldInfos);


            for (int i = 0; i < fieldInfoList.Count; i++)
            {
                Shared att = fieldInfoList[i].GetCustomAttribute<Shared>();
                if (att == null)
                {
                    fieldInfoList.RemoveAt(i);
                }

            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryReader br = new BinaryReader(ms , Encoding.UTF8))
                {
                    ms.Write(data, 0, data.Length);
                    ms.Position = 0;

                    for (int i = 0; i < fieldInfoList.Count; i++)
                    {
                        if (fieldInfoList[i].Name == "ClassID")
                        {
                            obj.ClassID = br.ReadString();
                            fieldInfoList.RemoveAt(i);

                            break;
                        }
                    }

                    for (int i = 0; i < fieldInfoList.Count; i++)
                    {
                        if (fieldInfoList[i].Name == "NetworkID")
                        {
                            obj.NetworkID = br.ReadInt32();
                            fieldInfoList.RemoveAt(i);

                            break;
                        }
                    }



                    foreach (FieldInfo f in fieldInfoList)
                    {
                        DeserializeField(f, obj, br);
                    }
                }
            }
        }   
        
        private static void DeserializeField(FieldInfo fieldInfo , SerializableObject obj , BinaryReader br)
        {
            Type fieldType = fieldInfo.FieldType;

            if (fieldType.IsClass && fieldType != typeof(string) && fieldType.IsArray == false)
            {
                if (fieldType.IsSubclassOf(typeof(SerializableObject)))
                {
#if !SERVER_BUILD
                    int key = br.ReadInt32();
                    SerializableObject refObj = NetworkManagerClient.Instance.SerializableObjects[key];
                    fieldInfo.SetValue(obj, refObj);
#endif

#if SERVER_BUILD




#endif
                }
            }
            else if(fieldType.IsPrimitive || fieldType == typeof(string))
            {
                if(fieldType == typeof(int) || fieldType.IsEnum)
                {
                    fieldInfo.SetValue(obj, br.ReadInt32());
                }
                else if(fieldType == typeof(float))
                {
                    fieldInfo.SetValue(obj, br.ReadSingle());
                }
                else if(fieldType == typeof(bool))
                {                    
                    fieldInfo.SetValue(obj, br.ReadBoolean());
                }
                else if(fieldType == typeof(string))
                {                    
                    fieldInfo.SetValue(obj, br.ReadString());
                }

            }
            else if (fieldType.IsArray)
            {              
                int len = br.ReadInt32();
                string elementTypeName = br.ReadString();
                
                
                if(elementTypeName == typeof(SerializableObject).Name)
                {
                    int[] networkIDs = new int[len];
                    SerializableObject[] serObjs = new SerializableObject[len];

                    for(int i = 0; i < len; i++)
                    {
                        networkIDs[i] = br.ReadInt32();
                    }

                    for(int i = 0; i < len; i++)
                    {
#if !SERVER_BUILD
                        serObjs[i] = NetworkManagerClient.Instance.SerializableObjects[networkIDs[i]];
#endif
#if SERVER_BUILD

                        


#endif

                    }

                    fieldInfo.SetValue(obj, serObjs);

                }



                
                if(elementTypeName == "Int32")
                {
                    int[] ints = new int[len];
                    for(int i = 0; i < len; i++)
                    {
                        ints[i] = br.ReadInt32();
                    }

                    fieldInfo.SetValue(obj, ints);
                }
                else if (elementTypeName == "Single")
                {
                    float[] floats = new float[len];
                    for (int i = 0; i < len; i++)
                    {
                        floats[i] = br.ReadSingle();
                    }

                    fieldInfo.SetValue(obj, floats);
                }
                else if (elementTypeName == "Boolean")
                {
                    bool[] bools = new bool[len];
                    for (int i = 0; i < len; i++)
                    {
                        bools[i] = br.ReadBoolean();
                    }

                    fieldInfo.SetValue(obj, bools);
                }
                else if (elementTypeName == "String")
                {
                    string[] strings = new string[len];
                    for (int i = 0; i < len; i++)
                    {
                        strings[i] = br.ReadString();
                    }

                    fieldInfo.SetValue(obj, strings);
                }


            }



        }

    }
}
