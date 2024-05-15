using System;
using System.Runtime.InteropServices;

namespace RecoilMacro.Helpers
{
    // https://github.com/ddxoft/master/tree/master
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public class CDD
    {
        [DllImport("Kernel32")]
        private static extern IntPtr LoadLibrary(string dllfile);

        [DllImport("Kernel32")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        public delegate int pDD_btn(int btn);
        public delegate int pDD_whl(int whl);
        public delegate int pDD_key(int ddcode, int flag);
        public delegate int pDD_mov(int x, int y);
        public delegate int pDD_movR(int dx, int dy);
        public delegate int pDD_str(string str);
        public delegate int pDD_todc(int vkcode);

        public pDD_btn btn;
        public pDD_whl whl;
        public pDD_mov mov;
        public pDD_movR movR;
        public pDD_key key;
        public pDD_str str;
        public pDD_todc todc;

        private IntPtr m_hinst;

        ~CDD()
        {
            if (!m_hinst.Equals(IntPtr.Zero))
            {
                FreeLibrary(m_hinst);
            }
        }

        public int Load(string dllfile)
        {
            m_hinst = LoadLibrary(dllfile);
            if (m_hinst.Equals(IntPtr.Zero))
            {
                return -2;
            }
            else
            {
                return GetDDfunAddress(m_hinst);
            }
        }

        private int GetDDfunAddress(IntPtr hinst)
        {
            IntPtr ptr;

            ptr = GetProcAddress(hinst, "DD_btn");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            btn = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_btn)) as pDD_btn;

            ptr = GetProcAddress(hinst, "DD_whl");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            whl = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_whl)) as pDD_whl;

            ptr = GetProcAddress(hinst, "DD_mov");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            mov = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_mov)) as pDD_mov;

            ptr = GetProcAddress(hinst, "DD_movR");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            movR = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_movR)) as pDD_movR;

            ptr = GetProcAddress(hinst, "DD_key");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            key = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_key)) as pDD_key;

            ptr = GetProcAddress(hinst, "DD_str");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            str = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_str)) as pDD_str;

            ptr = GetProcAddress(hinst, "DD_todc");
            if (ptr.Equals(IntPtr.Zero)) return -1;
            todc = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_todc)) as pDD_todc;

            return 1;
        }
    }
}
