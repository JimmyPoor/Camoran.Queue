using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Message
{
    public class ErrorMessage :IMessage
    {
        public string Header { get; set; }

        public byte[] Body { get; set; }

        public DateTime CreateDate { get; private set; }

        public string Error { get; set; }

        public void SetCreateDate(DateTime createDate)
        {
            this.CreateDate = createDate;
        }

        public override string ToString()
        {
            return string.Format("({0}) Error: innerMessage is {1}", CreateDate, Encoding.UTF8.GetString(Body));
        }


        public DateTime ReceiveDate
        {
            get { throw new NotImplementedException(); }
        }
    }
}
