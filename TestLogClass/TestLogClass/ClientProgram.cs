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

namespace MultibotPrograms
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            string pathToFile = @"C:\Users\GuilhermeMarques\Documents\RoS-BoT\Logs\logs.txt";
            RosController rosCon = new RosController(pathToFile);
            ClientController client = new ClientController();
            client.serverip = "192.168.1.9";
            client.serverport = 8910;
            client.Start();
            client.StartModules();
            while (true) {
                client.gameState.UpdateGameState();
                var newLogLines = client.rosController.rosLog.NewLines;
                
                if (LogFile.LookForString(newLogLines, "Vendor Loop Done") & !client.rosController.vendorLoopDone) {
                    //pause after vendor loop done
                    client.rosController.vendorLoopDone = true;
                    client.rosController.enteredRift = false;
                    client.sendMessage("Vendor Loop Done");
                    Console.WriteLine("Vendor Loop Done Detected");
                    if (client.rosController.otherVendorLoopDone == false)
                    {
                        client.rosController.Pause();
                    }
                    Thread.Sleep(100);
                }

                if (LogFile.LookForString(newLogLines, "Next rift in different") & !client.gameState.inMenu)
                {   
                    //failure detected
                    client.sendMessage("Go to menu");
                    client.GoToMenu();
                    Console.WriteLine("Next rift in different game detected: Go to menu and send Go to menu");
                    Thread.Sleep(500);
                }

                if (client.gameState.acceptgrUiVisible & client.rosController.vendorLoopDone) {
                    // click cancel grift request
                    client.rosController.enteredRift = false;
                    var xCoord = client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Left +
                        (client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Width / 2);
                    var yCoord = client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Top +
                        (client.gameState.acceptgrUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Height * 1.5);
                    RosController.SetCursorPos((int)xCoord, (int)yCoord);
                    client.rosController.inputSimulator.Mouse.LeftButtonClick();
                    Console.WriteLine("Accept Rift Dialog Detected: Click Cancel");
                    Thread.Sleep(1500);                   
                }

                if (client.gameState.cancelgriftUiVisible)
                {
                    //click cancel ok
                    var xCoord = client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Left +
                        (client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Width / 2);
                    var yCoord = client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Top +
                        (client.gameState.confirmationUiControl.uirect.TranslateToClientRect(client.gameState.clientWidth, client.gameState.clientHeight).Height / 2);
                    RosController.SetCursorPos((int)xCoord, (int)yCoord);
                    client.rosController.inputSimulator.Mouse.LeftButtonClick();
                    Console.WriteLine("Rift Cancelled Dialog Detected: Click Cancel");
                    Thread.Sleep(1500);
                }

                if (client.gameState.firstlevelRift & !client.rosController.enteredRift)
                {
                    //unpause after entering rift and reinit variables
                    Thread.Sleep(1500);
                    client.rosController.enteredRift = true;
                    client.rosController.Unpause();
                    client.rosController.InitVariables();
                    Thread.Sleep(500);
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
        }

    }
}
