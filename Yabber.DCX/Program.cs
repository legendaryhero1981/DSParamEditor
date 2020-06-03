using SoulsFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Timer = System.Timers.Timer;

namespace Yabber
{
    internal class Program
    {
        #region 模拟自动键盘输入事件
        private static readonly IntPtr ConsoleWindowHnd = GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        private const int VkReturn = 0x0D;
        private const int WmKeydown = 0x100;
        #endregion

        private const string ExeName = "DSDCXRepacker";
        private const string DllOo2Core = "oo2core_6_win64.dll";
        private const string SuffixXml = "-yabber-dcx.xml";
        private const string SuffixDcx = ".dcx";
        private const string SuffixUnDcx = ".undcx";
        private const string SuffixBak = ".bak";

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var assembly = Assembly.GetExecutingAssembly();
                Console.WriteLine(
                    $"{assembly.GetName().Name} {assembly.GetName().Version}\n\n" +
                    $"{ExeName} 没有图形用户界面。\n" +
                    "将DCX拖放到exe上以对其进行解压缩，或将已解压缩的文件重新进行压缩。\n\n" +
                    "按任意键退出。"
                    );
                Console.ReadKey();
                return;
            }

            var pause = false;

            foreach (var path in args)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine($"略过目录“{path}”");
                        continue;
                    }
                    if (DCX.Is(path)) pause |= Decompress(path);
                    else pause |= Compress(path);
                }
                catch (DllNotFoundException ex) when (ex.Message.Contains(DllOo2Core))
                {
                    Console.WriteLine($"要解压缩游戏《只狼：影逝二度》的 .dcx 文件，你必须从游戏《只狼：影逝二度》复制文件 {DllOo2Core}.dll 到文件 {ExeName}.exe 所在目录中！");
                    pause = true;
                }
                catch (UnauthorizedAccessException)
                {
                    using var current = Process.GetCurrentProcess();
                    var admin = new Process { StartInfo = current.StartInfo };
                    admin.StartInfo.FileName = current.MainModule?.FileName ?? string.Empty;
                    admin.StartInfo.Arguments = Environment.CommandLine.Replace($"\"{Environment.GetCommandLineArgs()[0]}\"", "");
                    admin.StartInfo.Verb = "runas";
                    admin.Start();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"未捕获异常：{ex}");
                    pause = true;
                }
            }

            if (!pause) return;
            var timer = new Timer { Interval = 1000, AutoReset = false };
            timer.Elapsed += (source, e) =>
            {
                PostMessage(ConsoleWindowHnd, WmKeydown, VkReturn, 0);
            };
            timer.Start();
            Console.ReadKey();
        }

        private static bool Decompress(string sourceFile)
        {
            Console.WriteLine($"解压缩 DCX 文件：{Path.GetFileName(sourceFile)}");

            var sourceDir = Path.GetDirectoryName(sourceFile);
            var outPath = sourceFile.EndsWith(SuffixDcx) ? @$"{sourceDir}\{Path.GetFileNameWithoutExtension(sourceFile)}" : $"{sourceFile}{SuffixUnDcx}";

            var bytes = DCX.Decompress(sourceFile, out var compression);
            File.WriteAllBytes(outPath, bytes);

            var xws = new XmlWriterSettings { Indent = true };
            var xw = XmlWriter.Create($"{outPath}{SuffixXml}", xws);

            xw.WriteStartElement("dcx");
            xw.WriteElementString("compression", compression.ToString());
            xw.WriteEndElement();
            xw.Close();

            return false;
        }

        private static bool Compress(string path)
        {
            var xmlPath = $"{path}{SuffixXml}";
            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"程序压缩文件所需的XML配置文件“{xmlPath}” 不存在！");
                return true;
            }

            Console.WriteLine($"压缩文件：{Path.GetFileName(path)}");
            var xml = new XmlDocument();
            xml.Load(xmlPath);
            var compression = (DCX.Type)Enum.Parse(typeof(DCX.Type), xml.SelectSingleNode("dcx/compression")?.InnerText ?? string.Empty);

            string outPath;
            if (path.EndsWith(SuffixUnDcx))
                outPath = path.Substring(0, path.Length - SuffixUnDcx.Length);
            else
                outPath = path + SuffixDcx;

            if (File.Exists(outPath) && !File.Exists(outPath + SuffixBak))
                File.Move(outPath, outPath + SuffixBak);

            DCX.Compress(File.ReadAllBytes(path), compression, outPath);

            return false;
        }
    }
}
