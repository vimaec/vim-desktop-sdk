using System.IO;
using Vim.DataFormat;
using Vim.BFast;
using static Vim.DataFormat.Serializer;
using Vim.DotNetUtilities;
using Vim.Buffers;
using Vim.G3d;
using Vim.DotNetUtilities.JsonSerializer;
using System.Linq;
using Vim.LinqArray;
using System.Collections.Generic;
using System;
using Vim.Geometry;

namespace Vim.HelloWorld.Plugin
{
    public static class ExportToText
    {
        public static void ExportAsset(INamedBuffer asset, string assetFolder)
        {
            var ext = Path.GetExtension(asset.Name);
            var extPos = asset.Name.LastIndexOf('.');
            var baseName = extPos >= 0 ? asset.Name.Substring(0, extPos) : asset.Name;
            var validName = Util.ToValidFileName(baseName);
            var assetFile = Path.Combine(assetFolder, validName + ext);
            File.WriteAllBytes(assetFile, asset.Bytes.ToArray());
        }

        public static IEnumerable<string> GetEntityTableRowValues(this SerializableEntityTable et, int n, string[] strings)
        {
            foreach (var col in et.NumericColumns)
                yield return col.Data[n].ToString();
            foreach (var col in et.StringColumns)
                yield return col.Data[n] >= 0 ? strings[col.Data[n]] : "";
            foreach (var col in et.IndexColumns)
                yield return col.Data[n].ToString();
        }

        public static IEnumerable<string> GetColumnNames(this SerializableEntityTable et)
        {
            foreach (var col in et.NumericColumns)
                yield return col.Name;
            foreach (var col in et.StringColumns)
                yield return col.Name;
            foreach (var col in et.IndexColumns)
                yield return col.Name;
        }

        public static void ExportEntityTable(INamedBuffer buffer, string entityFolder, string[] strings)
        {
            var buffers = buffer.Unpack();
            var baseName = Util.ToValidFileName(buffer.Name);
            var fileName = Path.Combine(entityFolder, baseName + ".csv");

            var et = new SerializableEntityTable();
            foreach (var b in buffers)
            {
                if (b.Name.StartsWith(VimConstants.IndexColumnNamePrefix))
                    et.IndexColumns.Add(b.StripPrefix().ToTypedBuffer<int>());
                else if (b.Name.StartsWith(VimConstants.NumberColumnNamePrefix))
                    et.NumericColumns.Add(b.StripPrefix().ToTypedBuffer<double>());
                else if (b.Name.StartsWith(VimConstants.StringColumnNamePrefix))
                    et.StringColumns.Add(b.StripPrefix().ToTypedBuffer<int>());
                else if (b.Name == VimConstants.PropertiesBufferName)
                    et.Properties = b.AsSpan<SerializableProperty>().ToArray();
                else
                    throw new Exception($"{b.Name} is not a recognized entity table buffer");
            }

            var numRows = et.GetNumRows();
            var rows = Enumerable
                .Range(0, numRows)
                .Select(row => et.GetEntityTableRowValues(row, strings))
                .Prepend(et.GetColumnNames());
            File.WriteAllLines(fileName, rows.Select(CsvUtil.ToCsvRow));

            if (et.Properties.Length > 0)
            {
                var d = new Dictionary<int, Dictionary<string, string>>();
                foreach (var p in et.Properties)
                {
                    if (!d.ContainsKey(p.EntityIndex))
                        d.Add(p.EntityIndex, new Dictionary<string, string>());
                    var key = p.Name >= 0 ? strings[p.Name] : "";
                    var val = p.Value >= 0 ? strings[p.Value] : "";
                    d[p.EntityIndex].AddOrReplace(key, val);
                }
                JsonUtil.ToJsonFile(d, Path.Combine(entityFolder, baseName) + ".json");
            }
        }

        public static void Export(string filePath, string outputFolder)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var reader = new BinaryReader(stream);
                var br = new BFastReader(reader);

                // Header 
                var header = br.ReadBuffer(BufferNames.Header).GetString();
                File.WriteAllText(Path.Combine(outputFolder, BufferNames.Header + ".txt"), header.ToString());

                // Assets             
                var assets = br.ReadBFast(BufferNames.Assets).ReadAllBuffers();
                var assetFolder = Path.Combine(outputFolder, BufferNames.Assets);
                Util.CreateAndClearDirectory(assetFolder);
                foreach (var asset in assets)
                    ExportAsset(asset, assetFolder);

                // Strings
                var strings = br.ReadBuffer(BufferNames.Strings).GetStrings();
                File.WriteAllLines(Path.Combine(outputFolder, BufferNames.Strings + ".txt"), strings);

                // Get the geometry 
                var geometry = br.ReadBFast(BufferNames.Geometry);
                var geometryBuffers = geometry.ReadAllBuffers();
                var g3d = geometryBuffers.ToG3D();
                g3d.Write(Path.Combine(outputFolder, BufferNames.Geometry + ".g3d"));

                // Get the individual meshes for interest's sake
                var meshFolder = Path.Combine(outputFolder, "meshes");
                Util.CreateAndClearDirectory(meshFolder);
                var meshes = g3d.ToIMesh().SplitByGroup();
                for (var i=0; i < meshes.Count; ++i)
                {
                    meshes[i].ToIMesh().WriteObj(Path.Combine(meshFolder, i.ToString() + ".obj"));
                    // TODO: get the element name and stuff . 
                }

                // Nodes 
                var nodeBuffer = br.ReadBuffer(BufferNames.Nodes);
                var nodes = Util.Cast<SerializableSceneNode>(nodeBuffer.Bytes).ToArray();
                JsonUtil.ToJsonFile(nodes, Path.Combine(outputFolder, BufferNames.Nodes + ".json"));

                // Entities 
                var entities = br.ReadBFast(BufferNames.Entities);
                var entityBuffers = entities.ReadAllBuffers();
                var entityFolder = Path.Combine(outputFolder, BufferNames.Entities);
                Util.CreateAndClearDirectory(entityFolder);

                // This class parses the entity tables further
                foreach (var entity in entityBuffers)
                    ExportEntityTable(entity, entityFolder, strings);
            }
        }
    }
}

