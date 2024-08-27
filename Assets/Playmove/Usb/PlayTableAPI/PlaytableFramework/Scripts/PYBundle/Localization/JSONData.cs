using UnityEngine;
using System;
using System.Collections;

namespace Playmove
{
    [Serializable]
    public class JSONData<T>
    {
        public JSONData(T obj)
        {
            Dados = obj;
        }

        public T Dados;
    }
}