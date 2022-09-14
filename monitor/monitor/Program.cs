using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace monitor
{
    class Program
    {
        public static string procname = "";
        public static int maxtime = 0;
        public static int freq = 0;

        public static void parseargs(string[] args)
        {
            procname = args[0];
            int.TryParse(args[1], out maxtime);
            int.TryParse(args[2], out freq);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // low level keyboard hook
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        // function called when key is pressed
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                // get key code
                int vkCode = Marshal.ReadInt32(lParam);
                string theKey = ((Keys)vkCode).ToString();
                if (theKey == "Q")
                {
                    Console.WriteLine("plop");
                    //release hook
                    UnhookWindowsHookEx(_hookID);
                    Environment.Exit(0);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static void WatchProcs()
        {
            while (true)
            {
                System.Threading.Thread.Sleep((int)(freq * 60 * 1000));
                Process[] prcsfound = Process.GetProcessesByName(procname);
                foreach (Process procfound in prcsfound)
                {
                    int minutes_exists = (DateTime.Now - procfound.StartTime).Minutes;
                    Console.WriteLine(minutes_exists);
                    if (minutes_exists >= maxtime)
                    {
                        procfound.Kill();
                        LogProcessKill(procfound);
                    }
                }
            }
        }

        public static void LogProcessKill(Process p)
        {
            using (StreamWriter sw = new StreamWriter("procskilled.log", true))
            {
                sw.WriteLine($"Process with ID {p.Id} was killed");
            }
        }

        public static void Main(string[] args)
        {
            parseargs(args);
            _hookID = SetHook(_proc);
            WatchProcs();
        }
    }
}