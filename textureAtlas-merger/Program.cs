using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json;

/// dotnet run -- 4 ..\output ..\croplist.json

namespace csharp {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(string.Join(' ', args));
            if (args.Length < 3) {
                Console.WriteLine("Invalid arguments. Please input the scale multiplier, followed by the folder containing the output of textureAtlas-slicer (all the .png files) and the parameters .json file");
                Console.WriteLine("Example arguments: 4 ..\\output ..\\croplist.json");
                return;
            }

            var multiplier = int.Parse(args[0]);
            if (multiplier <= 0) {
                Console.WriteLine("Invalid multiplier. Please use integer values");
                return;
            }

            var texturePath = args[1] + "\\slicedImage.png";
            var texture = Image.Load(texturePath);
            
            var parametersString = File.ReadAllText(args[2]);
            var parameters = JsonConvert.DeserializeObject<Rectangle[]>(parametersString);
            
            foreach (var parameter in parameters) {
                var upscaledImagePath = args[1] + "\\" + GetSimpleFilename(parameter) + ".png";
                var upscaledImage = Image.Load(upscaledImagePath);
                var upscaledImageLocation = new Point(multiplier * parameter.X, multiplier * parameter.Y);
                texture.Mutate(context => context.DrawImage(
                    upscaledImage,
                    upscaledImageLocation,
                    SixLabors.ImageSharp.PixelFormats.PixelColorBlendingMode.Normal,
                    SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.SrcOver,
                    1.0f));
            }
            texture.SaveAsPng(args[1] + "\\finalImage.png");
        }
        public static string GetSimpleFilename(Rectangle rectangle) { 
            return rectangle.X.ToString() + "-" + rectangle.Y.ToString() + "-" + rectangle.Width.ToString() + "-" + rectangle.Height.ToString();
        }

        public static string GetHashString(Rectangle rectangle) {
            var parameterString = JsonConvert.SerializeObject(rectangle);
            var parameterBytes = Encoding.UTF8.GetBytes(parameterString);
            HashAlgorithm algorithm = SHA256.Create();
            var resultBytes = algorithm.ComputeHash(parameterBytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in resultBytes) {
                sb.Append(b.ToString("x"));
            }
            return sb.ToString();
        }
    }
}
