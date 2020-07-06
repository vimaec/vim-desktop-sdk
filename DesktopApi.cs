using System;
using System.Threading.Tasks;
using Vim.Math3d;

namespace Vim.Desktop.Api
{
    public interface IVimApi
    {
        Task OpenFile(string name);
        void Clear();
        string CurrentFile { get; }
        IScene Scene { get; }
        IViewport Viewport { get; }
    }

    public interface ICamera
    {
        void Reset();
        Vector3 Position { get; set; }
        Quaternion Orientation { get; set; }
    }

    public struct RenderSettings
    {
        public float SSAORadius;
        public float SSAOFallOff;
        public float MinObjectSize;
        public float ToneMappingWhitePoint;
        public float ToneMappingScale;
        public float IBLBlur;
        public float TAAResolveBlend;
        public float ImageDithering;
    }

    public struct ElementData
    {
        public int Id;
        public int LevelIndex;
        public int PhaseIndex;
        public int CategoryIndex;
        public int FamilyNameIndex;
        public string Type;
        public string Name;
        public string FamilyName;
        public bool HasLevelData;
        public string LevelName;
        public double LevelElevation;
        public bool HasPhaseData;
        public string PhaseName;
        public Tuple<string, string>[] Properties;
    }

    public struct HitTestResult
    {
        public bool Success;
        public uint ElementIndex;
        public IntPtr InstancePtr;
        public uint FaceIndex;
        public Vector3 HitPoint;
        public ElementData ElementData;
    }

    public interface IViewport
    {
        IntPtr WindowHwnd { get; }
        Task<HitTestResult> HitTest(float x, float y);
        ICamera Camera { get; }
        RenderSettings RenderSettings { get; set; }
    }

    public interface ISceneElement
    {
        void Destroy();
    }

    public interface IMesh : ISceneElement
    {
        IntPtr Ptr { get; }
    }

    public interface IInstance : ISceneElement
    {
        IMesh Mesh { get; }
        Matrix4x4 Transform { get; set; }
        ColorRGBA Color { get; set; }
        bool Visible { get; set; }
    }

    public interface ILine : ISceneElement
    {
        Vector3 A { get; set; }
        Vector3 B { get; set; }
        float Width { get; set; }
        ColorRGBA Color { get; set; }
    }

    public interface IText : ISceneElement
    {
        string Content { get; set; }
        ColorRGBA Color { get; set; }
        Vector3 Position { get; set; }
    }

    public interface IScene 
    {
        int NumElements { get; }
        ElementData GetElementData(uint elementIndex);
        void SetElementVisibility(byte[] visible);
        void ClearScene();
        bool IsVimLoaded();
        AABox GetBoundsOfVim();
        IMesh CreateMesh(Vector3[] vertices, uint[] indices, ColorRGBA color);
        IInstance CreateInstance(IMesh mesh, Matrix4x4 transform, ColorRGBA color);
        ILine CreateLine(Vector3 point1, Vector3 point2, float width, ColorRGBA color);
        IText CreateText(string content, Vector3 position, ColorRGBA color);
    }

    public interface IVimPlugin
    {
        void Init(IVimApi api);
        void OnFrameUpdate(float deltaTime);
        void OnOpenFile(string fileName);
        void OnCloseFile();
    }
}
