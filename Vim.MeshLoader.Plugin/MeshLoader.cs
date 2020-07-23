using Assimp;
using System.Linq;
using Vim.Desktop.Api;
using Vim.Math3d;
using Matrix4x4 = Vim.Math3d.Matrix4x4;

namespace Vim.MeshLoader.Plugin
{
    [VimPlugin]
    public class MeshLoader : VimPluginBaseClass
    {
        public bool MeshLoaded;
        public IMesh Mesh;
        public IInstance Instance;
        public Vector3[] Vertices;
        public uint[] Indices;
        public ColorRGBA Color = new ColorRGBA(128, 128, 128, 255);

        public override void Init(IVimApi api)
        {
            base.Init(api);

            new ColorRGBA(128, 128, 128, 255);

            // TODO: change me!
            var filePath = @"C:\dev\repos\assimp\test\models\OBJ\WusonOBJ.obj";

            using (var context = new AssimpContext())
            {
                var mesh = context.ImportFile(filePath, PostProcessSteps.Triangulate).Meshes[0];
                Vertices = mesh.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z) * (float)Constants.FeetToMm / 10f).ToArray();
                Indices = new uint[mesh.Faces.Count * 3 * 2];
                var c = 0;
                foreach (var f in mesh.Faces)
                {
                    Indices[c++] = (uint)f.Indices[0];
                    Indices[c++] = (uint)f.Indices[1];
                    Indices[c++] = (uint)f.Indices[2];
                    
                    // Provide the back triangle as well.
                    Indices[c++] = (uint)f.Indices[2];
                    Indices[c++] = (uint)f.Indices[1];
                    Indices[c++] = (uint)f.Indices[0];
                }
                MeshLoaded = true;
            }
        }

        public override void OnFrameUpdate(float deltaTime)
        {
            base.OnFrameUpdate(deltaTime);
            if (MeshLoaded && Mesh == null)
            {
                // Mesh creation has to happen in an "OnFrameUpdate" call. 
                Mesh = API.Scene.CreateMesh(Vertices, Indices, Color);
                Instance = API.Scene.CreateInstance(Mesh, Matrix4x4.Identity, Color);
            }
        }
    }
}
