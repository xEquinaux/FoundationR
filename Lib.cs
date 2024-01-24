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

        bool flag, flag2, init;
        public static int offX, offY;
        public static Rectangle bounds;
        public static Camera? viewport;
        static BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        Form CreateForm(Surface surface)
        {
            Form form = new Form();
            form.TransparencyKey = System.Drawing.Color.CornflowerBlue;
            form.BackColor = System.Drawing.Color.CornflowerBlue;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Width = surface.Width;
            form.Height = surface.Height;
            form.Location = new Point(surface.X, surface.Y);
            form.Text = surface.Title;
            form.Name = surface.Title;
            surface.form = form;
            return form;
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
            CameraEvent += (s, e) => Camera(e.graphics, e);
        }
        internal void Run(Dispatcher dispatcher, Image surface)
        {
            this.RegisterHooks();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render, dispatcher);
            DispatcherTimer timer2 = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            timer.Tick += (s, e) => draw(ref flag, surface);
            timer2.Tick += (s, e) => update(ref flag2);
            timer.Start();
            timer2.Start();
            void draw(ref bool taskDone, Image surface)
            {
                if (!taskDone) return;
                taskDone = false;
                int width = (int)surface.Width;
                int height = (int)surface.Height;
                using (Bitmap bmp = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        using (BufferedGraphics buffered = context.Allocate(g, new Rectangle(0, 0, bounds.Width, bounds.Height)))
                        {
                            SetQuality(buffered.Graphics, new System.Drawing.Rectangle(0, 0, bounds.Width, bounds.Height));
                            g.Clear(System.Drawing.Color.CornflowerBlue);
                            ResizeEvent.Invoke(this, new EventArgs());
                            MainMenuEvent.Invoke(this, new DrawingArgs() { graphics = buffered.Graphics });
                            PreDrawEvent.Invoke(this, new PreDrawArgs() { graphics = buffered.Graphics });
                            DrawEvent.Invoke(this, new DrawingArgs() { graphics = buffered.Graphics });
                            CameraEvent.Invoke(this, new CameraArgs() { graphics = buffered.Graphics, CAMERA = viewport, offX = offX, offY = offY, screen = bounds });
                            buffered.Render();
                        }
                    }
                    int stride = width * ((PixelFormats.Bgr24.BitsPerPixel + 7) / 8);
                    var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    surface.Source = BitmapSource.Create(width, height, 96f, 96f, PixelFormats.Bgr24, null, data.Scan0, stride * height, stride);
                    bmp.UnlockBits(data);
                }
                taskDone = true;
            }
            void update(ref bool taskDone)
            {
                if (!init)
                {
                    init = true;
                    LoadResourcesEvent.Invoke(this, new EventArgs());
                    InitializeEvent.Invoke(this, new InitializeArgs());
                }
                if (!taskDone) return;
                taskDone = false;
                UpdateEvent.Invoke(this, new UpdateArgs());
                taskDone = true;
            }
        }
        internal void Run(Dispatcher dispatcher, Surface window)
        {
            this.CreateForm(window);
            this.RegisterHooks();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render, dispatcher);
            DispatcherTimer timer2 = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            timer.Tick += (s, e) => draw(ref flag, window);
            timer2.Tick += (s, e) => update(ref flag2);
            timer.Start();
            timer2.Start();
            void draw(ref bool taskDone, Surface surface)
            {
                if (!taskDone) return;
                taskDone = false;
                int width = (int)surface.Width;
                int height = (int)surface.Height;
                using (Bitmap bmp = Bitmap.FromHbitmap(FindWindow("", window.Title)))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        using (BufferedGraphics buffered = context.Allocate(g, new Rectangle(0, 0, bounds.Width, bounds.Height)))
                        {
                            SetQuality(buffered.Graphics, new System.Drawing.Rectangle(0, 0, bounds.Width, bounds.Height));
                            g.Clear(System.Drawing.Color.CornflowerBlue);
                            ResizeEvent     .Invoke(this, new EventArgs());
                            MainMenuEvent   .Invoke(this, new DrawingArgs() { graphics = buffered.Graphics });
                            PreDrawEvent    .Invoke(this, new PreDrawArgs() { graphics = buffered.Graphics });
                            DrawEvent       .Invoke(this, new DrawingArgs() { graphics = buffered.Graphics });
                            CameraEvent     .Invoke(this, new CameraArgs() { graphics = buffered.Graphics, CAMERA = viewport, offX = offX, offY = offY, screen = bounds });
                            buffered.Render();
                        }
                    }
                }
                taskDone = true;
            }
            void update(ref bool taskDone)
            {
                if (!init)
                {
                    init = true;
                    LoadResourcesEvent.Invoke(this, new EventArgs());
                    InitializeEvent.Invoke(this, new InitializeArgs());
                }
                if (!taskDone) return;
                taskDone = false;
                UpdateEvent.Invoke(this, new UpdateArgs());
                taskDone = true;
            }
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
            public Graphics graphics;
        }
        public class PreDrawArgs : EventArgs
        {
            public Graphics graphics;
        }
        public class UpdateArgs : EventArgs
        {
        }
        public class CameraArgs : EventArgs
        {
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
        public virtual void TitleScreen(Graphics graphics)
        {
        }
        public virtual void PreDraw(Graphics graphics)
        {
        }
        public virtual void Draw(Graphics graphics)
        {
        }
        public virtual void Update()
        {
        }
        public virtual void Camera(Graphics graphics, CameraArgs e)
        {
            if (e.CAMERA == null)
                return;
            if (e.CAMERA.follow && e.CAMERA.isMoving)
            {
                e.screen.X = (int)-e.CAMERA.position.X + e.screen.Width / 2 - e.offX;
                e.screen.Y = (int)-e.CAMERA.position.Y + e.screen.Height / 2 - e.offY;
            }
            graphics.RenderingOrigin = new System.Drawing.Point((int)e.CAMERA.position.X, (int)e.CAMERA.position.Y);
            graphics.TranslateTransform(
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
