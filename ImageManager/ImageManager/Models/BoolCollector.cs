using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models
{
    public class BoolCollector
    {
        private Dictionary<object, bool> bools = new Dictionary<object, bool>();

        public bool Value
        {
            get
            {
                List<bool> bList;
                lock (bools)
                    bList = new List<bool>(bools.Values);
                foreach (var val in bList)
                    if (!val)
                        return false;
                return true;
            }
        }

        public void ChangeBool(object key, bool value)
        {
            lock (bools)
                if (bools != null)
                    if (bools.ContainsKey(key))
                        bools[key] = value;
                    else
                        bools.Add(key, value);
        }
    }
}
