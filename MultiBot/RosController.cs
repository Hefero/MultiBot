using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using static EnvControllers.ServerController;
using System.Threading;

namespace EnvControllers
{
    public class RosController
    {
        public RosController(string logPath) {            
            rosLog = new LogFile(logPath);            
            enteredRift = false;
            InitVariables();
        }        
        public LogFile rosLog { get; set; }
        public bool paused { get; set; }
        public bool vendorLoopDone { get; set; }
        public bool otherVendorLoopDone { get; set; }
        public bool didUrshi { get; set; }
        public bool enteredRift { get; set; }
        public bool sentUrshi { get; set; }
        public bool failed { get; set; }

        public void InitVariables()
        {
            paused = false;
            vendorLoopDone = false;
            otherVendorLoopDone = false;
            didUrshi = false;
            sentUrshi = false;
            failed = false;
            Console.WriteLine("Restarting Variables");
        }

        public void Pause()
        {
            if (paused == false)
            {
                Thread.Sleep(100);
                String timeStamp = GetTimestamp(DateTime.Now);
                Console.WriteLine(timeStamp + " Pausing");
                SendF6();
                paused = true;
                Thread.Sleep(100);
            }
        }
        public void Unpause()
        {
            if (paused == true)
            {
                Thread.Sleep(100);
                String timeStamp = GetTimestamp(DateTime.Now);
                Console.WriteLine(timeStamp + "Unpausing");
                SendF6();
                paused = false;
                Thread.Sleep(100);
            }
        }
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);        
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        public const int SW_RESTORE = 9;
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

        }

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        public static void LeftClick()
        {
            Thread.Sleep(100);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            Thread.Sleep(100);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
            Thread.Sleep(100);
        }

        public static void RightClick()
        {
            Thread.Sleep(100);
            mouse_event((int)(MouseEventFlags.RIGHTDOWN), 0, 0, 0, 0);
            Thread.Sleep(100);
            mouse_event((int)(MouseEventFlags.RIGHTUP), 0, 0, 0, 0);
            Thread.Sleep(100);
        }

        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int uMsg, int wParam, IntPtr lParam);

        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_F6 = 0x75;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        public static void SendEscape()
        {
            IntPtr hWnd = GetForegroundWindow();
            Thread.Sleep(100);
            SendMessage(hWnd, WM_KEYDOWN, VK_ESCAPE, IntPtr.Zero);
            Thread.Sleep(100);
            SendMessage(hWnd, WM_KEYUP, VK_ESCAPE, IntPtr.Zero);
            Thread.Sleep(100);
        }

        public static void SendF6()
        {
            Thread.Sleep(100);
            keybd_event(VK_F6, 0, KEYEVENTF_EXTENDEDKEY, 0);
            Thread.Sleep(100);
            keybd_event(VK_F6, 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(100);
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy,MM/dd,HH:mm:ss.ffff");
        }
    }
}
