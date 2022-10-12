using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTwofish
{
    internal class Program
    {
        static void Main()
        {
            string file = @"D:\test2\install.exe.bin";

            string outFile = Path.Combine(@"D:\test2\", Path.GetFileName(file) + ".bin");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Crypt(file, outFile);
            stopwatch.Stop();
            long totalBytes = new FileInfo(file).Length;
            Console.WriteLine("Шифрованние файла: " + file);
            Console.WriteLine("Размер файла: " + totalBytes + " байт");
            Console.WriteLine("Затрачено времени: " + stopwatch.ElapsedMilliseconds + " миллисекунд или " + stopwatch.ElapsedTicks + " тактов таймера");
            Console.WriteLine("Скорость шифрования: " + 10000 * totalBytes / stopwatch.ElapsedTicks + " КБайт/с");
            Console.ReadKey();
        }
        static private void Crypt(string file, string outFile)
        {
            AlgorithmTwofish algorithmTwofish = new AlgorithmTwofish();
            byte[] key = new byte[] { 84, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 116 };
            algorithmTwofish.SetKey(128, key);

            using (var outFs = new FileStream(outFile, FileMode.Create)) 
            {
                int currBytes = 0;

                int blockSizeBytes = 100000;
                byte[] data = new byte[blockSizeBytes];

                long totalBytes = new FileInfo(file).Length;                   

                using (var inFs = new FileStream(file, FileMode.Open)) 
                {
                    while ((currBytes = inFs.Read(data, 0, blockSizeBytes)) > 0) 
                    {
                        outFs.Write(algorithmTwofish.Encrypt(data), 0, currBytes); 
                    }
                }
                data = new byte[1];
            }

            
        }
    }
}