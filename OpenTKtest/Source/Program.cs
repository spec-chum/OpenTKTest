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
        int vao, vbo, ibo, program, status, angleOffset;
        float angle;
        bool wireframe = false;

        float[] cube = 
        {
            -0.5f,-0.5f,-0.5f,  // 1
            -0.5f,-0.5f, 0.5f,  // 2
            -0.5f, 0.5f, 0.5f,  // 3
            0.5f, 0.5f,-0.5f,   // 4            
            -0.5f, 0.5f,-0.5f,  // 5        
            0.5f,-0.5f, 0.5f,   // 6            
            0.5f,-0.5f,-0.5f,   // 7        
            0.5f, 0.5f, 0.5f,   // 8           
        };

        int[] index = 
        {
            1, 2, 3, 4, 1, 5, 6, 1, 7,
            4, 7, 1, 1, 3, 5, 6, 2, 1,
            2, 3, 6, 8, 7, 4, 7, 8, 6,
            8, 4, 5, 8, 5, 3, 8, 3, 6
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

            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(index.Length * sizeof(float)), index, BufferUsageHint.StaticDraw);

            // Set position data                        
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Enable the position attribute
            GL.EnableVertexAttribArray(0);

            string shaderData = File.ReadAllText("./Shaders/vertex.glsl");
            int vs = GL.CreateShader(ShaderType.VertexShader);            
            GL.ShaderSource(vs, shaderData);
            GL.CompileShader(vs);
            GL.GetShader(vs, ShaderParameter.CompileStatus, out status);
            if (status != 1)
            {
                Debug.WriteLine(GL.GetShaderInfoLog(vs));
                Exit();                
            }

            shaderData = File.ReadAllText("./Shaders/fragment.glsl");
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

            Keyboard.KeyRepeat = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);            
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.W:
                    if (wireframe == true)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        wireframe = false;
                        break;
                    }
                    else
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        wireframe = true;
                    }

                    break;
            }
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
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.DrawElements(PrimitiveType.Triangles, index.Length, DrawElementsType.UnsignedInt, 0);
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
                game.Run(60.0);
            }

            debugLog.Flush();
            debugLog.Close();
        }
    }
}