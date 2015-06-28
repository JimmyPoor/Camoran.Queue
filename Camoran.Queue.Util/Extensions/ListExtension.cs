using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Util.Extensions
{
    public static class ListExtension
    {
        public static int GetFirstIndex<T, V>(this List<T> list, Func<T, V, bool> condition, V para)
        {

            if (condition == null) throw new ArgumentNullException("condition can't be null");
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (condition.Invoke(list[i], para))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public static int GetIndexInList<T, V>(this IList<T> ls, Func<T, V, bool> condiftion, V para)
        {
            return ls.ToList().GetFirstIndex(condiftion, para);
        }
    }
}
