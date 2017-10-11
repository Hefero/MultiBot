using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EnvControllers;
using SimpleTCP;
using System.IO;
using Enigma.D3.ApplicationModel;
using Enigma.D3.Assets;
using Enigma.D3.AttributeModel;
using Enigma.D3.DataTypes;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3;
using Enigma.D3.MemoryModel.Controls;
using static Enigma.D3.MemoryModel.Core.UXHelper;

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
            String timeStamp = RosController.GetTimestamp(DateTime.Now);
            Console.WriteLine(timeStamp + "Sending message: " + message);
        }
    }
}
