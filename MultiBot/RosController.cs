using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
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
using System.Runtime.InteropServices;

namespace EnvControllers
{
    public class RosController
    {
        public RosController(string logPath) {            
            rosLog = new LogFile(logPath);
            inputSimulator = new InputSimulator();
            enteredRift = false;
            InitVariables();
        }
        public LogFile rosLog { get; set; }
        public InputSimulator inputSimulator { get; set; }
        public bool paused { get; set; }
        public bool vendorLoopDone { get; set; }
        public bool otherVendorLoopDone { get; set; }
        public bool didUrshi { get; set; }
        public bool enteredRift { get; set; }
        public bool sentUrshi { get; set; }

        public void InitVariables()
        {
            paused = false;
            vendorLoopDone = false;
            otherVendorLoopDone = false;
            didUrshi = false;
            sentUrshi = false;
        }

        public void Pause()
        {
            if (paused == false)
            {
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F6);
                paused = true;
                Console.WriteLine("Pausing");
            }
            else
            {
                //throw new System.ArgumentException("Trying to pause but already paused");
            }
        }
        public void Unpause()
        {
            if (paused == true)
            {
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F6);
                paused = false;
                Console.WriteLine("Unpausing");
            }
            else
            {
                //throw new System.ArgumentException("Trying to unpause but already unpaused");
            }
        }
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
    }
}
