﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Utils
{
    public class Crc32Tab
    {
        const uint CRCPOL = 0xedb88320;

        public static Crc32Tab Instance = new Crc32Tab();

        public uint[] Tab { get; private set; } = new uint[256];

        public uint[] InvTab { get; private set; } = new uint[256];

        public Crc32Tab()
        {
            for (uint i = 0; i < 256; i++)
            {
                var crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc = (crc >> 1) ^ CRCPOL;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }
                Tab[i] = crc;
                InvTab[Msb(crc)] = crc << 8 ^ i;
            }
        }

        public static uint Crc32(uint pval, byte b)
        {
            return pval >> 8 ^ Instance.Tab[Lsb(pval) ^ b];
        }

        public static uint Crc32Inv(uint crc, byte b)
        {
            return crc << 8 ^ Instance.InvTab[Msb(crc)] ^ b;
        }

        public static byte Lsb(uint x)
        {
            return (byte)x;
        }

        public static byte Msb(uint x)
        {
            return (byte)(x >> 24);
        }
    }
}
