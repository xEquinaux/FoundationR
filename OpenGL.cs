//using System;
//using System.Windows.Forms;
//using SharpGL;
//using SharpGL.SceneGraph.Assets;

//namespace GPUImageCompositor
//{
//    public partial class MainForm : Form
//    {
//        private Texture texture1;
//        private Texture texture2;

//        public MainForm()
//        {
//            InitializeComponent();
//        }

//        private void openGLControl_OpenGLDraw(object sender, RenderEventArgs args)
//        {
//            OpenGL gl = openGLControl.OpenGL;

//            // Clear the color buffer 
//            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

//            // Enable 2D texturing 
//            gl.Enable(OpenGL.GL_TEXTURE_2D);

//            // Bind the textures 
//            texture1.Bind(gl);
//            texture2.Bind(gl);

//            // Set up blending 
//            gl.Enable(OpenGL.GL_BLEND);
//            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

//            // Draw a quad with the composited result 
//            gl.Begin(OpenGL.GL_QUADS);
//            gl.TexCoord(0.0f, 0.0f); gl.Vertex(-1.0f, -1.0f);
//            gl.TexCoord(1.0f, 0.0f); gl.Vertex(1.0f, -1.0f);
//            gl.TexCoord(1.0f, 1.0f); gl.Vertex(1.0f, 1.0f);
//            gl.TexCoord(0.0f, 1.0f); gl.Vertex(-1.0f, 1.0f);
//            gl.End();

//            // Disable blending and texturing 
//            gl.Disable(OpenGL.GL_BLEND);
//            gl.Disable(OpenGL.GL_TEXTURE_2D);
//        }

//        private void MainForm_Load(object sender, EventArgs e)
//        {
//            // Load your textures here (texture1 and texture2) 

//            // Initialize OpenGL settings 
//            OpenGL gl = openGLControl.OpenGL;

//            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
//        }

//        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
//        {
//            // Clean up resources (e.g., delete textures) 
//        }
//    }
//}