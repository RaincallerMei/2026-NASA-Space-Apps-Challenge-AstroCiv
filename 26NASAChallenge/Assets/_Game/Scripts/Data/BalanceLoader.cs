using Mapmakers.Sim;
using UnityEngine;

namespace Mapmakers.Data
{
    public static class BalanceLoader
    {
        public const string ResourcePath = "Balance";

        public static Balance Parse(string json) => JsonUtility.FromJson<Balance>(json);

        public static Balance Load() => Parse(Resources.Load<TextAsset>(ResourcePath).text);
    }
}
