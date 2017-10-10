using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using EnvControllers;
using SimpleTCP;
using System.IO;

namespace EnvControllers
{
    public class ClientController : ServerController
    {
        public override void Start()
        {
            tcpClient = new SimpleTcpClient().Connect(serverip, serverport);
            tcpClient.DelimiterDataReceived += (sender, msg) => {
                ReceivedMessage(sender, msg);
            };
        }
        public SimpleTcpClient tcpClient { get; set; }
        public string serverip { get; set; }
        public int serverport { get; set; }

        public override void sendMessage(string message)
        {
            lastSendMessage = message;
            tcpClient.WriteLine(message);
        }
    }
}
