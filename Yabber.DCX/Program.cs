using SoulsFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Yabber
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Console.WriteLine(
                    $"{assembly.GetName().Name} {assembly.GetName().Version}\n\n" +
                    "DSDCXRepacker 没有图形用户界面。\n" +
                    "将DCX拖放到exe上以对其进行解压缩，或将已解压缩的文件重新进行压缩。\n\n" +
                    "按任意键退出。"
                    );
                Console.ReadKey();
                return;
            }

            bool pause = false;

            foreach (string path in args)
            {
                try
                {
                    if (DCX.Is(path))
                    {
                        pause |= Decompress(path);
                    }
                    else
                    {
                        pause |= Compress(path);
                    }
                }
                catch (DllNotFoundException ex) when (ex.Message.Contains("oo2core_6_win64.dll"))
                {
                    Console.WriteLine("要解压缩游戏《只狼：影逝二度》的 .dcx 文件，你必须从游戏《只狼：影逝二度》复制文件 oo2core_6_win64.dll 到文件 DSDataRepacker.exe 所在目录中！");
                    pause = true;
                }
                catch (UnauthorizedAccessException)
                {
                    using (Process current = Process.GetCurrentProcess())
                    {
                        var admin = new Process();
                        admin.StartInfo = current.StartInfo;
                        admin.StartInfo.FileName = current.MainModule.FileName;
                        admin.StartInfo.Arguments = Environment.CommandLine.Replace($"\"{Environment.GetCommandLineArgs()[0]}\"", "");
                        admin.StartInfo.Verb = "runas";
                        admin.Start();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"未捕获异常：{ex}");
                    pause = true;
                }

                Console.WriteLine();
            }

            if (pause)
            {
                Console.WriteLine("遇到一个或多个错误并显示在上面。\n按任意键退出。");
                Console.ReadKey();
            }
        }

        private static bool Decompress(string sourceFile)
        {
            Console.WriteLine($"解压缩 DCX 文件：{Path.GetFileName(sourceFile)}……");

            string sourceDir = Path.GetDirectoryName(sourceFile);
            string outPath;
            if (sourceFile.EndsWith(".dcx"))
                outPath = $"{sourceDir}\\{Path.GetFileNameWithoutExtension(sourceFile)}";
            else
                outPath = $"{sourceFile}.undcx";

            byte[] bytes = DCX.Decompress(sourceFile, out DCX.Type compression);
            File.WriteAllBytes(outPath, bytes);

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            XmlWriter xw = XmlWriter.Create($"{outPath}-yabber-dcx.xml", xws);

            xw.WriteStartElement("dcx");
            xw.WriteElementString("compression", compression.ToString());
            xw.WriteEndElement();
            xw.Close();

            return false;
        }

        private static bool Compress(string path)
        {
            string xmlPath = $"{path}-yabber-dcx.xml";
            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"程序压缩文件所需的XML配置文件“{xmlPath}” 不存在！");
                return true;
            }

            Console.WriteLine($"压缩文件：{Path.GetFileName(path)}……");
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlPath);
            DCX.Type compression = (DCX.Type)Enum.Parse(typeof(DCX.Type), xml.SelectSingleNode("dcx/compression").InnerText);

            string outPath;
            if (path.EndsWith(".undcx"))
                outPath = path.Substring(0, path.Length - 6);
            else
                outPath = path + ".dcx";

            if (File.Exists(outPath) && !File.Exists(outPath + ".bak"))
                File.Move(outPath, outPath + ".bak");

            DCX.Compress(File.ReadAllBytes(path), compression, outPath);

            return false;
        }
    }
}
