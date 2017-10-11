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
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Choose Port: "); // Prompt
            int serverportInput = Convert.ToInt32(Console.ReadLine());

            string pathToFile = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents\\RoS-BoT\\Logs\\logs.txt");
            Process RosBotProcess = Win32Processes.GetProcessesLockingFile(pathToFile).FirstOrDefault();
            Console.WriteLine("Starting Server Controller");
            ServerController server = new ServerController();
            server.port = serverportInput;
            server.pathToLogFile = pathToFile;
            server.rosbotProcess = RosBotProcess;
            Console.WriteLine("Starting Server TCP");
            server.Start();
            Console.WriteLine("Starting Game Modules");
            server.StartModules();
            Console.WriteLine("All modules started: reading game states");            
            while (true) {
                try
                {
                    server.gameState.UpdateGameState();
                    var newLogLines = server.rosController.rosLog.NewLines;

                    if (LogFile.LookForString(newLogLines, "Vendor Loop Done") & !server.rosController.vendorLoopDone)
                    {
                        //pause after vendor loop done
                        server.rosController.vendorLoopDone = true;
                        server.rosController.enteredRift = false;
                        server.sendMessage("Server Vendor Loop Done");
                        Console.WriteLine("Vendor Loop Done Detected, server Always Pause here");
                        server.rosController.Pause();
                    }

                    if (LogFile.LookForString(newLogLines, "Next rift in different") & !server.gameState.inMenu)
                    {
                        //failure detected
                        server.sendMessage("Go to menu");
                        Console.WriteLine("Next rift in different game detected: send Go to menu");
                        server.rosController.failed = true;
                    }

                    if (server.gameState.inMenu & server.rosController.failed)
                    {
                        ServerController.BlockInput();
                        Thread.Sleep(11000);
                        server.rosController.InitVariables();
                        ServerController.UnBlockInput();
                    }

                    if (server.gameState.acceptgrUiVisible)
                    {
                        // grift accept request: always click cancel
                        server.rosController.enteredRift = false;
                        var xCoord = server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Left +
                            (server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Width / 2);
                        var yCoord = server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Top +
                            (server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Height * 1.5);
                        RosController.SetCursorPos((int)xCoord, (int)yCoord);
                        RosController.LeftClick();
                        Console.WriteLine("Accept Rift Dialog Detected: Click Cancel");
                    }

                    if (server.gameState.cancelgriftUiVisible)
                    {
                        //click cancel ok
                        server.rosController.Pause();
                        var xCoord = server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Left +
                            (server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Width / 2);
                        var yCoord = server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Top +
                            (server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Height / 2);
                        RosController.SetCursorPos((int)xCoord, (int)yCoord);
                        RosController.LeftClick();
                        Console.WriteLine("Rift Cancelled Dialog Detected: Click Cancel");
                    }

                    if (server.gameState.firstlevelRift & !server.rosController.enteredRift)
                    {
                        //unpause after entering rift and reinit variables
                        Thread.Sleep(1500);
                        server.rosController.enteredRift = true;
                        server.rosController.Unpause();
                        server.rosController.InitVariables();
                        Console.WriteLine("First Floor Rift Detected: Unpausing and Reiniting variables");
                    }

                    if (server.gameState.haveUrshiActor)
                    {
                        //set Urshi state
                        server.rosController.didUrshi = true;
                        //send have urushi to other if didnt yet
                        if (!server.rosController.sentUrshi)
                        {
                            server.sendMessage("Teleport");
                            server.rosController.sentUrshi = true;
                            Console.WriteLine("Sent Teleport for Urshi");
                        }
                    }
                }
                catch { }
            }
        }

    }
}
