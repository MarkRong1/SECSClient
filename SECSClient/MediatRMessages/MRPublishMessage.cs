using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECSClient.MediatRMessages
{
    public class MRPublishMessage : INotification
    {
        public string Sender { get; set; }
        public string Type { get; set; }
        public string sType { get; set; }
        public string JsonData { get; set; }
        public string StringData { get; set; }
    }
}
