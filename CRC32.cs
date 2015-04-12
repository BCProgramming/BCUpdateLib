using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BASeCamp.Updating
{
    public class CRC32 : HashAlgorithm
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        public CRC32(UInt32 polynomial, UInt32 pseed)
        {
            table = InitializeTable(polynomial);
            seed = pseed;
            Initialize();

        }

        public CRC32()
            : this(DefaultPolynomial, DefaultSeed)
        {


        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            hash = CalculateHash(table, hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            byte[] HashBuffer = UInt32ToBigEndianBytes(~hash);
            HashValue = HashBuffer;
            return HashBuffer;
        }
        public override int HashSize
        {
            get
            {
                return 32;
            }
        }

        public static UInt32 Compute(byte[] buffer)
        {
            return Compute(DefaultPolynomial, DefaultSeed, buffer);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return Compute(DefaultPolynomial, seed, buffer);
        }

        public static UInt32 Compute(UInt32 Polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(Polynomial), seed, buffer, 0, buffer.Length);

        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;





        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }

            }
            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 u)
        {
            return new byte[] {
                (byte)((u >> 24)&0xff),
                (byte)((u >> 16) & 0xff),
                (byte)((u >> 8) & 0xff),
                (byte)((u & 0xff))
                };


        }
        public static String GetCRC32forFile_String(String filename)
        {
            byte[] crcvalue = GetCRC32forFile(filename);
            return CRCToString(crcvalue);

        }
        public static String CRCToString(byte[] buffer)
        {

            StringBuilder hashbuild = new StringBuilder();

            foreach (byte b in buffer)
                hashbuild.Append(b.ToString("x2").ToLower());


            return hashbuild.ToString();

        }

        public static byte[] GetCRC32forFile(String filename)
        {
            CRC32 crc32 = new CRC32();

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return crc32.ComputeHash(fs);

            }


        }
        public static byte[] ComputeCRC(byte[] buffer)
        {
            CRC32 crc32 = new CRC32();
            return crc32.ComputeHash(buffer);


        }

    }
}
