﻿using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Newtonsoft.Json;

namespace TextureAtlasTools {
    class Program {
        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Invalid arguments. Please specify an action [ split | merge ], folowed by a base filename (without extension) and an optional scale multiplier (for the merge action)");
                Console.WriteLine("The split action needs to be run first. For this action, both the texture .png file and the parameters .json file should be at the same place and have the same name, but different extensions.");
                Console.WriteLine("An output folder with the base filename will be created and populated with the separated textures.");
                Console.WriteLine("For the merge action, a folder generated by textureAtlas-slicer must exist first. The [basefilename]_sliced.png (generated by textureAtlas-slicer) as well as [basefilename].json should also exist at the path specified. An additional scale multiplier int parameter is also needed");
                Console.WriteLine("Example1: TextureAtlasTools.exe split tex1_512x256_B20814E2D6573DFE_0");
                Console.WriteLine("Example2: TextureAtlasTools.exe merge tex1_512x256_B20814E2D6573DFE_0 4");
                return;
            }

            var baseFilename = args[1];
            try {
                if (args[0] == "split") {
                    SplitLogic(baseFilename);
                } else if (args[0] == "merge") {
                    if (args.Length < 3) {
                        Console.WriteLine("Missing scale multiplier parameter");
                    } else {
                        var multiplier = int.Parse(args[2]);
                        MergeLogic(baseFilename, multiplier);
                    }
                } else {
                    Console.WriteLine("Invalid action. Please specify an action [ split | merge ], folowed by a base filename (without extension) and an optional scale multiplier (for the merge action)");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        static void SplitLogic(string baseFilename) {
            var texturePath = $"{baseFilename}.png";
            if (File.Exists(texturePath) == false) { Console.WriteLine($"Invalid filename. {texturePath} missing"); return; }
            var texture = Image.Load(texturePath);

            var parametersPath = $"{baseFilename}.json";
            if (File.Exists(parametersPath) == false) { Console.WriteLine($"Invalid filename. {parametersPath} missing"); return; }
            var parametersString = File.ReadAllText(parametersPath);
            var parameters = JsonConvert.DeserializeObject<Rectangle[]>(parametersString);
            
            System.IO.Directory.CreateDirectory(baseFilename);
            foreach (var parameter in parameters) {
                var croppedImage = texture.Clone(context => context.Crop(parameter));
                var emptyImage = new Image<Rgba32>(parameter.Width, parameter.Height);
                var location = new Point(parameter.X, parameter.Y);
                texture.Mutate(context => context.DrawImage(
                    emptyImage,
                    location,
                    SixLabors.ImageSharp.PixelFormats.PixelColorBlendingMode.Normal,
                    SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.Clear,
                    1.0f));
                var resultPath = $"{baseFilename}\\{GetSimpleFilename(parameter)}.png";
                croppedImage.SaveAsPng(resultPath);
            }
            texture.SaveAsPng($"{baseFilename}_sliced.png");
        }

        static void MergeLogic(string baseFilename, int multiplier) {
            if (multiplier <= 0) {
                Console.WriteLine("Invalid multiplier. Please use integer values");
                return;
            }

            var originalTexturePath = $"{baseFilename}.png";
            if (File.Exists(originalTexturePath) == false) { Console.WriteLine($"Invalid filename. {originalTexturePath} missing"); return; }
            var originalTexture = Image.Load(originalTexturePath);

            var texturePath = $"{baseFilename}_sliced.png";
            if (File.Exists(texturePath) == false) { Console.WriteLine($"Invalid filename. {texturePath} missing"); return; }
            var texture = Image.Load(texturePath);
            var resampler = new NearestNeighborResampler();
            if (multiplier * originalTexture.Size().Width > texture.Size().Width) {
                Console.WriteLine($"sliced texture not upscaled. resizing with {multiplier}x multiplier");
                texture.Mutate(context => context.Resize(
                    multiplier * originalTexture.Size().Width,
                    multiplier * originalTexture.Size().Height,
                    resampler));
            }
            
            var parametersPath = $"{baseFilename}.json";
            if (File.Exists(parametersPath) == false) { Console.WriteLine($"Invalid filename. {parametersPath} missing"); return; }
            var parametersString = File.ReadAllText(parametersPath);
            var parameters = JsonConvert.DeserializeObject<Rectangle[]>(parametersString);
            
            DirectoryInfo taskDirectory = new DirectoryInfo(baseFilename);

            foreach (var parameter in parameters) {
                var wildcardImagePath = $"{GetSimpleFilename(parameter)}*.png";
                FileInfo[] taskFiles = taskDirectory.GetFiles(wildcardImagePath);
                if (taskFiles.Length >= 1) {
                    if (taskFiles.Length != 1) {
                        Console.WriteLine("multiple files!");
                        foreach (var file in taskFiles) {
                            Console.WriteLine($"{file.Name}");
                        }
                    }
                    var simpleFilename = taskFiles[0].Name;
                    var upscaledImagePath = $"{baseFilename}\\{simpleFilename}";
                    var upscaledImage = Image.Load(upscaledImagePath);
                    if (multiplier * parameter.Width > upscaledImage.Size().Width) {
                        Console.WriteLine($"{simpleFilename} texture not upscaled. resizing with {multiplier}x multiplier");
                        upscaledImage.Mutate(context => context.Resize(
                            multiplier * parameter.Width,
                            multiplier * parameter.Height,
                            resampler));
                    }
                    var upscaledImageLocation = new Point(multiplier * parameter.X, multiplier * parameter.Y);
                    texture.Mutate(context => context.DrawImage(
                        upscaledImage,
                        upscaledImageLocation,
                        SixLabors.ImageSharp.PixelFormats.PixelColorBlendingMode.Normal,
                        SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.SrcOver,
                        1.0f));
                } else {
                    Console.WriteLine($"Missing slice with name: {wildcardImagePath}. The final image will be incomplete");
                }
            }
            texture.SaveAsPng($"{baseFilename}_final.png");
        }

        public static string GetSimpleFilename(Rectangle rectangle) {
            return $"{rectangle.Width.ToString()}x{rectangle.Height.ToString()}@{rectangle.X.ToString()}x{rectangle.Y.ToString()}";
        }
    }
}
