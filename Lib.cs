using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace FoundationR
{
    public partial class Foundation
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]  
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, uint flags);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);
        
        
        bool flag = true, flag2 = true, init;
        public static int offX, offY;
        public static Rectangle bounds;
        public static Camera? viewport;
        static BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        static RewBatch rewBatch;

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

        void RegisterHooks()
        {
            ResizeEvent += (s, e) => ResizeWindow();
            InitializeEvent += (s, e) => Initialize();
            LoadResourcesEvent += (s, e) => LoadResources();
            MainMenuEvent += (s, e) => TitleScreen(e.graphics);
            PreDrawEvent += (s, e) => PreDraw(e.graphics);
            DrawEvent += (s, e) => Draw(e.graphics);
            UpdateEvent += (s, e) => Update();
            CameraEvent += (s, e) => Camera(e);
        }
        internal void Run(Dispatcher dispatcher, Image surface)
        {
            this.RegisterHooks();
            rewBatch = new RewBatch((int)surface.Width, (int)surface.Height);
            new DispatcherTimer(TimeSpan.FromMilliseconds(60 / 1000), DispatcherPriority.Background, (s, e) => draw(ref flag, surface), dispatcher).Start();
            update(ref flag2);
            void draw(ref bool taskDone, Image surface)
            {
                if (taskDone)
                { 
                    taskDone = false;
                    int width = (int)surface.Width;
                    int height = (int)surface.Height;
                    using (Bitmap bmp = new Bitmap(width, height))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            using (BufferedGraphics b = context.Allocate(g, new Rectangle(0, 0, bounds.Width, bounds.Height)))
                            {
                                rewBatch.Begin();
                                SetQuality(b.Graphics, new System.Drawing.Rectangle(0, 0, width, height));
                                b.Graphics.Clear(System.Drawing.Color.CornflowerBlue);
                                ResizeEvent     .Invoke(this, new EventArgs());
                                MainMenuEvent   .Invoke(this, new DrawingArgs() { graphics = rewBatch });
                                PreDrawEvent    .Invoke(this, new PreDrawArgs() { graphics = rewBatch });
                                DrawEvent       .Invoke(this, new DrawingArgs() { graphics = rewBatch });
                                CameraEvent     .Invoke(this, new CameraArgs() { graphics = b.Graphics, CAMERA = viewport, offX = offX, offY = offY, screen = bounds });
                                rewBatch.Render(b.Graphics);
                                b.Render();
                                rewBatch.End();
                            }
                        }
                        int stride = width * ((PixelFormats.Bgr24.BitsPerPixel + 7) / 8);
                        var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        surface.Source = BitmapSource.Create(width, height, 96f, 96f, PixelFormats.Bgr24, null, data.Scan0, stride * height, stride);
                        bmp.UnlockBits(data);
                    }
                    taskDone = true;
                }
            }
            void update(ref bool taskDone)
            {
                if (!init)
                {
                    init = true;
                    LoadResourcesEvent.Invoke(this, new EventArgs());
                    InitializeEvent.Invoke(this, new InitializeArgs());
                }
                if (taskDone)
                { 
                    taskDone = false;
                    UpdateEvent.Invoke(this, new UpdateArgs());
                    taskDone = true;
                }
                dispatcher.BeginInvoke(() => update(ref flag2), DispatcherPriority.Background, null);
            }
        }
        internal void Run(Dispatcher dispatcher, Surface window)
        {
            Form form = new SurfaceForm(window);
            rewBatch = new RewBatch(window.Width, window.Height);
            //this.RegisterHooks();
            new DispatcherTimer(TimeSpan.FromMilliseconds(60 / 1000), DispatcherPriority.Background, (s, e) => draw(ref flag, window), dispatcher).Start();
            update(ref flag2);
            void draw(ref bool taskDone, Surface surface)
            {
                if (taskDone)
                {
                    taskDone = false;
                    int width = (int)surface.Width;
                    int height = (int)surface.Height;
                    IntPtr HDC = GetDCEx(FindWindowByCaption(IntPtr.Zero, window.Title), IntPtr.Zero, 0x403);
                    using (Graphics g = Graphics.FromHdc(HDC))
                    {
                        using (BufferedGraphics b = context.Allocate(g, new Rectangle(0, 0, width, height)))
                        {
                            rewBatch.Begin();
                            SetQuality(b.Graphics, new System.Drawing.Rectangle(0, 0, width, height));
                            b.Graphics.Clear(System.Drawing.Color.CornflowerBlue);
                            ResizeWindow();
                            TitleScreen(rewBatch);
                            PreDraw(rewBatch);
                            Draw(rewBatch);
                            Camera(new CameraArgs(b.Graphics, viewport, bounds, offX, offY));
                            rewBatch.Render(b.Graphics);
                            b.Render();
                            rewBatch.End();
                        }
                    }
                    DeleteObject(HDC);
                    taskDone = true;
                }
            }
            void update(ref bool taskDone)
            {
                if (!init)
                {
                    init = true;
                    LoadResources();
                    Initialize();
                }
                if (taskDone)
                { 
                    taskDone = false;
                    Update();
                    taskDone = true;
                }
                dispatcher.BeginInvoke(() => update(ref flag2), DispatcherPriority.Background, null);
            }
            form.ShowDialog();
        }
        #region events
        public static event EventHandler<EventArgs> ResizeEvent;
        public static event EventHandler<InitializeArgs> InitializeEvent;
        public static event EventHandler<EventArgs> LoadResourcesEvent;
        public static event EventHandler<DrawingArgs> MainMenuEvent;
        public static event EventHandler<PreDrawArgs> PreDrawEvent;
        public static event EventHandler<DrawingArgs> DrawEvent;
        public static event EventHandler<UpdateArgs> UpdateEvent;
        public static event EventHandler<CameraArgs> CameraEvent;
        public class DrawingArgs : EventArgs
        {
            public RewBatch graphics;
        }
        public class PreDrawArgs : EventArgs
        {
            public RewBatch graphics;
        }
        public class UpdateArgs : EventArgs
        {
        }
        public class CameraArgs : EventArgs
        {
            public CameraArgs() { }
            public CameraArgs(Graphics g, Camera a, Rectangle r, int offX, int offY)
            {
                graphics = g;
                CAMERA = a;
                screen = r;
                this.offX = offX;
                this.offY = offY;
            }
            public Graphics graphics;
            public Camera CAMERA;
            public Rectangle screen;
            public int offX, offY;
        }
        public class InitializeArgs : EventArgs
        {
        }
        #endregion
        #region methods
        public virtual void ResizeWindow()
        {
        }
        public virtual void LoadResources()
        {
        }
        public virtual void Initialize()
        {
        }
        public virtual void TitleScreen(RewBatch graphics)
        {
        }
        public virtual void PreDraw(RewBatch graphics)
        {
        }
        public virtual void Draw(RewBatch graphics)
        {
        }
        public virtual void Update()
        {
        }
        public virtual void Camera(CameraArgs e)
        {
            if (e.CAMERA == null)
                return;
            if (e.CAMERA.follow && e.CAMERA.isMoving)
            {
                e.screen.X = (int)-e.CAMERA.position.X + e.screen.Width / 2 - e.offX;
                e.screen.Y = (int)-e.CAMERA.position.Y + e.screen.Height / 2 - e.offY;
            }
            e.graphics.RenderingOrigin = new System.Drawing.Point((int)e.CAMERA.position.X, (int)e.CAMERA.position.Y);
            e.graphics.TranslateTransform(
                e.screen.X,
                e.screen.Y,
                MatrixOrder.Append);
            e.CAMERA.oldPosition = e.CAMERA.position;
        }
        #endregion
        #region quality settings
        public CompositingQuality compositingQuality = CompositingQuality.AssumeLinear;
        public CompositingMode compositingMode = CompositingMode.SourceOver;
        public InterpolationMode interpolationMode = InterpolationMode.NearestNeighbor;
        public TextRenderingHint textRenderHint = TextRenderingHint.ClearTypeGridFit;
        public GraphicsUnit graphicsUnit = GraphicsUnit.Pixel;
        public SmoothingMode smoothingMode = SmoothingMode.Default;
        private void SetQuality(Graphics graphics, System.Drawing.Rectangle bounds)
        {
            graphics.CompositingQuality = compositingQuality;
            graphics.CompositingMode = compositingMode;
            graphics.InterpolationMode = interpolationMode;
            graphics.TextRenderingHint = textRenderHint;
            //graphics.RenderingOrigin = new Point(bounds.X, bounds.Y);
            //graphics.Clip = new System.Drawing.Region(bounds);
            graphics.PageUnit = graphicsUnit;
            graphics.SmoothingMode = smoothingMode;
        }
        #endregion
    }
    public class Surface
    {
        public Surface(int x, int y, int width, int height, string windowTitle)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Title = windowTitle;
        }
        public string? Title;
        public int Width, Height;
        public int X, Y;
        internal Form? form;
    }
    public class Camera
    {
        public Vector2 oldPosition;
        public Vector2 position;
        public Vector2 velocity;
        public bool isMoving => velocity != Vector2.Zero || oldPosition != position;
        public bool follow = false;
        public bool active = false;
    }
}
