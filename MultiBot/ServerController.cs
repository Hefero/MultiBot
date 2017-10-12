using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using static EnvControllers.RosController;

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
            FocusRosBot();
            RECT _rct = new RECT();
            GetWindowRect(rosbotProcess.MainWindowHandle, ref _rct);
            while (_rct.Left <= 0 | _rct.Right <= 0 | _rct.Bottom <= 0 | _rct.Top <= 0)
            {
                FocusRosBot();
                GetWindowRect(rosbotProcess.MainWindowHandle, ref _rct);
            }
            rosbotRect = _rct;
        }
        public RECT rosbotRect { get; set; }
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
            while (gameState.isLoading) { //wait for game to leave loading screen
                gameState.UpdateGameState();
            }
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
                            TeleportToPlayer1();
                        }
                        break;
                    }
                case "Server Vendor Loop Done":
                    {
                        Thread.Sleep(1000);
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
                        Thread.Sleep(1000);
                        this.rosController.otherVendorLoopDone = true;
                        break;
                    }
                case "BeginRosBot":
                    {
                        Thread.Sleep(1000);
                        ClickRosStart();
                        break;
                    }
                default:
                        Console.WriteLine(msg.MessageString.ToString());
                    break;
            }
            String timeStamp = RosController.GetTimestamp(DateTime.Now);
            Console.WriteLine(timeStamp + " Received: " + msg.MessageString.ToString());
        }
        public virtual void sendMessage(string message)
        {
            lastSendMessage = message;
            tcpServer.BroadcastLine(message);
            String timeStamp = RosController.GetTimestamp(DateTime.Now);
            Console.WriteLine(timeStamp + "Sending message: " + message);
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
                Thread.Sleep(13500);
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
        public void TeleportToPlayer1() {
            try
            {
                Console.WriteLine("Teleport routine started");
                rosController.Pause();
                //right click player 1
                UXControl player1FrameControl = UXHelper.GetControl<UXControl>("Root.NormalLayer.portraits.stack.party_stack.portrait_1.Frame");
                var xCoord = player1FrameControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Left +
                            (player1FrameControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Width / 2);
                var yCoord = player1FrameControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Top +
                    (player1FrameControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Height / 2);
                RosController.SetCursorPos((int)xCoord, (int)yCoord);
                RosController.RightClick();
                Thread.Sleep(100);
                //left click teleport
                UXControl teleportOptionUiControl = UXHelper.GetControl<UXControl>("Root.TopLayer.ContextMenus.PlayerContextMenu.PlayerContextMenuContent.PlayerContextMenuList.InGameTeleportToPlayer");
                xCoord = teleportOptionUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Left +
                            (teleportOptionUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Width / 2);
                yCoord = teleportOptionUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Top +
                    (teleportOptionUiControl.uirect.TranslateToClientRect(gameState.clientWidth, gameState.clientHeight).Height / 2);
                RosController.SetCursorPos((int)xCoord, (int)yCoord);
                RosController.LeftClick();
                Thread.Sleep(6000);
                rosController.Unpause();
                Console.WriteLine("Teleport routine finished");
            }
            catch {  }
        }
        public void ClickRosStart()
        {
            FocusRosBot();
            Thread.Sleep(100);
            var xCoord = rosbotRect.Right - (0.15*rosbotRect.Width);
            var yCoord = rosbotRect.Bottom - (0.07*rosbotRect.Height);
            RosController.SetCursorPos((int)xCoord, (int)yCoord);
            Thread.Sleep(100);
            RosController.LeftClick();
        }
        public void FocusRosBot()
        {
            SetForegroundWindow(rosbotProcess.MainWindowHandle);
            Thread.Sleep(100);
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