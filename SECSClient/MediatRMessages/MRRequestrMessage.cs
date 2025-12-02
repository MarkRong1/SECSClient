using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECSClient.MediatRMessages
{
    public class MRRequestrMessage : IRequest<bool>
    {
        public string TxnName { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Type { get; set; }
        public string sType { get; set; }
        public string JsonData { get; set; }
        public string StringData { get; set; }
    }
}
