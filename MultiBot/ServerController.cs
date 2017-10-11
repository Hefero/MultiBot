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
using System.Threading;
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
    public class ServerController
    {
        public virtual void Start()
        {
            tcpServer = new SimpleTcpServer().Start(port);
            tcpServer.DelimiterDataReceived += (sender, msg) => {
                ReceivedMessage(sender, msg);
            };
        }

        public SimpleTcpServer tcpServer { get; set; }
        public Message lastMessage { get; set; }
        public RosController rosController { get; set; }
        public GameState gameState { get; set; }
        public string lastSendMessage { get; set; }
        public string pathToLogFile { get; set; }
        public int port { get; set; }

        public virtual void StartModules() {            
            rosController = new RosController(pathToLogFile);
            gameState = new GameState();
        }
        public virtual void ReceivedMessage(object sender, Message msg)
        {
            Console.WriteLine("Received: ");
            lastMessage = msg;
            switch (msg.MessageString.ToString())
            {
                case "StartModules":
                    {
                        StartModules();
                        break;
                    }
                case "Unpause": case "Start":
                    {
                        rosController.Unpause();
                        break;
                    }
                case "Pause" : case "Stop" :
                    {
                        rosController.Pause();
                        break;
                    }
                case "Go to menu":
                    {                        
                        GoToMenu();
                        break;
                    }
                case "Teleport":
                    {
                        if (this.gameState.haveUrshiActor == false)
                        {
                           //teleport
                        }
                        break;
                    }
                case "Vendor Loop Done":
                    {
                        this.rosController.otherVendorLoopDone = true;
                        break;
                    }
                default:
                        Console.WriteLine(msg.MessageString.ToString());
                    break;
            }
            Console.WriteLine("\n");
        }
        public virtual void sendMessage(string message)
        {
            lastSendMessage = message;
            tcpServer.BroadcastLine(message);
        }
        public void GoToMenu() {
            rosController.Pause();
            BlockInput();
            for (int i = 0; i < 10; i++)
            {
               rosController.inputSimulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
                System.Threading.Thread.Sleep(800);
                if (gameState.leavegameUiVisible == true)
                {
                    //click it
                    System.Threading.Thread.Sleep(800);
                }
                if (gameState.inMenu == true)
                {
                    break;
                }
                Thread.Sleep(11000);
                UnBlockInput();
                rosController.Unpause();
            }
        }
        public partial class NativeMethods
        {

            /// Return Type: BOOL->int
            ///fBlockIt: BOOL->int
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "BlockInput")]
            [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool BlockInput([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fBlockIt);
        }
        public static void BlockInput()
        {
            try
            {
                NativeMethods.BlockInput(true);
            }
            catch { }
        }
        public static void UnBlockInput()
        {
            try
            {
                NativeMethods.BlockInput(false);
            }
            catch { }
        }
    }
}
