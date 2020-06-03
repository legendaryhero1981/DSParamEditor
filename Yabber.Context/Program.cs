using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace Yabber.Context
{
    internal class Program
    {
        private const string DataRepacker = "DSDataRepacker";
        private const string DcxRepacker = "DSDCXRepacker";

        private static void Main()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Console.Write(
                $"{assembly.GetName().Name} {assembly.GetName().Version}\n\n" +
                $"此程序将注册程序 {DataRepacker}.exe 和 {DcxRepacker}.exe\n" +
                "这样就可以通过右键单击文件或文件夹来运行它们。\n" +
                "按R键注册，按U键反注册，或按其他键退出（按回车键确定输入）。\n" +
                "> ");
            var choice = Console.ReadLine()?.ToUpper();
            Console.WriteLine();

            if (choice != "R" && choice != "U") return;
            try
            {
                var classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
                switch (choice)
                {
                    case "R":
                        {
                            var path = Path.GetFullPath($"{DataRepacker}.exe");
                            var fileKey = classes?.CreateSubKey(@$"*\shell\{DataRepacker}");
                            var fileCommand = fileKey?.CreateSubKey("command");
                            fileKey?.SetValue(null, DataRepacker);
                            fileCommand?.SetValue(null, $"\"{path}\" \"%1\"");
                            var dirKey = classes?.CreateSubKey(@$"directory\shell\{DataRepacker}");
                            var dirCommand = dirKey?.CreateSubKey("command");
                            dirKey?.SetValue(null, DataRepacker);
                            dirCommand?.SetValue(null, $"\"{path}\" \"%1\"");
                            var dcxPath = Path.GetFullPath($"{DcxRepacker}.exe");
                            var dcxFileKey = classes?.CreateSubKey(@$"*\shell\{DcxRepacker}");
                            var dcxFileCommand = dcxFileKey?.CreateSubKey("command");
                            dcxFileKey?.SetValue(null, DcxRepacker);
                            dcxFileCommand?.SetValue(null, $"\"{dcxPath}\" \"%1\"");
                            Console.WriteLine("程序注册成功！");
                            break;
                        }
                    case "U":
                        classes?.DeleteSubKeyTree(@$"*\shell\{DataRepacker}", false);
                        classes?.DeleteSubKeyTree(@$"directory\shell\{DataRepacker}", false);
                        classes?.DeleteSubKeyTree(@$"*\shell\{DcxRepacker}", false);
                        Console.WriteLine("程序反注册成功！");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作失败，请使用系统管理员身份运行！错误信息：\n{ex}");
            }

            Console.WriteLine("按任意键退出……");
            Console.ReadKey();
        }
    }
}
