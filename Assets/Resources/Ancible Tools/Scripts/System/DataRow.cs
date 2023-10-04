using System.Collections.Generic;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class DataRow<T>
    {
        public List<T> Items = new List<T>();

        public void Clear()
        {
            Items.Clear();
            Items = new List<T>();
        }
    }
}