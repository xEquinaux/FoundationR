﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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


        bool flag = true, flag2 = true, init, init2, running = true;
        public static int offX, offY;
        public static Rectangle bounds;
        internal static Camera viewport = new Camera();
        protected static RewBatch _rewBatch;
        public static IntPtr HDC, HWND, Handle;
        public Stopwatch watch = new Stopwatch();

        internal class SurfaceForm : Form
        {
            internal SurfaceForm(Surface surface)
            {
                //form.TransparencyKey = System.Drawing.Color.CornflowerBlue;
                BackColor = System.Drawing.Color.CornflowerBlue;
                FormBorderStyle = FormBorderStyle.FixedSingle;
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
        internal void Run(Surface window)
        {
            this.RegisterHooks();
            window.form = new SurfaceForm(window);
            _rewBatch = new RewBatch(window.Width, window.Height, window.BitsPerPixel);
            LoadResourcesEvent?.Invoke();
            InitializeEvent?.Invoke(new InitializeArgs() { form = window.form });
            Thread t = new Thread(() => Loop(ref running));
            Thread t2 = new Thread(() => draw(ref flag, window));
            t.SetApartmentState(ApartmentState.STA);
            t2.SetApartmentState(ApartmentState.STA);
            t.Start();
            t2.Start();

            void Loop(ref bool running)
            {
                watch.Start();
                double deltaTime = 0;
                double accumulator = 0;
                double targetFrameTime = 1.0 / 120.0;
                double oldTime = 0;

                while (running)
                {
                    double currentTime = watch.Elapsed.TotalSeconds;
                    watch.Restart();
                    deltaTime = currentTime - oldTime;
                    oldTime = currentTime;

                    accumulator += deltaTime;

                    if (accumulator < 0)
                    {
                        accumulator = 0d;
                    }
                    while (accumulator >= targetFrameTime)
                    {
                        update(ref flag2);
                        accumulator -= targetFrameTime;
                    }

                    //draw(ref flag, window);
                }
            }

            void draw(ref bool taskDone, Surface surface)
            {
                int width = (int)surface.Width;
                int height = (int)surface.Height;
                while (running)
                { 
                    if (taskDone)
                    {
                        taskDone = false;
                        try
                        {
                            window.form?.Invoke(() =>
                            {
                                InputEvent?.Invoke(new InputArgs() { mouse = window.form.PointToClient(System.Windows.Forms.Cursor.Position) });
                            });
                        }
                        catch
                        { }
                        finally
                        {
                            InternalBegin(window);
                            if ((bool)ResizeEvent?.Invoke(new ResizeArgs()))
                            {
                                _rewBatch = new RewBatch(width, height, window.BitsPerPixel);
                            }
                            MainMenuEvent?.Invoke(new DrawingArgs() { rewBatch = _rewBatch });
                            PreDrawEvent?.Invoke(new PreDrawArgs() { rewBatch = _rewBatch });
                            DrawEvent?.Invoke(new DrawingArgs() { rewBatch = _rewBatch });
                            CameraEvent?.Invoke(new CameraArgs() { rewBatch = _rewBatch, CAMERA = viewport, offX = offX, offY = offY, screen = bounds });
                            InternalEnd(GetDCEx(FindWindowByCaption(IntPtr.Zero, window.Title), IntPtr.Zero, 0x403));
                        }
                        taskDone = true;
                    }
                }
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
            HDC = GetDCEx(FindWindowByCaption(IntPtr.Zero, window.Title), IntPtr.Zero, 0x403);
            HWND = FindWindowByCaption(IntPtr.Zero, window.Title);
            Handle = window.form.Handle;
            window.form.ShowDialog();
        }
        bool UpdateLimiter(Stopwatch watch1)
        {
            double deltaTime = 0; // Initialize the time accumulator
            double accumulator = 0; // Accumulated time
            double targetFrameTime = 1.0 / 60.0; // Target frame time (1/60 seconds)
            double oldTime = 0;

            double currentTime = watch1.Elapsed.Milliseconds; // Get current time
            deltaTime = currentTime - oldTime; // Calculate time since last frame
            oldTime = currentTime; // Update old time

            accumulator += deltaTime; // Accumulate time

            // Update when the accumulated time exceeds the target frame time
            while (accumulator >= targetFrameTime)
            {
                watch1.Restart();
                accumulator -= targetFrameTime; // Subtract the frame time
                return true;
            }
            return false;
        }

        public static class WindowUtils
        {
            [DllImport("dwmapi.dll")]
            static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            public static RECT GetWindowRectangleWithoutShadows(IntPtr handle)
            {
                RECT rect;
                DwmGetWindowAttribute(handle, 9 /* DWMWA_EXTENDED_FRAME_BOUNDS */, out rect, Marshal.SizeOf(typeof(RECT)));
                return rect;
            }
        }

        private void InternalBegin(Surface window)
        {
            _rewBatch.Begin(IntPtr.Zero);//GetDCEx(FindWindowByCaption(IntPtr.Zero, window.Title), IntPtr.Zero, 0x403));
        }
        private void InternalBegin(IntPtr hdc)
        {
            _rewBatch.Begin(hdc);
        }
        private void InternalEnd(IntPtr handle)
        {
            _rewBatch.End(RewBatch.renderOption, handle);
        }
        #region events
        public delegate void Event<T>(T e);
        public delegate void Event();
        public delegate bool Resize<T>(T e);
        public static event Resize<ResizeArgs> ResizeEvent;
        public static event Event<InitializeArgs> InitializeEvent;
        public static event Event<InputArgs> InputEvent;
        public static event Event LoadResourcesEvent;
        public static event Event<DrawingArgs> MainMenuEvent;
        public static event Event<PreDrawArgs> PreDrawEvent;
        public static event Event<DrawingArgs> DrawEvent;
        public static event Event<UpdateArgs> UpdateEvent;
        public static event Event<CameraArgs> CameraEvent;
        public interface IArgs
        {
        }
        public class ResizeArgs : IArgs
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
            public byte[] backBuffer;
        }
        public class InitializeArgs : IArgs
        {
            public Form form;
        }
        public class InputArgs : IArgs
        {
            public Point mouse;
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
        public Rectangle bounds;
        public int Width => bounds.Width;
        public int Height => bounds.Height;
        public int X => (int)position.X;
        public int Y => (int)position.Y;
        public virtual bool isMoving => velocity != Vector2.Zero || oldPosition != position;
        public bool follow = false;
        public bool active = false;
    }
}
