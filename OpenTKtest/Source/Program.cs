using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.IO;

namespace OpenTKTest
{
    class Game : GameWindow
    {
        int vao, vbo, program, status, angleOffset;
        float angle;

        float[] cube = 
    {
        -0.5f,-0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f, 0.5f,  1.0f, 0.0f, 0.0f,

        0.5f, 0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        
        0.5f,-0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        0.5f,-0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        
        0.5f, 0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        0.5f,-0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        
        -0.5f,-0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        
        0.5f,-0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        
        -0.5f, 0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f,-0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        0.5f,-0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        
        0.5f, 0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        0.5f,-0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        0.5f, 0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        
        0.5f,-0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        0.5f, 0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        0.5f,-0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        
        0.5f, 0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        0.5f, 0.5f,-0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        
        0.5f, 0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f,-0.5f,  1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        
        0.5f, 0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
        -0.5f, 0.5f, 0.5f,  1.0f, 0.0f, 0.0f,
        0.5f,-0.5f, 0.5f,   1.0f, 0.0f, 0.0f,
};

        public Game() : base(800, 800, GraphicsMode.Default, "OpenTK")
        {
            VSync = VSyncMode.Off;
        }        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);

            // Generate Vertex array
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // Vertices
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cube.Length * sizeof(float)), cube, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            // Set position data                        
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            // Set colour data
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            // Enable both attributes
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            string shaderData = File.ReadAllText("./Shaders/vertex.vert");
            int vs = GL.CreateShader(ShaderType.VertexShader);            
            GL.ShaderSource(vs, shaderData);
            GL.CompileShader(vs);
            GL.GetShader(vs, ShaderParameter.CompileStatus, out status);
            if (status != 1)
            {
                Debug.WriteLine(GL.GetShaderInfoLog(vs));
                Exit();                
            }

            shaderData = File.ReadAllText("./Shaders/fragment.frag");
            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, shaderData);
            GL.CompileShader(fs);
            GL.GetShader(fs, ShaderParameter.CompileStatus, out status);
            if (status != 1)
            {
                Debug.WriteLine(GL.GetShaderInfoLog(fs));
                Exit();
            }

            program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out status);
            if (status != 1)
            {
                Debug.WriteLine(GL.GetProgramInfoLog(program));
                Exit();
            }

            GL.DetachShader(program, vs);
            GL.DetachShader(program, fs);

            angleOffset = GL.GetUniformLocation(program, "angle");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);            
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();

            GL.UseProgram(program);
            GL.Uniform1(angleOffset, angle);
            angle += 1.0f * (float)e.Time;
            GL.UseProgram(0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(program);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.UseProgram(0);

            SwapBuffers();
        }

        [STAThread]
        static void Main()
        {
            TextWriterTraceListener debugLog = new TextWriterTraceListener("Debug.log");
            Debug.Listeners.Add(debugLog);

            using (Game game = new Game())
            {
                game.Run(144.0);
            }

            debugLog.Flush();
            debugLog.Close();
        }
    }
}