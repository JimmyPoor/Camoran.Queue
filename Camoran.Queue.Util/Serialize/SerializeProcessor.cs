using Camoran.Socket.Message;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Util.Serialize
{

    public class ProtoBufSerializeProcessor : ISerializeProcessor
    {
        public T Deserialize<T>(byte[] data)
        {
            T t = default(T);
            if (data.Length <= 0) return t;
            var removeZeroData = this.TrimZeroInByteArry(data);
            using (MemoryStream ms = new MemoryStream(removeZeroData))
            {
                t = Serializer.Deserialize<T>(ms);
            }
            return t;
        }

        public byte[] Serialize<T>(T t)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, t);
                data = ms.ToArray();
            }
            return data;
        }


        private byte[] TrimZeroInByteArry(byte[] data)
        {
            int lastIndex = data.Length - 1;
            while (data[lastIndex] == 0)
            {
                --lastIndex;
            }

            byte[] temp = new byte[lastIndex+1];
            Array.Copy(data, temp, temp.Length);
            return temp;
        }
    }
}
