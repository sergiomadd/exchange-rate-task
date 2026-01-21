using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateUpdater.Application.Menu
{
    public static class ASCIIHelper
    {
        public static string Load(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("ASCII art file path cannot be null or empty.", nameof(filePath));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"ASCII art file not found: {filePath}");
                }

                return File.ReadAllText(filePath);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Unable to load ascii asset {filePath}");
            }

            return null;
        }

        public static void Print(string filePath)
        {
            Console.WriteLine(Load(filePath));
        }
    }
}
