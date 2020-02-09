using SoulsFormats;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Cell = SoulsFormats.PARAM.Cell;

namespace Yapped
{
    internal static class Util
    {
        public static object GetValueByNameExpr(object obj, string expr)
        {
            foreach (var s in expr.Split('.'))
            {
                if (null == obj) break;
                var p = obj.GetType().GetProperty(s);
                if (null == p) break;
                obj = p.GetValue(obj);
            }
            return obj;
        }

        public static Dictionary<string, PARAM.Layout> LoadLayouts(string directory)
        {
            var layouts = new Dictionary<string, PARAM.Layout>();
            if (Directory.Exists(directory))
            {
                foreach (var path in Directory.GetFiles(directory, "*.xml"))
                {
                    var paramID = Path.GetFileNameWithoutExtension(path);
                    try
                    {
                        var layout = PARAM.Layout.ReadXMLFile(path);
                        layouts[paramID] = layout;
                    }
                    catch (Exception ex)
                    {
                        ShowError($"加载配置文件 {paramID}.xml 失败！\r\n\r\n{ex}");
                    }
                }
            }
            return layouts;
        }

        public static LoadParamsResult LoadParams(string paramPath, Dictionary<string, ParamInfo> paramInfo,
            Dictionary<string, PARAM.Layout> layouts, Dictionary<BinderFile, ParamWrapper> fileWrapperCache, GameMode gameMode, bool hideUnusedParams)
        {
            if (!File.Exists(paramPath))
            {
                ShowError($"Parambnd类型文件 {paramPath} 不存在！\r\n请选定要给要编辑的Data0.bdt文件或Parambnd类型文件。");
                return null;
            }

            var result = new LoadParamsResult();
            try
            {
                if (BND4.Is(paramPath))
                {
                    result.ParamBND = BND4.Read(paramPath);
                    result.Encrypted = false;
                }
                else if (BND3.Is(paramPath))
                {
                    result.ParamBND = BND3.Read(paramPath);
                    result.Encrypted = false;
                }
                else if (gameMode.Game == GameMode.GameType.DarkSouls2)
                {
                    result.ParamBND = DecryptDS2Regulation(paramPath);
                    result.Encrypted = true;
                }
                else if (gameMode.Game == GameMode.GameType.DarkSouls3)
                {
                    result.ParamBND = SFUtil.DecryptDS3Regulation(paramPath);
                    result.Encrypted = true;
                }
                else
                    throw new FormatException("无法识别文件的数据格式！");
            }
            catch (DllNotFoundException ex) when (ex.Message.Contains("oo2core_6_win64.dll"))
            {
                ShowError("为了加载Sekiro参数，必须将文件oo2core_6_win64.dll从Sekiro复制到文件DSParamEditor.exe的同一目录中。");
                return null;
            }
            catch (Exception ex)
            {
                ShowError($"加载Parambnd类型文件失败！\r\n{paramPath}\r\n\r\n{ex}");
                return null;
            }

            fileWrapperCache.Clear();
            result.ParamWrappers = new List<ParamWrapper>();
            foreach (var file in result.ParamBND.Files.Where(f => f.Name.EndsWith(".param")))
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                if (paramInfo.ContainsKey(name))
                {
                    if (paramInfo[name].Blocked || paramInfo[name].Hidden && hideUnusedParams)
                        continue;
                }

                try
                {
                    var param = PARAM.Read(file.Bytes);
                    PARAM.Layout layout = null;
                    if (layouts.ContainsKey(param.ParamType))
                        layout = layouts[param.ParamType];

                    string description = null;
                    if (paramInfo.ContainsKey(name))
                        description = paramInfo[name].Description;

                    var wrapper = new ParamWrapper(name, param, layout, description);
                    result.ParamWrappers.Add(wrapper);
                    fileWrapperCache[file] = wrapper;
                }
                catch (Exception ex)
                {
                    ShowError($"加载参数文件：{name}.param失败！\r\n\r\n{ex}");
                }
            }

            result.ParamWrappers.Sort();
            return result;
        }

        private static byte[] ds2RegulationKey = { 0x40, 0x17, 0x81, 0x30, 0xDF, 0x0A, 0x94, 0x54, 0x33, 0x09, 0xE1, 0x71, 0xEC, 0xBF, 0x25, 0x4C };

        public static BND4 DecryptDS2Regulation(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var iv = new byte[16];
            iv[0] = 0x80;
            Array.Copy(bytes, 0, iv, 1, 11);
            iv[15] = 1;
            var input = new byte[bytes.Length - 32];
            Array.Copy(bytes, 32, input, 0, bytes.Length - 32);
            using (var ms = new MemoryStream(input))
            {
                var decrypted = CryptographyUtility.DecryptAesCtr(ms, ds2RegulationKey, iv);
                return BND4.Read(decrypted);
            }
        }

        public static void EncryptDS2Regulation(string path, BND4 bnd)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            bnd.Write(path);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal static class ResourceUtil
    {
        #region 内存回收
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="size">进程的当前内存占用值。</param>
        /// <returns></returns>
        public static void ClearMemory(long size)
        {
            //获得当前工作进程
            var proc = Process.GetCurrentProcess();
            var usedMemory = proc.PrivateMemorySize64;
            if (usedMemory < size) return;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion
    }

    internal static class ParamUtil
    {
        public static void CastCellValue(Cell cell, object value)
        {
            try
            {
                cell.Value = 1 == cell.Def.BitSize ? Convert.ChangeType(Convert.ToBoolean(value) ? 1 : 0, cell.Value.GetType()) : Convert.ChangeType(value, cell.Value.GetType());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }

    internal static class FileEncodingUtil
    {
        /// <summary>
        /// 取得一个文本文件的编码方式。如果无法在文件头部找到有效的前导符，Encoding.Default将被返回。
        /// </summary>
        /// <param name="path">文件路径名。</param>
        /// <returns></returns>
        public static Encoding GetEncoding(string path)
        {
            return GetEncoding(path, Encoding.Default);
        }
        /// <summary>
        /// 取得一个文本文件的编码方式。
        /// </summary>
        /// <param name="path">文件路径名。</param>
        /// <param name="encoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
        /// <returns></returns>
        public static Encoding GetEncoding(string path, Encoding encoding)
        {
            var fs = new FileStream(path, FileMode.Open);
            var targetEncoding = GetEncoding(fs, encoding);
            fs.Close();
            return targetEncoding;
        }
        /// <summary>
        /// 取得一个文本文件流的编码方式。
        /// </summary>
        /// <param name="stream">文本文件流。</param>
        /// <param name="encoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
        /// <returns></returns>
        public static Encoding GetEncoding(FileStream stream, Encoding encoding)
        {
            var result = encoding;
            var reader = new BinaryReader(stream, Encoding.Default);
            var bytes = reader.ReadBytes(3);
            if (2 > bytes.Length)
                return result;
            if (bytes[0] == 0xFE && bytes[1] == 0xFF)
                result = Encoding.BigEndianUnicode;
            else if (bytes[0] == 0xFF && bytes[1] == 0xFE)
                result = Encoding.Unicode;
            else if (2 < bytes.Length && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                result = Encoding.UTF8;
            else
            {
                int.TryParse(stream.Length.ToString(), out var i);
                bytes = reader.ReadBytes(i);
                if (IsUtf8Bytes(bytes))
                    result = Encoding.UTF8;
            }
            reader.Close();
            return result;
        }
        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static bool IsUtf8Bytes(IEnumerable<byte> bytes)
        {
            var charByteCounter = 1;　 //计算当前正分析的字符应还有的字节数
            foreach (var b in bytes)
            {
                var curByte = b; //当前分析的字节.
                if (charByteCounter == 1)
                {
                    if (curByte < 0x80) continue;
                    //判断当前
                    while (((curByte <<= 1) & 0x80) != 0)
                        charByteCounter++;
                    //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                    if (charByteCounter == 1 || charByteCounter > 6)
                        return false;
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                        return false;
                    charByteCounter--;
                }
            }
            return charByteCounter <= 1;
        }
    }

    internal class LoadParamsResult
    {
        public bool Encrypted { get; set; }

        public IBinder ParamBND { get; set; }

        public List<ParamWrapper> ParamWrappers { get; set; }
    }
}
