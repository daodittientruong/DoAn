using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Application.Payloads.Response
{
    public class ResponseObject<T>
    {
        public int Status {  get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public ResponseObject() { }
        public ResponseObject(int status, string messages, T? data)
        {
            Status = status;
            Message = messages;
            Data = data;
        }
    }
}
