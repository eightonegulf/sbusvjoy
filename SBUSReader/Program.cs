using System;
using System.IO;

namespace SBUSReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SBUSReader r = new SBUSReader(File.ReadAllLines("port.txt")[0]);
            Console.ReadLine();
        }
    }
}
