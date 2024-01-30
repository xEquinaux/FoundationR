﻿using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace FoundationR
{
    public enum SurfaceType
    {
        WPFImage,
        WindowHandle
    }
    public partial class Foundation : Window
    {
        public Image wpfImage;
        public Surface windowHandle;
        public void Run(SurfaceType type)
        {
            switch (type)
            {
                case SurfaceType.WPFImage:
                    Run(Dispatcher, wpfImage);
                    break;
                case SurfaceType.WindowHandle:
                    Run(Dispatcher, windowHandle);
                    break;
            }
        }
        public void Run(Image wpfImage)
        {
            Run(Dispatcher, wpfImage);
        }
        public void Run(Surface windowData)
        {
            Run(Dispatcher, windowData);
        }
        class SurfaceTypeException : Exception
        {
            public override string Message => "Surface enum not defined";
        }

        int bpp = 32;
        string path = Directory.GetCurrentDirectory();
        List<FileObj> list = new List<FileObj>();
        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            var open = new Microsoft.Win32.OpenFileDialog();
            open.Multiselect = true;
            open.InitialDirectory = System.IO.Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"));
            open.Filter = "";
            open.CheckFileExists = true;
            open.CheckPathExists = true;
            open.ShowDialog();
            foreach (string file in open.FileNames)
            {
                listbox_file.Items.Add(System.IO.Path.GetFileName(file));
                list.Add(new FileObj(file));
            }
        }

        private void button_convert_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "Select output directory";
            folder.ShowDialog();
            ImageLoader.Initialize(folder.SelectedPath);
            foreach (FileObj file in list)
            {
                ImageLoader.HandleFile(System.IO.Path.Combine(folder.SelectedPath, file.FileName + ".rew"), file.FullPath, bpp);
            }
        }

        private void listbox_file_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listbox_file.SelectedIndex != -1)
            {
                
                Bitmap preview = (Bitmap)Bitmap.FromFile(list[listbox_file.SelectedIndex].FullPath);
                int bpp = preview.PixelFormat.BytesPerPixel();
                int stride = ((preview.Width * bpp) + 3) & ~3;
                int width = preview.Width;
                int height = preview.Height;
                var data = preview.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, preview.PixelFormat);
                image.Source = BitmapSource.Create(width, height, 96f, 96f, Ext.GetFormat(bpp), null, data.Scan0, stride * height, stride);
                preview.UnlockBits(data);
            }
        }

        private void button_remove_Click(object sender, RoutedEventArgs e)
        {
            int select = listbox_file.SelectedIndex;
            if (select >= 0)
            {
                listbox_file.Items.RemoveAt(select);
                list.RemoveAt(select);
            }
        }

        private void button_clear_Click(object sender, RoutedEventArgs e)
        {
            listbox_file.Items.Clear();
            list.Clear();
        }
    }
    internal struct FileObj
    {
        public FileObj(string path)
        {
            FileName = System.IO.Path.GetFileNameWithoutExtension(path);
            FullPath = path;
            Ext = System.IO.Path.GetExtension(path);
        }
        public string FileName;
        public string FullPath;
        public string Ext;
    }
}