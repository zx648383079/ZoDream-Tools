using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageToIco.Drawing
{
    public static class Ico
    {
        /// <summary>
        /// 每张图片头
        /// </summary>
        const int SizeIconImageEntry = 16;
        /// <summary>
        /// 文件头
        /// </summary>
        const int SizeIconHeader = 6;
        const uint MaxWidth = 256;
        const uint MaxHeight = 256;

        public static Bitmap ResizeImage(int width, int height, Image source)
        {
            return ResizeImage(width, height, source, SmoothingMode.HighQuality);
        }
        /// <summary>
        /// 修改图片尺寸
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap ResizeImage(int width, int height, Image source, SmoothingMode quality)
        {
            var bmp = new Bitmap(width, height);
            bmp.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                switch (quality)
                {
                    case SmoothingMode.AntiAlias:
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        break;
                    case SmoothingMode.HighQuality:
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        break;
                    case SmoothingMode.HighSpeed:
                        g.CompositingQuality = CompositingQuality.HighSpeed;
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                        g.SmoothingMode = SmoothingMode.HighSpeed;
                        break;
                }
                using (var ia = new ImageAttributes())
                {
                    ia.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(source, new Rectangle(0, 0, width, height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);
                }
            }
            return bmp;
        }

        public static bool Converter(IEnumerable<Image> images, Stream stream)
        {
            var bw = new BinaryWriter(stream);
            AddIconHeader(images.Count(), bw);
            var offset = SizeIconHeader + (SizeIconImageEntry * images.Count());
            var cacheItems = new Dictionary<int, byte[]>();
            foreach (var item in images)
            {
                // var length = GetImageSize(item);
                var png = ImageCorrection(item);
                var bytes = CreateBuffer(png);
                cacheItems[offset] = bytes;
                var length = bytes.Length;
                if (png != item)
                {
                    png.Dispose();
                }
                AddImageHeader(offset, item, length, bw);
                offset += length;
            }
            foreach (var item in cacheItems)
            {
                bw.BaseStream.Seek(item.Key, SeekOrigin.Begin);
                bw.Write(item.Value);
            }
            bw.Dispose();
            return true;
        }

        /// <summary>
        /// 根据一张图，生成不同尺寸
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sizes"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool Converter(Image source, int[] sizes, Stream stream)
        {
            return Converter(source, sizes, SmoothingMode.HighQuality, stream);
        }

        public static bool Converter(Image source, int[] sizes, SmoothingMode quality, Stream stream)
        {
            var items = new List<Bitmap>();
            // Array.Sort(sizes, (p1, p2) => p2.CompareTo(p1));
            foreach (var item in sizes)
            {
                items.Add(ResizeImage(item, item, source, quality));
            }
            return Converter(items, stream);
        }

        /// <summary>
        /// 传入多张图片，自动根据较大尺寸选择
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sizes"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool Converter(IEnumerable<Image> data, int[] sizes, Stream stream)
        {
            return Converter(data, sizes, SmoothingMode.HighQuality, stream);
        }

        public static bool Converter(IEnumerable<Image> data, int[] sizes, SmoothingMode quality, Stream stream)
        {
            data = data.OrderBy(x => x.Width).ThenBy(x => x.Height);
            var items = new List<Bitmap>();
            foreach (var item in sizes)
            {
                var image = GetThanImage(item, data);
                items.Add(ResizeImage(item, item, image, quality));
            }
            return Converter(items, stream);
        }

        private static Image GetThanImage(int width, IEnumerable<Image> data)
        {
            foreach (var item in data)
            {
                if (item.Width >= width)
                {
                    return item;
                }
            }
            return data.Last();
        }

        /// <summary>
        /// 写头
        /// </summary>
        /// <param name="count"></param>
        /// <param name="writer"></param>
        private static void AddIconHeader(int count, BinaryWriter writer)
        {
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)count); // 
        }


        private static int GetImageSize(Image image)
        {
            return image.Height * image.Width * 4;// * Image.GetPixelFormatSize(image.PixelFormat) / 1024 / 1024;
        }

        private static void AddImageHeader(int offset, Image image, int imageLength, BinaryWriter writer)
        {
            var width = image.Width >= MaxWidth ? byte.MinValue : image.Width;
            var height = image.Height >= MaxHeight ? byte.MinValue : image.Height;
            var bpp = Image.GetPixelFormatSize(image.PixelFormat);

            writer.Write((byte)width);
            writer.Write((byte)height);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((ushort)1);
            writer.Write((ushort)bpp);
            writer.Write((uint)imageLength);
            writer.Write((uint)offset);
        }


        private static byte[] CreateBuffer(Image image)
        {
            byte[] ba;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                ba = ms.ToArray();
            }
            return ba;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image ImageCorrection(Image image)
        {
            var dispose = false;
            try
            {
                var img = image;
                if (!img.PixelFormat.Equals(PixelFormat.Format32bppArgb))
                {
                    var bitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppPArgb);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(img, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }
                    img = bitmap;
                }
                if (!img.RawFormat.Guid.Equals(ImageFormat.Png.Guid))
                {
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        img = Image.FromStream(ms);
                    }
                }
                if (image != img)
                {
                    dispose = true;
                }
                return img;
            }
            finally
            {
                //if (dispose)
                //    image?.Dispose();
            }
        }

    }
}
