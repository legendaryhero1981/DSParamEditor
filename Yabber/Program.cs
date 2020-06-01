using SoulsFormats;
using SoulsFormats.AC4;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
                    "DSDataRepacker 没有图形用户界面。\n" +
                    "将文件拖放到exe上以将其解包，或将已解包的文件夹重新打包。\n\n" +
                    "DCX文件将被透明地解压缩和重新压缩；\n" +
                    "如果需要解压缩或重新压缩不支持的格式，请改用 DSDCXRepacker。\n\n" +
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
                    if (Directory.Exists(path))
                    {
                        pause |= RepackDir(path);
                    }
                    else if (File.Exists(path))
                    {
                        pause |= UnpackFile(path);
                    }
                    else
                    {
                        Console.WriteLine($"文件或目录“{path}”不存在！");
                        pause = true;
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
                catch (FriendlyException ex)
                {
                    Console.WriteLine($"错误信息：{ex.Message}");
                    pause = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"未捕获异常：{ex}");
                    pause = true;
                }

                Console.WriteLine();
            }

            if (!pause) return;
            Console.WriteLine("遇到一个或多个错误并显示在上面。\n按任意键退出。");
            Console.ReadKey();
        }

        private static bool UnpackFile(string sourceFile)
        {
            string sourceDir = Path.GetDirectoryName(sourceFile);
            string filename = Path.GetFileName(sourceFile);
            string targetDir = $"{sourceDir}\\{filename.Replace('.', '-')}";
            if (File.Exists(targetDir))
                targetDir += "-yabber";

            if (DCX.Is(sourceFile))
            {
                Console.WriteLine($"解压缩 DCX 文件：{filename}……");
                byte[] bytes = DCX.Decompress(sourceFile, out DCX.Type compression);
                if (BND3.Is(bytes))
                {
                    Console.WriteLine($"解包 BND3 文件：{filename}……");
                    BND3 bnd = BND3.Read(bytes);
                    bnd.Compression = compression;
                    bnd.Unpack(filename, targetDir);
                }
                else if (BND4.Is(bytes))
                {
                    Console.WriteLine($"解包 BND4 文件：{filename}……");
                    BND4 bnd = BND4.Read(bytes);
                    bnd.Compression = compression;
                    bnd.Unpack(filename, targetDir);
                }
                else if (sourceFile.EndsWith(".fmg.dcx"))
                {
                    Console.WriteLine($"解包 FMG 文件：{filename}……");
                    FMG fmg = FMG.Read(bytes);
                    fmg.Compression = compression;
                    fmg.Unpack(sourceFile);
                }
                else if (GPARAM.Is(bytes))
                {
                    Console.WriteLine($"解包 GPARAM 文件：{filename}……");
                    GPARAM gparam = GPARAM.Read(bytes);
                    gparam.Compression = compression;
                    gparam.Unpack(sourceFile);
                }
                else if (TPF.Is(bytes))
                {
                    Console.WriteLine($"解包 TPF 文件：{filename}……");
                    TPF tpf = TPF.Read(bytes);
                    tpf.Compression = compression;
                    tpf.Unpack(filename, targetDir);
                }
                else
                {
                    Console.WriteLine($"不能识别格式文件“{filename}”的格式！");
                    return true;
                }
            }
            else
            {
                if (BND3.Is(sourceFile))
                {
                    Console.WriteLine($"解包 BND3 文件： {filename}……");
                    BND3 bnd = BND3.Read(sourceFile);
                    bnd.Unpack(filename, targetDir);
                }
                else if (BND4.Is(sourceFile))
                {
                    Console.WriteLine($"解包 BND4 文件：{filename}……");
                    BND4 bnd = BND4.Read(sourceFile);
                    bnd.Unpack(filename, targetDir);
                }
                else if (BXF3.IsBHD(sourceFile))
                {
                    string bdtExtension = Path.GetExtension(filename).Replace("bhd", "bdt");
                    string bdtFilename = $"{Path.GetFileNameWithoutExtension(filename)}{bdtExtension}";
                    string bdtPath = $"{sourceDir}\\{bdtFilename}";
                    if (File.Exists(bdtPath))
                    {
                        Console.WriteLine($"解包 BXF3 文件：{filename}……");
                        BXF3 bxf = BXF3.Read(sourceFile, bdtPath);
                        bxf.Unpack(filename, bdtFilename, targetDir);
                    }
                    else
                    {
                        Console.WriteLine($"未找到与 BHD 文件相对应的 BDT 文件：{filename}");
                        return true;
                    }
                }
                else if (BXF4.IsBHD(sourceFile))
                {
                    string bdtExtension = Path.GetExtension(filename).Replace("bhd", "bdt");
                    string bdtFilename = $"{Path.GetFileNameWithoutExtension(filename)}{bdtExtension}";
                    string bdtPath = $"{sourceDir}\\{bdtFilename}";
                    if (File.Exists(bdtPath))
                    {
                        Console.WriteLine($"解包 BXF4 文件：{filename}……");
                        BXF4 bxf = BXF4.Read(sourceFile, bdtPath);
                        bxf.Unpack(filename, bdtFilename, targetDir);
                    }
                    else
                    {
                        Console.WriteLine($"未找到与 BHD 文件相对应的 BDT 文件：{filename}");
                        return true;
                    }
                }
                else if (sourceFile.EndsWith(".fmg"))
                {
                    Console.WriteLine($"解包 FMG 文件：{filename}……");
                    FMG fmg = FMG.Read(sourceFile);
                    fmg.Unpack(sourceFile);
                }
                else if (sourceFile.EndsWith(".fmg.xml") || sourceFile.EndsWith(".fmg.dcx.xml"))
                {
                    Console.WriteLine($"重打包 FMG 文件：{filename}……");
                    YFMG.Repack(sourceFile);
                }
                else if (GPARAM.Is(sourceFile))
                {
                    Console.WriteLine($"解包 GPARAM 文件：{filename}……");
                    GPARAM gparam = GPARAM.Read(sourceFile);
                    gparam.Unpack(sourceFile);
                }
                else if (sourceFile.EndsWith(".gparam.xml") || sourceFile.EndsWith(".gparam.dcx.xml")
                    || sourceFile.EndsWith(".fltparam.xml") || sourceFile.EndsWith(".fltparam.dcx.xml"))
                {
                    Console.WriteLine($"重打包 GPARAM 文件：{filename}……");
                    YGPARAM.Repack(sourceFile);
                }
                else if (sourceFile.EndsWith(".luagnl"))
                {
                    Console.WriteLine($"解包 LUAGNL 文件：{filename}……");
                    LUAGNL gnl = LUAGNL.Read(sourceFile);
                    gnl.Unpack(sourceFile);
                }
                else if (sourceFile.EndsWith(".luagnl.xml"))
                {
                    Console.WriteLine($"重打包 LUAGNL 文件：{filename}……");
                    YLUAGNL.Repack(sourceFile);
                }
                else if (LUAINFO.Is(sourceFile))
                {
                    Console.WriteLine($"解包 LUAINFO 文件：{filename}……");
                    LUAINFO info = LUAINFO.Read(sourceFile);
                    info.Unpack(sourceFile);
                }
                else if (sourceFile.EndsWith(".luainfo.xml"))
                {
                    Console.WriteLine($"重打包 LUAINFO 文件：{filename}……");
                    YLUAINFO.Repack(sourceFile);
                }
                else if (TPF.Is(sourceFile))
                {
                    Console.WriteLine($"解包 TPF 文件：{filename}……");
                    TPF tpf = TPF.Read(sourceFile);
                    tpf.Unpack(filename, targetDir);
                }
                else if (Zero3.Is(sourceFile))
                {
                    Console.WriteLine($"解包 000 文件：{filename}……");
                    Zero3 z3 = Zero3.Read(sourceFile);
                    z3.Unpack(targetDir);
                }
                else
                {
                    Console.WriteLine($"不能识别格式文件“{filename}”的格式！");
                    return true;
                }
            }
            return false;
        }

        private static bool RepackDir(string sourceDir)
        {
            string sourceName = new DirectoryInfo(sourceDir).Name;
            string targetDir = new DirectoryInfo(sourceDir).Parent.FullName;
            if (File.Exists($"{sourceDir}\\_yabber-bnd3.xml"))
            {
                Console.WriteLine($"重打包 BND3 文件：{sourceName}……");
                YBND3.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-bnd4.xml"))
            {
                Console.WriteLine($"重打包 BND4 文件：{sourceName}……");
                YBND4.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-bxf3.xml"))
            {
                Console.WriteLine($"重打包 BXF3 文件：{sourceName}……");
                YBXF3.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-bxf4.xml"))
            {
                Console.WriteLine($"重打包 BXF4 文件：{sourceName}……");
                YBXF4.Repack(sourceDir, targetDir);
            }
            else if (File.Exists($"{sourceDir}\\_yabber-tpf.xml"))
            {
                Console.WriteLine($"重打包 TPF 文件：{sourceName}……");
                YTPF.Repack(sourceDir, targetDir);
            }
            else
            {
                Console.WriteLine($"程序重打包文件所需的XML配置文件“{sourceName}” 不存在！");
                return true;
            }
            return false;
        }
    }
}
