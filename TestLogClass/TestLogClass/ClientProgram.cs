using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using EnvControllers;
using SimpleTCP;
using System.IO;
using Enigma.D3.ApplicationModel;
using Enigma.D3.Assets;
using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3;
using Enigma.D3.MemoryModel.Controls;
using static Enigma.D3.MemoryModel.Core.UXHelper;
using SlimDX.DirectInput;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MultibotPrograms
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server Ip: "); // Prompt
            string serveripInput = Console.ReadLine();
            Console.WriteLine("Server Port: "); // Prompt
            int serverportInput = Convert.ToInt32(Console.ReadLine());

            string pathToFile = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents\\RoS-BoT\\Logs\\logs.txt");
            Process RosBotProcess = Win32Processes.GetProcessesLockingFile(pathToFile).FirstOrDefault();
            Console.WriteLine("Starting Client controller");
            ClientController client = new ClientController();
            client.serverip = serveripInput;
            client.serverport = serverportInput;
            client.pathToLogFile = pathToFile;
            client.rosbotProcess = RosBotProcess;
            Console.WriteLine("Connecting to Server");
            client.Start();
            Console.WriteLine("Starting Game Modules");
            client.StartModules();            
            Console.WriteLine("All modules started: reading game states");
            client.sendMessage("Client started modules");
            client.ClickRosStart();
            client.sendMessage("BeginRosBot");
            while (true) {
                try
                {
                    client.gameState.UpdateGameState();
                    while (client.gameState.isLoading)
                    { //wait for game to leave loading screen
                        client.gameState.UpdateGameState();
                    }
                    var newLogLines = client.rosController.rosLog.NewLines;

                    if (LogFile.LookForString(newLogLines, "Vendor Loop Done") & !client.rosController.vendorLoopDone)
                    {
                        //pause after vendor loop done
                        client.rosController.vendorLoopDone = true;
                        client.rosController.enteredRift = false;
                        client.sendMessage("Client Vendor Loop Done");
                        Console.WriteLine("Vendor Loop Done Detected");
                        if (!client.rosController.otherVendorLoopDone)
                        {
                            client.rosController.Pause();
                        }
                        Thread.Sleep(100);
                    }

                    if (LogFile.LookForString(newLogLines, "Next rift in different") & !client.gameState.inMenu)
                    {
                        //failure detected
                        client.sendMessage("Go to menu");
                        Console.WriteLine("Next rift in different game detected: send Go to menu");
                        client.rosController.failed = true;
                    }

                    if (client.gameState.inMenu & client.rosController.failed)
                    {
                        ServerController.BlockInput();
                        Thread.Sleep(11000);
                        client.rosController.InitVariables();
                        ServerController.UnBlockInput();
                    }

                    if (client.gameState.acceptgrUiVisible & client.rosController.vendorLoopDone)
                    {
                        // click accept grift yes
                        client.rosController.enteredRift = false;
                        var xCoord = client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Left +
                            (client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Width / 2);
                        var yCoord = client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Top +
                            (client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Height / 2);
                        RosController.SetCursorPos((int)xCoord, (int)yCoord);
                        RosController.LeftClick();
                        client.sendMessage("Pause"); //not to fail
                        Console.WriteLine("Accept Rift Dialog Detected: Click Accept and Send Pause");
                    }

                    if (client.gameState.cancelgriftUiVisible)
                    {
                        //click cancel ok
                        client.rosController.Pause();
                        var xCoord = client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Left +
                            (client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Width / 2);
                        var yCoord = client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Top +
                            (client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Height / 2);
                        RosController.SetCursorPos((int)xCoord, (int)yCoord);
                        RosController.LeftClick();
                        client.sendMessage("Start");
                        Console.WriteLine("Rift Cancelled Dialog Detected: Pause, Click Cancel, and Send Start");
                    }

                    if (client.gameState.firstlevelRift & !client.rosController.enteredRift)
                    {
                        //unpause after entering rift and reinit variables
                        Thread.Sleep(1500);
                        client.rosController.enteredRift = true;
                        client.rosController.Unpause();
                        client.rosController.InitVariables();
                        Console.WriteLine("First Floor Rift Detected: Unpausing and Reiniting variables");
                    }

                    if (client.gameState.haveUrshiActor)
                    {
                        //set Urshi state
                        client.rosController.didUrshi = true;
                        //send have urushi to other if didnt yet
                        if (!client.rosController.sentUrshi)
                        {
                            client.sendMessage("Teleport");
                            client.rosController.sentUrshi = true;
                            Console.WriteLine("Sent Teleport for Urshi");
                        }
                    }
                }
                catch { }
            }
        }

    }
}
