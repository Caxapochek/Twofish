using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medo.Security.Cryptography;

namespace ConsoleAppTwofish
{
    internal class Program
    {
        static void Main(string[] args)
        {
            byte a = 1;
            byte b = 0x02;
            Console.WriteLine(a==b);
            Console.ReadKey();
        }
    }
}
