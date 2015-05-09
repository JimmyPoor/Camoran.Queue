using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Util.Extensions
{
    public static class TypeExtension
    {
        public static bool IsComplexType(this Type type) 
        {
            return !(type.IsPrimitive || type.Equals(typeof(string)));
        }
    }
}
