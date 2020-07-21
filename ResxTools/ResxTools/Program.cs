using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ResxTools
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo rootFolderToCheck = new DirectoryInfo(@"C:\H\Ebriza");
            FileInfo resxFileToCheck = new FileInfo(@"C:\H\Ebriza\Unicolor\UC.ControlCentre.Web.Resources\Translation.resx");

            XDocument doc = XDocument.Parse(File.ReadAllText(resxFileToCheck.FullName));

            IEnumerable<XElement> dataNodes = doc.Descendants().Where(x => x.Name == "data");

            TranslationInfo[] translations
                = doc
                .Descendants()
                .Where(x => x.Name == "data")
                .Select(x =>
                    new TranslationInfo
                    {
                        Key = x.Attribute("name").Value,
                    }
                )
                .ToArray()
                ;

            FileInfo[] filesToCheck 
                = rootFolderToCheck
                .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                .Where(x => !x.Name.Contains(resxFileToCheck.Name, StringComparison.InvariantCultureIgnoreCase))
                .ToArray()
                ;



            Console.WriteLine($"Done @ {DateTime.Now}");
            Console.ReadLine();
        }
    }

    class TranslationInfo
    {
        public string Key { get; set; }

        public string Language { get; set; } = "Default/EN";
    }
}
