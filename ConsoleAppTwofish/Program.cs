using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTwofish
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AlgorithmTwofish algorithmTwofish = new AlgorithmTwofish();

            byte[] plaintext = Encoding.UTF8.GetBytes("Hello, VVorld!!! Its me.");
            byte[] key = new byte[] { 101, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 116 };

            algorithmTwofish.SetKey(128, key);

            byte[] ciphertext = algorithmTwofish.Encrypt(plaintext);

            Console.WriteLine("ciphertext: " + Encoding.UTF8.GetString(ciphertext));

            byte[] pltext = algorithmTwofish.Decrypt(ciphertext);

            Console.WriteLine("deciphertext: " + Encoding.UTF8.GetString(pltext));

            Console.ReadKey();
        }
    }
}