#include <GL/glew.h> 
#include <GLFW/glfw3.h> 
#include <iostream> 

// Vertex shader 
const char* vertexShaderSource = R"( 
    #version 330 core 
    layout (location = 0) in vec2 position; 
    out vec2 TexCoord; 
    void main() 
    { 
        gl_Position = vec4(position.x, position.y, 0.0, 1.0); 
        TexCoord = position * 0.5 + 0.5; 
    } 
)"; 

// Fragment shader 
const char* fragmentShaderSource = R"( 
    #version 330 core 
    in vec2 TexCoord; 
    uniform sampler2D texture1; 
    uniform sampler2D texture2; 
    out vec4 FragColor; 
    void main() 
    { 
        vec4 color1 = texture(texture1, TexCoord); 
        vec4 color2 = texture(texture2, TexCoord); 
        FragColor = mix(color1, color2, 0.5); // Blend the two textures 
    } 
)"; 

int main() 
{ 
    // Initialize GLFW and create a window 
    if (!glfwInit()) 
    { 
        std::cerr << "Failed to initialize GLFW" << std::endl; 
        return -1; 
    } 

    // Create an OpenGL context 
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3); 
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3); 
    GLFWwindow* window = glfwCreateWindow(800, 600, "GPU Compositor", nullptr, nullptr); 
    if (!window) 
    { 
        std::cerr << "Failed to create GLFW window" << std::endl; 
        glfwTerminate(); 
        return -1; 
    } 
    glfwMakeContextCurrent(window); 

    // Initialize GLEW 
    if (glewInit() != GLEW_OK) 
    { 
        std::cerr << "Failed to initialize GLEW" << std::endl; 
        return -1; 
    } 

    // Compile shaders 
    GLuint vertexShader = glCreateShader(GL_VERTEX_SHADER); 
    glShaderSource(vertexShader, 1, &vertexShaderSource, nullptr); 
    glCompileShader(vertexShader); 

    GLuint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER); 
    glShaderSource(fragmentShader, 1, &fragmentShaderSource, nullptr); 
    glCompileShader(fragmentShader); 

    // Create shader program 
    GLuint shaderProgram = glCreateProgram(); 
    glAttachShader(shaderProgram, vertexShader); 
    glAttachShader(shaderProgram, fragmentShader); 
    glLinkProgram(shaderProgram); 

    // Load textures (texture1 and texture2) 

    // Main loop 
    while (!glfwWindowShouldClose(window)) 
    { 
        glClear(GL_COLOR_BUFFER_BIT); 

        // Use shader program 
        glUseProgram(shaderProgram); 

        // Bind textures and set uniform values 


        // Render a quad with the composited result 
 

        glfwSwapBuffers(window); 
        glfwPollEvents(); 
    } 

    // Cleanup 
    glDeleteProgram(shaderProgram); 
    glDeleteShader(vertexShader); 
    glDeleteShader(fragmentShader); 

    glfwTerminate(); 
    return 0; 
} 