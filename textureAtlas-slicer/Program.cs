using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json;

/// dotnet run -- ..\tex1_512x256_B20814E2D6573DFE_0.png ..\croplist.json

namespace csharp {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(string.Join(' ', args));
            if (args.Length < 2) {
                Console.WriteLine("Invalid arguments. Please specify both the texture .png file and the parameters .json file");
                Console.WriteLine("Example arguments: ..\\tex1_512x256_B20814E2D6573DFE_0.png ..\\croplist.json");
                return;
            }

            var texture = Image.Load(args[0]);
            var parametersString = File.ReadAllText(args[1]);
            var parameters = JsonConvert.DeserializeObject<Rectangle[]>(parametersString);
            
            System.IO.Directory.CreateDirectory("..\\output");
            foreach (var parameter in parameters) {
                Console.WriteLine(parameter.ToString());
                var croppedImage = texture.Clone(context => context.Crop(parameter));
                var resultPath = "..\\output\\" + GetSimpleFilename(parameter) + ".png";
                var emptyImage = new Image<Rgba32>(parameter.Width, parameter.Height);
                var location = new Point(parameter.X, parameter.Y);
                texture.Mutate(context => context.DrawImage(
                    emptyImage,
                    location,
                    SixLabors.ImageSharp.PixelFormats.PixelColorBlendingMode.Normal,
                    SixLabors.ImageSharp.PixelFormats.PixelAlphaCompositionMode.Clear,
                    1.0f));
                croppedImage.SaveAsPng(resultPath);
            }
            texture.SaveAsPng("..\\output\\slicedImage.png");
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
