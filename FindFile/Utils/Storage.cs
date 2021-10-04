using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Utils
{
    public static class Storage
    {

        

        public static string GetMD5(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                return string.Empty;
            }
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                return GetMD5(fs);
            }
        }

        public static string Format(long v)
        {
            if (v < 0)
            {
                return "0";
            }
            else if (v >= 1024 * 1024 * 1024) //文件大小大于或等于1024MB
            {
                return string.Format("{0:0.00} GB", (double)v / (1024 * 1024 * 1024));
            }
            else if (v >= 1024 * 1024) //文件大小大于或等于1024KB
            {
                return string.Format("{0:0.00} MB", (double)v / (1024 * 1024));
            }
            else if (v >= 1024) //文件大小大于等于1024bytes
            {
                return string.Format("{0:0.00} KB", (double)v / 1024);
            }
            else
            {
                return string.Format("{0:0.00} B", v);
            }
        }

        public static string GetMD5(Stream fs)
        {
            var md5 = MD5.Create();
            var res = md5.ComputeHash(fs);
            var sb = new StringBuilder();
            foreach (var b in res)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetCRC32(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                return string.Empty;
            }
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                return GetCRC32(fs);
            }
        }

        public static string GetCRC32(Stream fs)
        {
            uint res = 0;
            while (true)
            {
                var b = fs.ReadByte();
                if (b == -1)
                {
                    break;
                }
                res = Crc32Tab.Crc32(res, (byte)b);
            }
            return res.ToString("X");
        }
    }
}
