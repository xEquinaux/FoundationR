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
        Automatic,
        WPFImage,
        WindowHandle
    }
    public partial class Foundation : Window
    {
        SurfaceType type;
        Image? wpfImage;
        Surface? windowHandle;
        public Foundation()
        {
            type = SurfaceType.Automatic;
            InitializeComponent();
        }
        public Foundation(Image wpfImage)
        {
            this.wpfImage = wpfImage;
            type = SurfaceType.WPFImage;
        }
        public Foundation(Surface surface)
        {
            this.windowHandle = surface;
            type = SurfaceType.WindowHandle;
        }
        public void Run(SurfaceType type)
        {
            switch (type)
            {
                case SurfaceType.Automatic:
                    Run(Dispatcher, auto_surface);
                    break;
                case SurfaceType.WPFImage:
                    Run(Dispatcher, wpfImage);
                    break;
                case SurfaceType.WindowHandle:
                    Run(Dispatcher, windowHandle);
                    break;
            }
        }
    }
}