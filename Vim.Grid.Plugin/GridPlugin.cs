using System;
using System.Collections.Generic;
using System.Threading;
using Vim.Desktop.Api;
using Vim.Math3d;

namespace Vim.GridPlugin
{
    [VimPlugin]
    public class GridPlugin : VimPluginBaseClass
    {
        private List<ILine> Lines = new List<ILine>();
        private List<IText> Texts = new List<IText>();
        float TotalTime = 0.0f;
        private static Mutex UpdateMutex = new Mutex();

        public override void Init(IVimApi api)
        {
            base.Init(api);
        }

        public override void OnOpenFile(string fileName)
        {
            base.OnOpenFile(fileName);

            UpdateMutex.WaitOne();
            for (int i = -10; i <= 10; i++)
            {
                Lines.Add(API.Scene.CreateLine(new Vector3(-100, 0.0f, i * 10), new Vector3(100, 0.0f, i * 10), 1.0f, new ColorRGBA(128, 255, 128, 255)));
                Texts.Add(API.Scene.CreateText(i.ToString(), new Vector3(-100, 0.0f, i * 10), new ColorRGBA(128, 255, 128, 255)));
                Texts.Add(API.Scene.CreateText(i.ToString(), new Vector3(100, 0.0f, i * 10), new ColorRGBA(128, 255, 128, 255)));
            }

            for (int i = -10; i <= 10; i++)
            {
                Lines.Add(API.Scene.CreateLine(new Vector3(i * 10, 0.0f, -100), new Vector3(i * 10, 0.0f, 100), 1.0f, new ColorRGBA(128, 255, 128, 255)));
                Texts.Add(API.Scene.CreateText(i.ToString(), new Vector3(i * 10, 0.0f, -100), new ColorRGBA(128, 255, 128, 255)));
                Texts.Add(API.Scene.CreateText(i.ToString(), new Vector3(i * 10, 0.0f, 100), new ColorRGBA(128, 255, 128, 255)));
            }

            UpdateMutex.ReleaseMutex();
        }

        public override void OnCloseFile()
        {
            if (UpdateMutex.WaitOne())
            {
                foreach (var line in Lines)
                {
                    line.Destroy();
                }
                foreach (var text in Texts)
                {
                    text.Destroy();
                }

                Lines.Clear();
                Texts.Clear();
                UpdateMutex.ReleaseMutex();
            }
        }

        public override void OnFrameUpdate(float deltaTime)
        {
            TotalTime += deltaTime;

            var newLineColor = new ColorRGBA((byte)(128 + Math.Sin(TotalTime * 5) * 64.0 + 63.0), (byte)(128 + Math.Sin(TotalTime * 7) * 64.0 + 63.0), (byte)(128 + Math.Sin(TotalTime * 11) * 64.0 + 63.0), 255);
            var newTextColor = new ColorRGBA((byte)(128 + Math.Sin(TotalTime * 11) * 64.0 + 63.0), (byte)(128 + Math.Sin(TotalTime * 5) * 64.0 + 63.0), (byte)(128 + Math.Sin(TotalTime * 7) * 64.0 + 63.0), 255);

            if (UpdateMutex.WaitOne(0))
            {
                foreach (var line in Lines)
                {
                    line.Color = newLineColor;
                }
                foreach (var text in Texts)
                {
                    text.Color = newTextColor;
                }
                UpdateMutex.ReleaseMutex();
            }
        }
    }
}

