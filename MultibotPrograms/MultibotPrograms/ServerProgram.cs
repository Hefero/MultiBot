using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvControllers;
using System.IO;
using System.Diagnostics;
using Enigma.D3.MemoryModel.Core;
using static Enigma.D3.MemoryModel.Core.UXHelper;

namespace MultibotPrograms
{
    public class ServerProgram
    {
        public ServerProgram()
        {
            
            IntPtr multibotProcess = RosController.GetForegroundWindow();
            string pathToFile = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents\\RoS-BoT\\Logs\\logs.txt");
            Process RosBotProcess = Win32Processes.GetProcessesLockingFile(pathToFile).FirstOrDefault();
            Console.WriteLine("Starting Server Controller");
            ServerController server = new ServerController();            
            server.pathToLogFile = pathToFile;
            server.rosbotProcess = RosBotProcess;
            server.multibotProcess = multibotProcess;
            server.GetRosRect();
            Console.WriteLine("Choose Port: "); // Prompt
            int serverportInput = Convert.ToInt32(Console.ReadLine());
            server.port = serverportInput;
            Console.WriteLine("Starting Server TCP");
            bool serverStarting = true;
            while (serverStarting) {
                try
                {
                    server.StartServerTCP();
                    try
                    {
                        serverStarting = !server.tcpServer.IsStarted;
                    }
                    catch
                    { serverStarting = true; }
                }
                catch
                {
                    Console.WriteLine("Failed to start server TCP, press key to try again");
                    Console.ReadLine();
                }
            }
            Console.WriteLine("Starting Game Modules");
            server.StartModules();            
            Console.WriteLine("All modules started: reading game states");
            Console.WriteLine("Server Ready to start, client can connect and start now");
            while (true) {
                try
                {
                    server.gameState.UpdateGameState();
                    var newLogLines = server.rosController.rosLog.NewLines;

                    if (LogFile.LookForString(newLogLines, "Vendor Loop Done") & !server.rosController.vendorLoopDone & !server.gameState.inMenu & !server.gameState.isLoading)
                    {
                        //pause after vendor loop done
                        server.rosController.vendorLoopDone = true;
                        server.rosController.enteredRift = false;
                        server.sendMessage("Server Vendor Loop Done");
                        Console.WriteLine("Vendor Loop Done Detected, server Always Pause here");
                        bool isRiftStarted = false;
                        try //check for rift started for pausing
                        {
                            UXControl riftStartedUiControl = GetControl<UXControl>("Root.NormalLayer.eventtext_bkgrnd.eventtext_region.stackpanel.rift_wrapper");
                            isRiftStarted = riftStartedUiControl.IsVisible();
                        }
                        catch { isRiftStarted = false; }
                        if (!isRiftStarted) {
                            server.rosController.Pause();
                        }
                    }

                    if (LogFile.LookForString(newLogLines, "Next rift in different") & !server.gameState.inMenu)
                    {
                        //failure detected
                        server.sendMessage("Go to menu");
                        Console.WriteLine("Next rift in different game detected: send Go to menu");
                        server.rosController.failed = true;
                    }

                    if (  LogFile.LookForString(newLogLines, "[20] Reseting timeouts") | LogFile.LookForString(newLogLines, "[21] Reseting timeouts") 
                        | LogFile.LookForString(newLogLines, "[22] Reseting timeouts") | LogFile.LookForString(newLogLines, "[23] Reseting timeouts") 
                        | LogFile.LookForString(newLogLines, "[24] Reseting timeouts") | LogFile.LookForString(newLogLines, "[25] Reseting timeouts")
                        | LogFile.LookForString(newLogLines, "[26] Reseting timeouts") | LogFile.LookForString(newLogLines, "[27] Reseting timeouts") 
                        | LogFile.LookForString(newLogLines, "[28] Reseting timeouts") | LogFile.LookForString(newLogLines, "[29] Reseting timeouts")
                        | LogFile.LookForString(newLogLines, "[30] Reseting timeouts"))
                    {
                        //paused detected
                        server.rosController.paused = true;
                    }

                    if (server.gameState.inMenu & server.rosController.failed)
                    {
                        RosController.BlockInput();
                        Console.WriteLine("Sleeping 20s");
                        Thread.Sleep(20000);
                        Console.WriteLine("Done");
                        server.rosController.InitVariables();
                        RosController.UnBlockInput();
                    }

                    if (server.gameState.acceptgrUiVisible & !server.gameState.inMenu & !server.gameState.isLoading)
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

                    if (server.gameState.cancelgriftUiVisible & !server.gameState.inMenu & !server.gameState.isLoading)
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

                    if (server.gameState.firstlevelRift & !server.rosController.enteredRift & !server.gameState.inMenu & !server.gameState.isLoading)
                    {
                        //unpause after entering rift and reinit variables
                        Thread.Sleep(1500);
                        server.rosController.enteredRift = true;
                        server.rosController.Unpause();
                        server.rosController.InitVariables();
                        Console.WriteLine("First Floor Rift Detected: Unpausing and Reiniting variables");
                    }

                    if (server.gameState.haveUrshiActor & !server.gameState.inMenu & !server.gameState.isLoading) 
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

                    if (server.gameState.lastRift.ElapsedMilliseconds > 360000 ) //Detect timeout, send F7 and restart
                    {                                                            //360 000 = 6min
                        
                        Console.WriteLine("Timeout detected");                        
                        RosController.SendF7();                        
                        Thread.Sleep(5000);
                        server.gameState.lastRift.Restart();
                        server.rosController.InitVariables();
                        server.ClickRosStart();
                        Thread.Sleep(7000);
                        server.sendMessage("Timeout");
                    }
                    if (server.gameState.aloneInGame)
                    {
                        server.rosController.Unpause();
                        break;
                    }
                }
                catch { }
            }
            Console.ReadKey();
        }

    }
}
