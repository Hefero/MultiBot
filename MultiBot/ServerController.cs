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
using System.Net.Sockets;
using System.Diagnostics;

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
            tcpServer.ClientConnected += (sender, msg) => {
                ClientConnected(sender, msg);
            };
        }

        public Process rosbotProcess { get; set; }
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
        public virtual void ClientConnected(object sender, TcpClient msg)
        {
            Console.WriteLine("Client Connected");
        }
        public virtual void ReceivedMessage(object sender, Message msg)
        {
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
                            Console.WriteLine("Going to Urshi");
                        }
                        break;
                    }
                case "Server Vendor Loop Done":
                    {
                        this.rosController.otherVendorLoopDone = true;
                        if (rosController.vendorLoopDone)
                        {
                            Console.WriteLine("Unpausing: Client Vendor Loop is done and received Vendor Loop done from server");
                            rosController.Unpause();                            
                        }
                        break;
                    }
                case "Client Vendor Loop Done":
                    {
                        this.rosController.otherVendorLoopDone = true;
                        break;
                    }
                default:
                        Console.WriteLine(msg.MessageString.ToString());
                    break;
            }
            Console.WriteLine("Received: " + msg.MessageString.ToString());
            Console.WriteLine("\n");
        }
        public virtual void sendMessage(string message)
        {
            lastSendMessage = message;
            tcpServer.BroadcastLine(message);
            Console.WriteLine("Sending message: " + message);
        }
        public void GoToMenu() {
            try
            {                
                rosController.Pause();                
                Console.WriteLine("Go to menu routine started");
                Console.WriteLine("First ESC and try to click");                
                RosController.SendEscape();                
                if (gameState.leavegameUiVisible == true)
                {
                    var xCoord = gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Left +
                        (gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Width / 2);
                    var yCoord = gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Top +
                        (gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Height / 2);
                    RosController.SetCursorPos((int)xCoord, (int)yCoord);
                    RosController.LeftClick();
                    Console.WriteLine("Clicked to leave");
                }
                else
                {
                    Console.WriteLine("Second ESC and try to click");
                    RosController.SendEscape();                    
                    if (gameState.leavegameUiVisible == true)
                    {
                        var xCoord = gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Left +
                            (gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Width / 2);
                        var yCoord = gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Top +
                            (gameState.leavegameUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Height / 2);
                        RosController.SetCursorPos((int)xCoord, (int)yCoord);
                        RosController.LeftClick();
                        Console.WriteLine("Clicked to leave");                        
                    }
                }
                Thread.Sleep(200);
                BlockInput();
                Console.WriteLine("Sleeping 11s");
                Thread.Sleep(12000);
                UnBlockInput();
                rosController.Unpause();
                rosController.InitVariables();
                Console.WriteLine("Go to menu routine finished");
            }
            catch
            {
                UnBlockInput();
                rosController.Unpause();
                rosController.InitVariables();
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
                Console.WriteLine("Blocking inputs");
            }
            catch { }
        }
        public static void UnBlockInput()
        {
            try
            {
                NativeMethods.BlockInput(false);
                Console.WriteLine("Unblocking inputs");
            }
            catch { }
        }
    }
}
