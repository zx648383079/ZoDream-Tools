using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace TxtToDpl
{
    public static class Helper
    {
        const string HEADER_TAG = "DAUMPLAYLIST";

        const string FILE_TAG = "*file*";
        const string TITLE_TAG = "*title*";

        const string NEW_LINE = "\n";

        public static readonly string[] Headers = new string[3]{
            "playname=", // 当前播放的网址
            "topindex=0", // 当前播放的序号
            "saveplaypos=0"
        };

        /// <summary>
        /// 转换文件
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="source"></param>
        public static void Converter(string dist, string source)
        {
            using (var sourceStream = new FileStream(source, FileMode.Open))
            {
                using (var distStream = new FileStream(dist, FileMode.Create))
                {
                    Converter(distStream, sourceStream);
                }
            }
        }

        /// <summary>
        /// 转换文件
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="source"></param>
        public static void Converter(FileStream dist, FileStream source)
        {
            Reset(source);
            var encoder = new TxtEncoder();
            var encoding = encoder.GetEncoding(source);
            source.Seek(encoder.Position, SeekOrigin.Begin);
            var line = ReadLine(source, encoding);
            if (line.Trim() == HEADER_TAG)
            {
                Reset(source);
                Copy(dist, source);
                return;
            }
            source.Seek(encoder.Position, SeekOrigin.Begin);
            Write(dist, HEADER_TAG);
            Write(dist, NEW_LINE);
            foreach (var item in Headers)
            {
                Write(dist, item);
                Write(dist, NEW_LINE);
            }
            var i = 1;
            while (null != (line = ReadLine(source, encoding)))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var args = line.Split(',');
                if (args.Length < 2)
                {
                    continue;
                }
                Write(dist, i.ToString());
                Write(dist, FILE_TAG);
                Write(dist, args[1]);
                Write(dist, NEW_LINE);
                Write(dist, i.ToString());
                Write(dist, TITLE_TAG);
                Write(dist, args[0]);
                Write(dist, NEW_LINE);
                i++;
            }
        }

        /// <summary>
        /// 移动指针到开始位置
        /// </summary>
        /// <param name="source"></param>
        private static void Reset(Stream source)
        {
            source.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// 复制流
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="source"></param>
        private static void Copy(FileStream dist, FileStream source)
        {
            source.CopyTo(dist);
        }

        /// <summary>
        /// 读取一行
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string ReadLine(FileStream source, Encoding encoding)
        {
            var bytes = new List<byte>();
            int code;
            bool hasByte = false;
            while ((code = source.ReadByte()) != -1)
            {
                hasByte = true;
                if (code == 0x0a/* \n */ || code == 0x0d /* \r */)
                {
                    break;
                }
                bytes.Add(Convert.ToByte(code));
            }
            if (!hasByte)
            {
                return null;
            }
            if (bytes.Count  < 1)
            {
                return "";
            }
            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// 写入字符
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="line"></param>
        private static void Write(FileStream dist, string line)
        {
            var bytes = Encoding.UTF8.GetBytes(line);
            dist.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 选择保存文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string ChooseSaveFile(string name = "", string filter = "文本文件|*.txt|所有文件|*.*")
        {
            var open = new Microsoft.Win32.SaveFileDialog
            {
                Title = "选择保存路径",
                Filter = filter,
                FileName = name
            };
            return open.ShowDialog() == true ? open.FileName : null;
        }

        /// <summary>
        /// 选择个文件
        /// </summary>
        /// <returns></returns>
        public static string ChooseFile(string filter = "文本文件|*.txt|所有文件|*.*")
        {
            var open = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = filter,
                Title = "选择文件"
            };
            return open.ShowDialog() == true ? open.FileName : string.Empty;
        }
    }
}
