using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace QuantifyMeHub
{
    public class QuantifyMeHub : Hub
    {
        public void Send(string name, string payload)
        {
            Clients.All.send(name, payload);
        }
    }
}