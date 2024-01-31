using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace FoundationR
{
    public partial class Foundation
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]  
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, uint flags);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);
        
        
        bool flag = true, flag2 = true, init, init2;
        public static int offX, offY;
        public static Rectangle bounds;
        public static Camera viewport;
        protected static RewBatch _rewBatch;

        internal class SurfaceForm : Form
        {
            internal SurfaceForm(Surface surface)
            {
                //form.TransparencyKey = System.Drawing.Color.CornflowerBlue;
                BackColor = System.Drawing.Color.CornflowerBlue;
                FormBorderStyle = FormBorderStyle.None;
                Width = surface.Width;
                Height = surface.Height;
                Location = new Point(surface.X, surface.Y);
                Text = surface.Title;
                Name = surface.Title;
                DoubleBuffered = true;
                UseWaitCursor = false;
                BringToFront();
            }
        }

        public virtual void RegisterHooks()
        {
        }
        internal void Run(Dispatcher dispatcher, Surface window)
        {
            this.RegisterHooks();
            window.form = new SurfaceForm(window);
            _rewBatch = new RewBatch(window.Width, window.Height, window.BitsPerPixel);
            LoadResourcesEvent  ?.Invoke();
            InitializeEvent     ?.Invoke(new InitializeArgs());
            new DispatcherTimer(TimeSpan.FromMilliseconds(60 / 1000), DispatcherPriority.Background, (s, e) => update(ref flag2), dispatcher).Start();
            IntPtr HDC = IntPtr.Zero;
            draw(ref flag, window);
            void draw(ref bool taskDone, Surface surface)
            {
                int width = (int)surface.Width;
                int height = (int)surface.Height;
                if (taskDone)
                {
                    taskDone = false;
                    {
                        InternalBegin(window);
                        if ((bool)ResizeEvent?.Invoke())
                        {
                            _rewBatch = new RewBatch(width, height, window.BitsPerPixel);
                        }
                        MainMenuEvent   ?.Invoke(new DrawingArgs() { rewBatch = _rewBatch });
                        PreDrawEvent    ?.Invoke(new PreDrawArgs() { rewBatch = _rewBatch });
                        DrawEvent       ?.Invoke(new DrawingArgs() { rewBatch = _rewBatch });
                        CameraEvent     ?.Invoke(new CameraArgs()  { rewBatch = _rewBatch, CAMERA = viewport, offX = offX, offY = offY, screen = bounds });
                        InternalEnd();
                    }
                    taskDone = true;
                }
                dispatcher.BeginInvoke(() => draw(ref flag, window), DispatcherPriority.Background, null);
            }
            void update(ref bool taskDone)
            {
                if (taskDone)
                { 
                    taskDone = false;
                    UpdateEvent?.Invoke(new UpdateArgs());
                    taskDone = true;
                }
            }
            window.form.ShowDialog();
        }
        private void InternalBegin(Surface window)
        {
            _rewBatch.Begin(GetDCEx(FindWindowByCaption(IntPtr.Zero, window.Title), IntPtr.Zero, 0x403));
        }
        private void InternalEnd()
        {
            _rewBatch.End();
        }
        #region events
        public delegate void Event<T>(T e);
        public delegate void Event();
        public delegate bool Resize();
        public static event Resize ResizeEvent;
        public static event Event<InitializeArgs> InitializeEvent;
        public static event Event LoadResourcesEvent;
        public static event Event<DrawingArgs> MainMenuEvent;
        public static event Event<PreDrawArgs> PreDrawEvent;
        public static event Event<DrawingArgs> DrawEvent;
        public static event Event<UpdateArgs> UpdateEvent;
        public static event Event<CameraArgs> CameraEvent;
        public interface IArgs
        {
        }
        public class DrawingArgs : IArgs
        {
            public RewBatch rewBatch;
        }
        public class PreDrawArgs : IArgs
        {
            public RewBatch rewBatch;
        }
        public class UpdateArgs : IArgs
        {
        }
        public class CameraArgs : IArgs
        {
            public RewBatch rewBatch;
            public Camera CAMERA;
            public Rectangle screen;
            public int offX, offY;
        }
        public class InitializeArgs : IArgs
        {
        }
        #endregion
    }
    public struct Surface
    {
        public Surface(int x, int y, int width, int height, string windowTitle, int bitsPerPixel)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Title = windowTitle;
            this.BitsPerPixel = bitsPerPixel;
            form = default;
        }
        public string? Title;
        public int Width, Height;
        public int X, Y;
        public int BitsPerPixel;
        public Form form;
    }
    public class Camera
    {
        public Vector2 oldPosition;
        public Vector2 position;
        public Vector2 velocity;
        public virtual bool isMoving => velocity != Vector2.Zero || oldPosition != position;
        public bool follow = false;
        public bool active = false;
    }
}
