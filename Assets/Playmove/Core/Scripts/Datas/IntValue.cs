using UnityEngine;

namespace Playmove.Core.Datas
{
    [CreateAssetMenu(fileName = "Int Value", menuName = "Playmove/Values/Int")]
    public class IntValue : ScriptableObject
    {
        public int Value;

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
