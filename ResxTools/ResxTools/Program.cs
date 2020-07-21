using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResxTools
{
    class Program
    {
        static FileAndContent[] filesToCheck;

        static void Main(string[] args)
        {
            DirectoryInfo rootFolderToCheck = new DirectoryInfo(@"C:\H\Ebriza");

            filesToCheck
                = rootFolderToCheck
                .EnumerateFiles("*.cs", SearchOption.AllDirectories)
                .Concat(rootFolderToCheck.EnumerateFiles("*.cshtml", SearchOption.AllDirectories))
                .Concat(rootFolderToCheck.EnumerateFiles("*.rdlc", SearchOption.AllDirectories))
                .Where(x => !x.Name.Contains(".resx", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => !x.Name.Contains(".Designer.cs", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new FileAndContent(x))
                .ToArray()
                ;

            Task.WaitAll(
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\UC.ControlCentre.Web.Resources\Translation.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\UC.ControlCentre.Web.Resources.Translation.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\UC.ControlCentre.Web.Resources\Translation.ro.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\UC.ControlCentre.Web.Resources.Translation.ro.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\Ebriza.Client.Api\ErrorCodes.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\Ebriza.Client.Api.ErrorCodes.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\UC.Global\Translation.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\UC.Global.Translation.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\Ebriza.Engine.Report.Resources\Translation.ro.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\Ebriza.Engine.Report.Resources.Translation.ro.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\Ebriza.Engine.Report.Resources\Translation.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\Ebriza.Engine.Report.Resources.Translation.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\RaumFramework\Translations\Translation.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\RaumFramework.Translations.Translation.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\QueueManager\Translation.ro.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\QueueManager.Translation.ro.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\QueueManager\Translation.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\QueueManager.Translation.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\POSPrintInspector\Resources\Translations.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\POSPrintInspector.Resources.Translations.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\Ebriza.Engine.Subscription.Resources\Translation.ro.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\Ebriza.Engine.Subscription.Resources.Translation.ro.resx_refs.txt")
                    )
                )
                ,
                Task.Run(() => AnalyzeResxFile(
                    new FileInfo(@"C:\H\Ebriza\Unicolor\Ebriza.Engine.Subscription.Resources\Translation.resx"),
                    new FileInfo(@"C:\Users\hinte\Downloads\Ebriza.Engine.Subscription.Resources.Translation.resx_refs.txt")
                    )
                )
            )
            ;

            Console.WriteLine($"Done @ {DateTime.Now}");
            Console.ReadLine();
        }

        private static void AnalyzeResxFile(FileInfo resxFileToCheck, FileInfo outputFile)
        {
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

            Console.WriteLine($"Checking references @ {DateTime.Now} ...");
            int checkCount = 0;
            foreach (TranslationInfo translationInfo in translations)
            {
                foreach (FileAndContent file in filesToCheck)
                {
                    if (translationInfo.Key.Contains("_Enums_") || translationInfo.Key.Contains("_Enum_"))
                    {
                        translationInfo.IsReferenced = true;
                        break;
                    }

                    translationInfo.IsReferenced = file.Content.Contains(translationInfo.Key, StringComparison.InvariantCulture);
                    if (translationInfo.IsReferenced)
                        break;
                }
                checkCount++;
                Console.WriteLine($"{checkCount} / {translations.Length}");
            }
            Console.WriteLine();
            Console.WriteLine($"DONE Checking references @ {DateTime.Now}");

            PrintResults(resxFileToCheck, translations, outputFile);
        }

        private static void PrintResults(FileInfo resxFileToCheck, TranslationInfo[] results, FileInfo outputFile)
        {
            StringBuilder printer = new StringBuilder();
            printer.Append(resxFileToCheck.FullName);
            printer.AppendLine();
            foreach (TranslationInfo result in results.OrderBy(x => x.IsReferenced))
            {
                printer.AppendLine($"{(result.IsReferenced ? "[REFERENCED]" : "[NOT REFERENCED]")}\t{result.Key}");
            }

            File.WriteAllText(outputFile.FullName, printer.ToString());
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
