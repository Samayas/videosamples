using Samayas.Tools.MIBExtractor;
using Samayas.Tools.MIBExtractor.Models;

namespace MIBExtractor
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Processing MIB Extractor");
            string[] mibLines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "MIBs\\mikrotik.mib");
            MIBParser mibParser = new MIBParser(mibLines);

            // Read MIB
            mibParser = new MIBParser(AppDomain.CurrentDomain.BaseDirectory + "MIBs\\mikrotik.mib");


            Console.WriteLine("List Warnings");
            foreach (string warning in mibParser.Warnings)
            {
                Console.WriteLine($"Warning: {warning}");
            }
            Console.WriteLine("");

            // Get Parsed Nodes
            IDictionary<string, MIBNode> nodes = mibParser.GetParsedNodes();

            // Textual Convention
            Dictionary<string, MIBNode> textualConventions = mibParser.GetTextualConventions();

            // Find OID
            MIBNode? foundOID = mibParser.GetNodeFromOID("1.3.6.1.4.1.14988.1.1.1.1.1.4");
            Console.Write($"Found OID: {foundOID.Name}");

            // Flat OIDs
            IList<string> oids = mibParser.GetFlatListOfResolvedOIDs();

            /// OID Definitions
            IList<MIBDefinition> oidDefinitions = mibParser.GetOIDDefinitions();

            // Print Tree
            using (StringWriter stringWriter = new StringWriter()) 
            {
                mibParser.PrintMIBTree(stringWriter);
                Console.Write(stringWriter.ToString());
            }

            // Print Flat OID
            using (StringWriter stringWriter = new StringWriter())
            {
                mibParser.PrintFlatListOfOIDs(stringWriter);
                Console.Write(stringWriter.ToString());
            }

            Console.ReadLine();
        }
    }
}