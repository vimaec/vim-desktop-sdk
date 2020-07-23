using Assimp;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Vim.DataFormat;
using Vim.Desktop.Api;
using Vim.Hackathon;
using Vim.Math3d;
using Matrix4x4 = Vim.Math3d.Matrix4x4;

namespace Vim.MeshLoader.Plugin
{
    [VimPlugin]
    public class MeshLoader : VimPluginBaseClass
    {
        public class VimMeshData
        {
            public Vector3[] Vertices;
            public uint[] Indices;
            public IMesh ApiMesh;
        }

        public class VimInstanceData
        {
            public Matrix4x4 WorldTransform;
            public int MeshIndex;
            public IInstance ApiInstance;
        }

        public List<VimMeshData> LoadedMeshes = new List<VimMeshData>();
        public List<VimInstanceData> LoadedInstances = new List<VimInstanceData>();

        public bool meshesLoaded;
        public bool meshesCreated;

        public ColorRGBA Color = new ColorRGBA(128, 128, 128, 255);
        public AssimpContext context = new AssimpContext();

        public static VimMeshData ToMeshData(Assimp.Mesh mesh)
        {
            var r = new VimMeshData();
            r.Vertices = mesh.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z) * (float)Constants.FeetToMm / 10f).ToArray();
            r.Indices = new uint[mesh.Faces.Count * 3 * 2];
            var c = 0;
            foreach (var f in mesh.Faces)
            {
                r.Indices[c++] = (uint)f.Indices[0];
                r.Indices[c++] = (uint)f.Indices[1];
                r.Indices[c++] = (uint)f.Indices[2];

                // Provide the back triangle as well.
                r.Indices[c++] = (uint)f.Indices[2];
                r.Indices[c++] = (uint)f.Indices[1];
                r.Indices[c++] = (uint)f.Indices[0];
            }
            return r;
        }

        public static Matrix4x4 ToMath3d(Assimp.Matrix4x4 m)
            => new Matrix4x4(
                m.A1, m.A2, m.A3, m.A4,
                m.B1, m.B2, m.B3, m.B4,
                m.C1, m.C2, m.C3, m.C4,
                m.D1, m.D2, m.D3, m.D4);

        public static Matrix4x4 ToMath3d(double[] m)
            => new Matrix4x4(
                (float)m[0], (float)m[1], (float)m[2], (float)m[3],
                (float)m[4], (float)m[5], (float)m[6], (float)m[7],
                (float)m[8], (float)m[9], (float)m[10], (float)m[11],
                (float)m[12], (float)m[13], (float)m[14], (float)m[15]);

        public static VimMeshData ToMeshData(GeometryBuilder gb)
            => new VimMeshData
            {
                Vertices = gb.Vertices.ToArray(),
                Indices = gb.Indices.Select(i => (uint)i).ToArray()
            };

        public static void CollectInstances(Assimp.Node node, Matrix4x4 transform, List<VimInstanceData> r)
        {
            transform *= ToMath3d(node.Transform);
            foreach (var i in node.MeshIndices) {
                var inst = new VimInstanceData { 
                    WorldTransform = transform, 
                    MeshIndex = i };
                r.Add(inst);
            }
            foreach (var c in node.Children)
                CollectInstances(c, transform, r);
        }

        public static void CollectInstances(Va3cObject obj, Dictionary<string, int> geoLookup, Matrix4x4 transform, List<VimInstanceData> r)
        {
            transform *= ToMath3d(obj.matrix);
            if (geoLookup.ContainsKey(obj.geometry))
            {
                var inst = new VimInstanceData
                {
                    WorldTransform = transform,
                    MeshIndex = geoLookup[obj.geometry]
                };
                r.Add(inst);
            }
            foreach (var c in obj.children)
                CollectInstances(c, geoLookup, transform, r);
        }

        public void LoadAssimpFile(string filePath)
        {
            var scene = context.ImportFile(filePath, PostProcessSteps.Triangulate);
            LoadedMeshes = scene.Meshes.Select(ToMeshData).ToList();
            CollectInstances(scene.RootNode, Matrix4x4.Identity, LoadedInstances);
            meshesLoaded = true;
        }

        public void LoadVa3CFile(string filePath)
        {
            var va3c = VimHackerProgram.LoadVa3c(filePath);
            LoadedMeshes = va3c.geometries.Select(g => ToMeshData(g.ToGeometryBuilder())).ToList();

            var geoLookup = new Dictionary<string, int>();
            foreach (var g in va3c.geometries)
                geoLookup.Add(g.uuid, geoLookup.Count);

            CollectInstances(va3c.obj, geoLookup, Matrix4x4.Identity, LoadedInstances);
            meshesLoaded = true;
        }

        public override void Init(IVimApi api)
        {
            base.Init(api);

            // TODO: change me!
            //var filePath = @"C:\dev\repos\assimp\test\models\OBJ\WusonOBJ.obj";
            //LoadAssimpFile(filePath);

            var filePath = @"C:\dev\repos\vim-desktop-sdk\test-data\input\BIMSocket.json";
            LoadVa3CFile(filePath);
        }

        public override void OnFrameUpdate(float deltaTime)
        {
            base.OnFrameUpdate(deltaTime);

            // Mesh creation has to happen in an "OnFrameUpdate" call. 
            if (meshesLoaded && !meshesCreated)
            {
                // Create the meshes 
                foreach (var md in LoadedMeshes)
                {
                    md.ApiMesh = API.Scene.CreateMesh(md.Vertices, md.Indices, Color);
                }

                // Create the instances
                foreach (var inst in LoadedInstances)
                {
                    var mesh = LoadedMeshes[inst.MeshIndex];
                    inst.ApiInstance = API.Scene.CreateInstance(mesh.ApiMesh, Matrix4x4.Identity, Color);
                }

                meshesCreated = true;
            }
        }
    }
}
