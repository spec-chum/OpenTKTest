using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.IO;

namespace OpenTKTest
{
    internal class Game : GameWindow
    {
        private float[] cube =
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

        private float[] colours =
        {
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f
        };
        private int[] indices =
        {
            0, 1, 2,    2, 1, 3,    // Front
            5, 4, 7,    6, 7, 4,    // Back
            4, 0, 6,    6, 0, 2,    // Left
            1, 5, 7,    7, 3, 1,    // Right
            0, 4, 5,    5, 1, 0,    // Top
            2, 7, 6,    2, 3, 7     // Bottom
        };

        private float angle, time;
        private Matrix4 MVP, projectionMatrix4, modelMatrix4;
        private int MVPLocation, modelMatrixLocation, angleLocation;
        private int vao, vbo, coloursVbo, normals, index, program, status;
        private bool wireframe = false;

        public Game()
            : base(800, 800, GraphicsMode.Default, "OpenTK")
        {
            VSync = VSyncMode.Adaptive;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Declare keyboard callback
            Keyboard.KeyDown += Keyboard_KeyDown;
            Keyboard.KeyRepeat = false;

            // Initialise everything
            SetupGLStates();
            GenerateBuffers();
            LoadShaders();
            InitScene();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(program);
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.UseProgram(0);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            // Update projection matrix for new window size
            float ar = (float)ClientSize.Width / (float)ClientSize.Height;
            projectionMatrix4 = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, ar, 0.1f, 1000.0f);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            time = (float)e.Time;

            // Spin!
            GL.UseProgram(program);
            GL.Uniform1(angleLocation, angle);
            angle += 1.0f * time;

            // User input - Just rotation for now
            if (Keyboard[Key.Left])
            {
                Matrix4 rotationY = Matrix4.CreateRotationY(-time);
                Matrix4.Mult(ref rotationY, ref modelMatrix4, out modelMatrix4);
            }

            if (Keyboard[Key.Right])
            {
                Matrix4 rotationY = Matrix4.CreateRotationY(time);
                Matrix4.Mult(ref rotationY, ref modelMatrix4, out modelMatrix4);
             }

            if (Keyboard[Key.Up])
            {
                Matrix4 rotationX = Matrix4.CreateRotationX(-time);
                Matrix4.Mult(ref rotationX, ref modelMatrix4, out modelMatrix4);
             }

            if (Keyboard[Key.Down])
            {
                Matrix4 rotationX = Matrix4.CreateRotationX(time);
                Matrix4.Mult(ref rotationX, ref modelMatrix4, out modelMatrix4);
            }

            // Update MVP and modelView matricies (for normals)
            MVP = modelMatrix4 * projectionMatrix4;
            GL.UniformMatrix4(MVPLocation, false, ref MVP);
            GL.UniformMatrix4(modelMatrixLocation, false, ref modelMatrix4);
            GL.UseProgram(0);
        }

        private static void SetupGLStates()
        {
            GL.ClearColor(0, 0, 0, 1);
            GL.Enable(EnableCap.DepthTest);

            // Don't need back faces for a cube
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.CullFace);
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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Colours
            GL.GenBuffers(1, out coloursVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, coloursVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colours.Length * sizeof(float)), colours, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Normals
            GL.GenBuffers(1, out normals);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normals);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cube.Length * sizeof(float)), cube, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Index
            GL.GenBuffers(1, out index);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, index);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

            // Enable the attributes
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
        }

        private void InitScene()
        {
            // Set up uniform locations
            GL.UseProgram(program);
            angleLocation = GL.GetUniformLocation(program, "angle");
            MVPLocation = GL.GetUniformLocation(program, "MVP");
            modelMatrixLocation = GL.GetUniformLocation(program, "modelMat");

            // Initial MVP and modelView
            float ar = (float)ClientSize.Width / (float)ClientSize.Height;
            projectionMatrix4 = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, ar, 0.1f, 1000.0f);
            modelMatrix4 = Matrix4.LookAt(0, 3.0f, 10.0f, 0, 0, 0, 0, 1.0f, 0);
            GL.UseProgram(0);
        }

        private void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
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

                case Key.Escape:
                    Exit();
                    break;
            }
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

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
        }
    }
}