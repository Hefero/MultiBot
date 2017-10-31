using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvControllers;
using SimpleTCP;
using System.IO;
using System.Diagnostics;
using static Enigma.D3.MemoryModel.Core.UXHelper;

namespace MultibotPrograms
{
    public class ClientProgram
    {
        public ClientProgram()
        {
            
            IntPtr multibotProcess = RosController.GetForegroundWindow();
            string pathToFile = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents\\RoS-BoT\\Logs\\logs.txt");
            Process RosBotProcess = Win32Processes.GetProcessesLockingFile(pathToFile).FirstOrDefault();
            Console.WriteLine("Starting Client controller: Make sure Server is Ready");
            ClientController client = new ClientController();
            client.pathToLogFile = pathToFile;
            client.rosbotProcess = RosBotProcess;
            client.multibotProcess = multibotProcess;
            client.GetRosRect();
            Console.WriteLine("Server Ip: "); // Prompt
            string serveripInput = Console.ReadLine();
            Console.WriteLine("Server Port: "); // Prompt
            int serverportInput = Convert.ToInt32(Console.ReadLine());
            client.serverip = serveripInput;
            client.serverport = serverportInput;
            Console.WriteLine("Connecting to Server");
            bool clientStarting = true;
            while (clientStarting)
            {
                try
                {
                    client.Connect();
                    try
                    {
                        clientStarting = !client.tcpClient.TcpClient.Connected;
                    }
                    catch { clientStarting = true; }
                }
                catch
                {
                    Console.WriteLine("Failed to connect to server, press key to try again");
                    Console.ReadLine();
                    Console.WriteLine("Server Ip: "); // Prompt
                    serveripInput = Console.ReadLine();
                    Console.WriteLine("Server Port: "); // Prompt
                    serverportInput = Convert.ToInt32(Console.ReadLine());
                }
            }
            Console.WriteLine("Starting Game Modules");
            client.StartModules();            
            Console.WriteLine("All modules started: reading game states");
            client.sendMessage("Client started modules");
            client.ClickRosStart();
            client.sendMessage("BeginRosBot");

            Thread gameStateDecider = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        client.gameState.UpdateGameState();
                        if (client.gameState.inMenu & client.rosController.failed)
                        {
                            //sleeping after going to menu
                            RosController.BlockInput();
                            Console.WriteLine("Sleeping 20s");
                            Thread.Sleep(20000);
                            Console.WriteLine("Done");
                            client.rosController.InitVariables();
                            RosController.UnBlockInput();
                        }

                        if (client.gameState.acceptgrUiVisible & client.rosController.vendorLoopDone)
                        {
                            // click accept grift yes
                            client.rosController.Pause();
                            client.rosController.enteredRift = false;
                            var xCoord = client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Left +
                                (client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Width / 2);
                            var yCoord = client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Top +
                                (client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Height / 2);
                            RosController.SetCursorPos((int)xCoord, (int)yCoord);
                            RosController.LeftClick();
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

                        if (!client.rosController.enteredRift & client.gameState.firstlevelRift & !client.gameState.inMenu & !client.gameState.isLoading)
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
            });

            Thread logFileDecider = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var newLogLines = client.rosController.rosLog.NewLines;

                        if (LogFile.LookForString(newLogLines, "Vendor Loop Done"))
                        {
                            //pause after vendor loop done
                            client.rosController.vendorLoopDone = true;
                            client.rosController.enteredRift = false;
                            client.sendMessage("Client Vendor Loop Done");
                            Console.WriteLine("Vendor Loop Done Detected");
                            if (!client.rosController.otherVendorLoopDone & !client.gameState.aloneInGame)
                            {
                                bool isRiftStarted = false;
                                try //check for rift started for pausing
                                {
                                    UXControl riftStartedUiControl = GetControl<UXControl>("Root.NormalLayer.eventtext_bkgrnd.eventtext_region.stackpanel.rift_wrapper");
                                    isRiftStarted = riftStartedUiControl.IsVisible();
                                }
                                catch { isRiftStarted = false; }
                                if (!isRiftStarted)
                                {
                                    client.rosController.Pause();
                                }
                            }
                            Thread.Sleep(100);
                        }

                        if (LogFile.LookForString(newLogLines, "Next rift in different") & !client.gameState.aloneInGame)
                        {
                            //failure detected
                            client.sendMessage("Go to menu");
                            Console.WriteLine("Next rift in different game detected: send Go to menu");
                            client.rosController.failed = true;
                        }

                        if (LogFile.LookForString(newLogLines, "[20] Reseting timeouts") | LogFile.LookForString(newLogLines, "[21] Reseting timeouts")
                            | LogFile.LookForString(newLogLines, "[22] Reseting timeouts") | LogFile.LookForString(newLogLines, "[23] Reseting timeouts")
                            | LogFile.LookForString(newLogLines, "[24] Reseting timeouts") | LogFile.LookForString(newLogLines, "[25] Reseting timeouts")
                            | LogFile.LookForString(newLogLines, "[26] Reseting timeouts") | LogFile.LookForString(newLogLines, "[27] Reseting timeouts")
                            | LogFile.LookForString(newLogLines, "[28] Reseting timeouts") | LogFile.LookForString(newLogLines, "[29] Reseting timeouts")
                            | LogFile.LookForString(newLogLines, "[30] Reseting timeouts"))
                        {
                            //paused detected
                            client.rosController.paused = true;
                            client.sendMessage("Status Check Code 20");
                        }
                    }
                    catch { }
                }
            });
            Console.WriteLine("Starting Threads");
            gameStateDecider.Start();
            logFileDecider.Start();
            Console.ReadKey();
        }

    }
}
