using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Util.Serialize
{
    public interface ISerializeProcessor
    {
        T Deserialize<T>(byte[] data);
        byte[] Serialize<T>(T t);
    }
}
