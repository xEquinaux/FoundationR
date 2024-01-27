using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    }
}