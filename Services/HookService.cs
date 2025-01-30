using Gma.System.MouseKeyHook;
using System.Windows.Forms;

namespace RecoilMacro.Services
{
    public class HookService : IDisposable
    {
        private IKeyboardMouseEvents _globalHook;

        public event Action<KeyEventArgs> KeyDown;
        public event Action<MouseEventExtArgs> MouseDownExt;
        public event Action<MouseEventExtArgs> MouseUpExt;

        public void Start()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += OnKeyDown;
            _globalHook.MouseDownExt += OnMouseDownExt;
            _globalHook.MouseUpExt += OnMouseUpExt;
        }

        public void Stop()
        {
            if (_globalHook == null) return;

            _globalHook.KeyDown -= OnKeyDown;
            _globalHook.MouseDownExt -= OnMouseDownExt;
            _globalHook.MouseUpExt -= OnMouseUpExt;
            _globalHook.Dispose();
            _globalHook = null;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown?.Invoke(e);
        }

        private void OnMouseDownExt(object sender, MouseEventExtArgs e)
        {
            MouseDownExt?.Invoke(e);
        }

        private void OnMouseUpExt(object sender, MouseEventExtArgs e)
        {
            MouseUpExt?.Invoke(e);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
