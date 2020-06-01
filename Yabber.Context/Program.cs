using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace Yabber.Context
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Console.Write(
                $"{assembly.GetName().Name} {assembly.GetName().Version}\n\n" +
                "此程序将注册程序 DSDataRepacker.exe 和 DSDCXRepacker.exe\n" +
                "这样就可以通过右键单击文件或文件夹来运行它们。\n" +
                "按R键注册，按U键反注册，或按其他键退出。\n" +
                "> ");
            string choice = Console.ReadLine().ToUpper();
            Console.WriteLine();

            if (choice == "R" || choice == "U")
            {
                try
                {
                    RegistryKey classes = Registry.CurrentUser.OpenSubKey("Software\\Classes", true);
                    if (choice == "R")
                    {
                        string yabberPath = Path.GetFullPath("DSDataRepacker.exe");
                        RegistryKey yabberFileKey = classes.CreateSubKey("*\\shell\\DSDataRepacker");
                        RegistryKey yabberFileCommand = yabberFileKey.CreateSubKey("command");
                        yabberFileKey.SetValue(null, "DSDataRepacker");
                        yabberFileCommand.SetValue(null, $"\"{yabberPath}\" \"%1\"");
                        RegistryKey yabberDirKey = classes.CreateSubKey("directory\\shell\\DSDataRepacker");
                        RegistryKey yabberDirCommand = yabberDirKey.CreateSubKey("command");
                        yabberDirKey.SetValue(null, "DSDataRepacker");
                        yabberDirCommand.SetValue(null, $"\"{yabberPath}\" \"%1\"");

                        string dcxPath = Path.GetFullPath("DSDCXRepacker.exe");
                        RegistryKey dcxFileKey = classes.CreateSubKey("*\\shell\\DSDCXRepacker");
                        RegistryKey dcxFileCommand = dcxFileKey.CreateSubKey("command");
                        dcxFileKey.SetValue(null, "DSDCXRepacker");
                        dcxFileCommand.SetValue(null, $"\"{dcxPath}\" \"%1\"");

                        Console.WriteLine("程序注册成功！");
                    }
                    else if (choice == "U")
                    {
                        classes.DeleteSubKeyTree("*\\shell\\DSDataRepacker", false);
                        classes.DeleteSubKeyTree("directory\\shell\\DSDataRepacker", false);
                        classes.DeleteSubKeyTree("*\\shell\\DSDCXRepacker", false);
                        Console.WriteLine("程序反注册成功！");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"操作失败；请使用系统管理员身份运行。错误信息：\n{ex}");
                }

                Console.WriteLine("按任意键退出。");
                Console.ReadKey();
            }
        }
    }
}
