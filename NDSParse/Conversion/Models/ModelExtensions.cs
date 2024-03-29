using NDSParse.Conversion.Models.Formats;
using NDSParse.Conversion.Models.Mesh;
using NDSParse.Conversion.Models.Processing;
using NDSParse.Conversion.Textures.Images;
using NDSParse.Objects.Exports.Meshes;
using NDSParse.Objects.Exports.Textures;
using SixLabors.ImageSharp;

namespace NDSParse.Conversion.Models;

public static class ModelExtensions
{
    public static List<Model> ExtractModels(this BMD0 bmd0, TEX0? textureReference = null)
    {
        textureReference ??= bmd0.TextureData;
        
        var models = new List<Model>();
        foreach (var dataModel in bmd0.ModelData.Models)
        {
            var processor = new MeshProcessor(dataModel);
            
            var model = new Model();
            model.Name = dataModel.Name;
            model.Sections = processor.Process();

            var vertexIndex = 0;
            foreach (var section in model.Sections)
            {
                foreach (var polygon in section.Polygons)
                {
                    switch (polygon.PolygonType)
                    {
                        case PolygonType.TRI:
                        {
                            for (var vtxCounter = 0; vtxCounter < polygon.Vertices.Count; vtxCounter += 3)
                            {
                                var face = new Face(section.MaterialName);
                                for (var vtxIdx = 0; vtxIdx < 3; vtxIdx++)
                                {
                                    face.AddIndex(vertexIndex);
                                    vertexIndex++;
                                }
                                section.Faces.Add(face);
                            }
                            break;
                        }
                        case PolygonType.QUAD:
                        {
                            for (var vtxCounter = 0; vtxCounter < polygon.Vertices.Count; vtxCounter += 4)
                            {
                                var face = new Face(section.MaterialName);
                                var indices = new int[4];
                                for (var vtxIdx = 0; vtxIdx < 4; vtxIdx++)
                                {
                                    indices[vtxIdx] = vertexIndex;
                                    vertexIndex++;
                                }
                                
                                face.AddIndex(indices[0]);
                                face.AddIndex(indices[1]);
                                face.AddIndex(indices[2]);
                                
                                face.AddIndex(indices[2]);
                                face.AddIndex(indices[3]);
                                face.AddIndex(indices[0]);
                                
                                section.Faces.Add(face);
                            }
                            break;
                        }
                        case PolygonType.TRI_STRIP:
                        case PolygonType.QUAD_STRIP:
                        {
                            for (var vtxCounter = 0; vtxCounter + 2 < polygon.Vertices.Count; vtxCounter += 2)
                            {
                                var firstFace = new Face(section.MaterialName);
                                firstFace.AddIndex(vertexIndex + vtxCounter);
                                firstFace.AddIndex(vertexIndex + vtxCounter + 1);
                                firstFace.AddIndex(vertexIndex + vtxCounter + 2);
                                section.Faces.Add(firstFace);

                                if (vtxCounter + 3 < polygon.Vertices.Count)
                                {
                                    var extraFace = new Face(section.MaterialName);
                                    extraFace.AddIndex(vertexIndex + vtxCounter + 1);
                                    extraFace.AddIndex(vertexIndex + vtxCounter + 3);
                                    extraFace.AddIndex(vertexIndex + vtxCounter + 2);
                                    section.Faces.Add(extraFace);
                                }
                                
                            }
                            
                            vertexIndex += polygon.Vertices.Count;
                            break;
                        }
                    }
                }
            }

            foreach (var dataMaterial in dataModel.Materials)
            {
                var material = new Material
                {
                    Name = dataMaterial.Name,
                    Texture = textureReference?.Textures.FirstOrDefault(texture => texture.Name.Equals(dataMaterial.TextureName, StringComparison.OrdinalIgnoreCase)),
                    FlipU = dataMaterial.FlipU,
                    FlipV = dataMaterial.FlipV,
                    RepeatU = dataMaterial.RepeatU,
                    RepeatV = dataMaterial.RepeatV
                };
                model.Materials.Add(material);
            }
            models.Add(model);
        }
        return models;
    }

    public static void SaveToDirectory(this Model model, string path)
    {
        Directory.CreateDirectory(path);
        model.SaveModel(Path.Combine(path, $"{model.Name}.obj"));
        model.SaveTextures(path);
    }

    public static void SaveTextures(this Model model, string path)
    {
        foreach (var material in model.Materials)
        {
            material.Texture?.ToImage().SaveAsPng(Path.Combine(path, $"{material.Texture?.Name}.png"));
        }
    }

    public static void SaveModel(this Model model, string path)
    {
        new OBJ(model).Save(path);
    }
}
