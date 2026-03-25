using Samayas.Tools.MIBExtractor;
using Samayas.Tools.MIBExtractor.Interfaces;
using Samayas.Tools.MIBExtractor.Models;
using System.Reflection;

namespace MIBDetails
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            exeDir = $"{exeDir}\\..\\..\\..\\MIBs\\";

            IMIBParser parser = new MIBParser($"{exeDir}ADSL-LINE.MIB");

            // Get all parsed nodes
            IDictionary<string, MIBNode> nodes = parser.GetParsedNodes();
        
            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);

            parser = new MIBParser($"{exeDir}ADSL-TC.MIB");

            // Get all parsed nodes
            nodes = parser.GetParsedNodes();

            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);

            parser = new MIBParser($"{exeDir}RFC1213.MIB");

            // Get all parsed nodes
            nodes = parser.GetParsedNodes();

            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);

            parser = new MIBParser($"{exeDir}VDSL2-LINE.MIB");

            // Get all parsed nodes
            nodes = parser.GetParsedNodes();

            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);

            parser = new MIBParser($"{exeDir}VDSL2-LINE-TC.MIB");

            // Get all parsed nodes
            nodes = parser.GetParsedNodes();

            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);

            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);

            parser = new MIBParser($"{exeDir}VDSL-LINE.MIB");

            // Get all parsed nodes
            nodes = parser.GetParsedNodes();

            // Export hierarchical tree
            parser.PrintMIBTree(Console.Out);
        }
    }
}
