using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json;

namespace csharp {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(string.Join(' ', args));
            if (args.Length < 2) {
                Console.WriteLine("Invalid arguments. Please specify both the texture .png file and the parameters .json file");
                return;
            }

            var texture = Image.Load(args[0]);
            Console.WriteLine(texture.ToString());
            
            var parametersString = File.ReadAllText(args[1]);
            var parameters = JsonConvert.DeserializeObject<Rectangle[]>(parametersString);
            Console.WriteLine(parameters.Length);
            
            System.IO.Directory.CreateDirectory("output");
            foreach (var parameter in parameters) {
                Console.WriteLine(parameter.ToString());
                var croppedImage = texture.Clone(x => x.Crop(parameter));
                var resultPath = ".\\output\\" + GetHashString(parameter) + ".png";
                croppedImage.SaveAsPng(resultPath);
            }
            return;
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
