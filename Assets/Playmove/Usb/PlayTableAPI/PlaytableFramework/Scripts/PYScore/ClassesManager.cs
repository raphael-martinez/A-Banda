using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ClassesManager
{
    [Serializable]
    public struct Class
    {
        public int Id;
        public string Name;
        public bool Hidden;
    }

    private List<Class> _studentClasses;
    public List<Class> StudentClasses
    {
        get { return _studentClasses; }
        set { _studentClasses = value; }
    }

    public ClassesManager()
    {
        Load();
    }

    public List<Class> Load()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        string path = dir.Parent.FullName;
        using (Stream stream = File.Open(string.Format("{0}/Classes.bin", path), FileMode.Create))
        {
            BinaryFormatter bin = new BinaryFormatter();
            bin.Serialize(stream, StudentClasses);
        }
        return StudentClasses;
    }
}
