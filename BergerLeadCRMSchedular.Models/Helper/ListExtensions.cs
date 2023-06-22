using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BergerLeadCRMSchedular.Models.Helper
{
    public static class ListExtensions
    {
        public static List<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }
    }
}
