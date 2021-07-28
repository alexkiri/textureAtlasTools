using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json;

/// dotnet run -- ..\tex1_512x256_B20814E2D6573DFE_0 ..\output

namespace csharp {
    class Program {
        static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("Invalid arguments. Please specify a base filename (without extension) followed by the output folder. Both the texture .png file and the parameters .json file should be at the same place and have the same name, but different extensions.");
                Console.WriteLine("Example arguments: ..\\tex1_512x256_B20814E2D6573DFE_0 ..\\output");
                return;
            }

            var texturePath = $"{args[0]}.png";
            if (File.Exists(texturePath) == false) { Console.WriteLine($"Invalid filename. {texturePath} missing"); return; }
            var texture = Image.Load(texturePath); // throws ¯\_(ツ)_/¯

            var parametersPath = $"{args[0]}.json";
            if (File.Exists(parametersPath) == false) { Console.WriteLine($"Invalid filename. {parametersPath} missing"); return; }
            var parametersString = File.ReadAllText(parametersPath); // throws ¯\_(ツ)_/¯ 
            var parameters = JsonConvert.DeserializeObject<Rectangle[]>(parametersString);
            
            System.IO.Directory.CreateDirectory(args[1]);
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
                var resultPath = $"{args[1]}\\{GetSimpleFilename(parameter)}.png";
                croppedImage.SaveAsPng(resultPath);
            }
            texture.SaveAsPng($"{args[0]}_sliced.png");
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
