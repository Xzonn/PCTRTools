using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonCTR
{
    public class PatchHelper
    {
        public static bool PatchIt(string originalPath, string patchPath, string outputPath)
        {
            if (!CheckIfFileExists(originalPath)) return false;
            if (!CheckIfFileExists(patchPath)) return false;
            // 创建临时文件夹
            string tempPath = $"temp_{DateTime.Now.GetHashCode():X8}";
            while (Directory.Exists(tempPath) || File.Exists(tempPath))
            {
                tempPath = $"temp_{DateTime.Now.GetHashCode():X8}";
            }
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(tempPath);
                di.Attributes |= FileAttributes.Hidden;
                // 读取并写入ndstool
                Assembly asm = Assembly.GetExecutingAssembly();
                BufferedStream inStream = new BufferedStream(asm.GetManifestResourceStream("PokemonCTR.ndstool.exe"));
                FileStream outStream = new FileStream($@"{tempPath}\ndstool.exe", FileMode.Create, FileAccess.Write);
                byte[] buffer = new byte[inStream.Length];
                inStream.Read(buffer, 0, (int)inStream.Length);
                outStream.Write(buffer, 0, (int)inStream.Length);
                inStream.Close();
                outStream.Close();
                // 调用ndstool拆包
                Process ndstool = new Process();
                ndstool.StartInfo.FileName = $@"{Directory.GetCurrentDirectory()}\{tempPath}\ndstool.exe";
                ndstool.StartInfo.Arguments = "-x \"" + originalPath + "\" -9 arm9.bin -7 arm7.bin -y9 overarm9.bin -y7 overarm7.bin -d data -y overlay -t banner.bin -h header.bin";
                ndstool.StartInfo.WorkingDirectory = $@"{Directory.GetCurrentDirectory()}\{tempPath}";
                ndstool.StartInfo.CreateNoWindow = true;
                ndstool.StartInfo.UseShellExecute = false;
                ndstool.StartInfo.RedirectStandardOutput = true;
                ndstool.Start();
                ndstool.WaitForExit();
                // 解压补丁包
                FileStream archiveStream = File.OpenRead(patchPath);
                ZipArchive archive = new ZipArchive(archiveStream);
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = Path.GetFullPath(Path.Combine(tempPath, file.FullName));
                    if (file.Name == "")
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        continue;
                    }
                    else
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(completeFileName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        }
                        file.ExtractToFile(completeFileName, true);
                    }
                }
                archiveStream.Close();
                // 调用ndstool打包
                ndstool.StartInfo.Arguments = "-c temp.nds -9 arm9.bin -7 arm7.bin -y9 overarm9.bin -y7 overarm7.bin -d data -y overlay -t banner.bin -h header.bin";
                ndstool.Start();
                ndstool.WaitForExit();
                // 补FF
                long size = new FileInfo($@"{tempPath}\temp.nds").Length;
                long sizePad = PowerOfTwo(size);
                FileStream fsr = new FileStream($@"{tempPath}\temp.nds", FileMode.Append);
                while (fsr.Position < sizePad)
                {
                    fsr.WriteByte(0xff);
                }
                fsr.Close();
                File.Copy($@"{tempPath}\temp.nds", outputPath, true);
            }
            catch (Exception e)
            {
                MessageBox.Show($"错误：{e.Message}\n{e.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Directory.Delete(tempPath, true);
                return false;
            }
            // 删除临时文件夹
            Directory.Delete(tempPath, true);
            MessageBox.Show("已完成。", "完成");
            return true;
        }

        public static bool CheckIfFileExists(string filePath)
        {
            bool exists = File.Exists(filePath);
            if (!exists)
            {
                MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return exists;
        }

        private static long PowerOfTwo(long x)
        {
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return ++x;

            // 作者：骑猪逛街666
            // 链接：https://juejin.cn/post/6844903459112779784
            // 来源：掘金
            // 著作权归作者所有。商业转载请联系作者获得授权，非商业转载请注明出处。
        }
    }
}
