using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecoilMacro.Helpers
{
    public static class WinApiHelper
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray), System.Runtime.InteropServices.In] INPUT[] pInputs, int cbSize);

        public static void MoveMouse(int dx, int dy)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = 0;
            inputs[0].U.mi = new MOUSEINPUT
            {
                dx = dx,
                dy = dy,
                mouseData = 0,
                dwFlags = 0x0001,
                time = 0,
                dwExtraInfo = System.IntPtr.Zero
            };
            SendInput(1, inputs, INPUT.Size);
        }

        public struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => System.Runtime.InteropServices.Marshal.SizeOf(typeof(INPUT));
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        public struct InputUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public System.IntPtr dwExtraInfo;
        }
    }
}
