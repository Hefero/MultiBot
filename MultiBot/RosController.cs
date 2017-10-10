using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

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

        public void InitVariables()
        {
            paused = false;
            vendorLoopDone = false;
            otherVendorLoopDone = false;
            didUrshi = false;
        }

        public void Pause()
        {
            if (paused == false)
            {
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F6);
                paused = true;
            }
            else
            {
                throw new System.ArgumentException("Trying to pause but already paused");
            }
        }
        public void Unpause()
        {
            if (paused == true)
            {
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F6);
                paused = false;
            }
            else
            {
                throw new System.ArgumentException("Trying to unpause but already unpaused");
            }
        }
    }
}
