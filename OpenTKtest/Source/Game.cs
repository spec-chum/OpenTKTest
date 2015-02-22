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
        Matrix4 projectionMatrix4, modelMatrix4;
        int vao, vbo, normals, index, program, status;
        int projectionMatrixLocation, modelMatrixLocation, angleLocation;
        float angle;
        bool wireframe = false;

        float[] cube = 
        {
            -1.0f,  1.0f,  1.0f,  // 0 - LUF
             1.0f,  1.0f,  1.0f,  // 1 - RUF
            -1.0f, -1.0f,  1.0f,  // 2 - LDF
             1.0f, -1.0f,  1.0f,  // 3 - RDF
             
            -1.0f,  1.0f, -1.0f,  // 4 - LUB
             1.0f,  1.0f, -1.0f,  // 5 - RUB
            -1.0f, -1.0f, -1.0f,  // 6 - LDB
             1.0f, -1.0f, -1.0f,  // 7 - RDB
            
        };

        int[] indices = 
        {
            0, 1, 2,    2, 1, 3,    // Front
            5, 4, 7,    6, 7, 4,    // Back
            4, 0, 6,    6, 0, 2,    // Left
            1, 5, 7,    7, 3, 1,    // Right
            0, 4, 5,    5, 1, 0,    // Top
            2, 7, 6,    2, 3, 7     // Bottom
        };

        public Game() : base(800, 800, GraphicsMode.Default, "OpenTK")
        {
            VSync = VSyncMode.Adaptive;
        }        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Keyboard.KeyDown += Keyboard_KeyDown;
            Keyboard.KeyRepeat = false;

            SetupGLStates();

            GenerateBuffers();

            LoadShaders();

            InitScene();
        }

        private static void SetupGLStates()
        {
            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);

            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
        }

        private void InitScene()
        {
            GL.UseProgram(program);

            angleLocation = GL.GetUniformLocation(program, "angle");
            projectionMatrixLocation = GL.GetUniformLocation(program, "projMat");
            modelMatrixLocation = GL.GetUniformLocation(program, "modelMat");

            float AR = ClientSize.Width / ClientSize.Height;
            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AR, 0.1f, 1000.0f, out projectionMatrix4);
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix4);

            modelMatrix4 = Matrix4.LookAt(0, 3.0f, 10.0f, 0, 0, 0, 0, 1.0f, 0);
            GL.UniformMatrix4(modelMatrixLocation, false, ref modelMatrix4);

            GL.UseProgram(0);
        }

        private void LoadShaders()
        {
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
        }

        private void GenerateBuffers()
        {
            // Generate Vertex array
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // Vertices
            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cube.Length * sizeof(float)), cube, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out normals);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normals);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cube.Length * sizeof(float)), cube, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out index);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, index);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(float)), indices, BufferUsageHint.StaticDraw);

            // Set position data                        
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            // Set normals
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Enable the attributes
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            GL.UseProgram(program);

            float AR = ClientSize.Width / ClientSize.Height;
            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AR, 0.1f, 1000.0f, out projectionMatrix4);
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix4);

            GL.UseProgram(0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();          

            GL.UseProgram(program);

            GL.Uniform1(angleLocation, angle);
            angle += 1.0f * (float)e.Time;

            Matrix4 rotationY = Matrix4.CreateRotationY((float)e.Time);
            Matrix4.Mult(ref rotationY, ref modelMatrix4, out modelMatrix4);
            GL.UniformMatrix4(modelMatrixLocation, false, ref modelMatrix4);

            GL.UseProgram(0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(program);

            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.UseProgram(0);

            SwapBuffers();
        }        
    }
}