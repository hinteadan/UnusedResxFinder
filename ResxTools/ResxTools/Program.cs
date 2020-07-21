using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ResxTools
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo rootFolderToCheck = new DirectoryInfo(@"C:\H\Ebriza");
            FileInfo resxFileToCheck = new FileInfo(@"C:\H\Ebriza\Unicolor\UC.ControlCentre.Web.Resources\Translation.resx");
            DirectoryInfo outputFolder = new DirectoryInfo(@"C:\Users\hinte\Downloads");

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

            FileAndContent[] filesToCheck
                = rootFolderToCheck
                .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                .Where(x => !x.Name.Contains(resxFileToCheck.Name, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new FileAndContent(x))
                .ToArray()
                ;

            Console.WriteLine($"Checking references @ {DateTime.Now} ...");
            foreach (TranslationInfo translationInfo in translations)
            {
                foreach (FileAndContent file in filesToCheck)
                {
                    translationInfo.IsReferenced = file.Content.Contains(translationInfo.Key, StringComparison.InvariantCulture);
                    if (translationInfo.IsReferenced)
                        break;

                    Console.Write('.');
                }
            }
            Console.WriteLine();
            Console.WriteLine($"DONE Checking references @ {DateTime.Now}");

            PrintResults(resxFileToCheck, translations, outputFolder);

            Console.WriteLine($"Done @ {DateTime.Now}");
            Console.ReadLine();
        }

        private static void PrintResults(FileInfo resxFileToCheck, TranslationInfo[] results, DirectoryInfo outputFolder)
        {
            StringBuilder printer = new StringBuilder();
            printer.Append(resxFileToCheck.FullName);
            printer.AppendLine();
            foreach (TranslationInfo result in results.OrderBy(x => x.IsReferenced))
            {
                printer.AppendLine($"{(result.IsReferenced ? "[REFERENCED]" : "[NOT REFERENCED]")}\t{result.Key}");
            }

            FileInfo resultFile = new FileInfo(Path.Combine(outputFolder.FullName, $"resx_refs_{DateTime.Now.ToString("yyyyMMddHHmmss")}"));

            File.WriteAllText(resultFile.FullName, printer.ToString());
        }
    }

    class FileAndContent
    {
        readonly Lazy<string> lazyFileContent;
        public FileAndContent(FileInfo file)
        {
            File = file;
            lazyFileContent = new Lazy<string>(() => System.IO.File.ReadAllText(File.FullName));
        }

        public FileInfo File { get; }
        public string Content => lazyFileContent.Value;
    }

    class TranslationInfo
    {
        public string Key { get; set; }

        public string Language { get; set; } = "Default/EN";

        public bool IsReferenced { get; set; } = false;
    }
}
