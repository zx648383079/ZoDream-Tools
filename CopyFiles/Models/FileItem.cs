using System.IO;

namespace ZoDream.CopyFiles.Models
{
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;

        public bool IsFile { get; set; } = true;

        public string FileName { get; set; } = string.Empty;

        public FileItem(string fileName): this(Path.GetFileName(fileName), fileName)
        {
            
        }

        public FileItem(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
            var fileInfo = new FileInfo(fileName);
            IsFile = (fileInfo.Attributes & FileAttributes.Directory) == 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is string o)
            {
                return o == FileName;
            }
            if (obj is FileItem f)
            {
                return f.FileName == FileName;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
