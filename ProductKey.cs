/*
 * BASeCamp BCUpdateLib
Copyright (c) 2011, Michael Burgwin
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in 
    the documentation and/or other materials provided with the distribution.
    Neither the name of BASeCamp Corporation nor the names of its contributors may be used to endorse or promote products derived 
    from this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
 * BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO 
 * EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BASeCamp.Licensing 
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Reflection;
    using System.ComponentModel;



    #region Base32 encoder/decoder

    public class Base32Encoder
    {
        private const string ENCODING_TABLE = "abcdefghijklmnopqrstuvwxyz234567";
        private const char DEF_PAD = '=';
        private readonly byte[] dTable; //Decoding table

        private readonly string eTable; //Encoding table
        private readonly char padding;

        public Base32Encoder()
            : this(ENCODING_TABLE, DEF_PAD)
        {
        }

        public Base32Encoder(char padding)
            : this(ENCODING_TABLE, padding)
        {
        }

        public Base32Encoder(string encodingTable)
            : this(encodingTable, DEF_PAD)
        {
        }

        public Base32Encoder(string encodingTable, char padding)
        {
            this.eTable = encodingTable;
            this.padding = padding;
            dTable = new byte[0x80];
            InitialiseDecodingTable();
        }

        public virtual string Encode(byte[] input)
        {
            var output = new StringBuilder();
            int specialLength = input.Length % 5;
            int normalLength = input.Length - specialLength;
            for (int i = 0; i < normalLength; i += 5)
            {
                int b1 = input[i] & 0xff;
                int b2 = input[i + 1] & 0xff;
                int b3 = input[i + 2] & 0xff;
                int b4 = input[i + 3] & 0xff;
                int b5 = input[i + 4] & 0xff;

                output.Append(eTable[(b1 >> 3) & 0x1f]);
                output.Append(eTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
                output.Append(eTable[(b2 >> 1) & 0x1f]);
                output.Append(eTable[((b2 << 4) | (b3 >> 4)) & 0x1f]);
                output.Append(eTable[((b3 << 1) | (b4 >> 7)) & 0x1f]);
                output.Append(eTable[(b4 >> 2) & 0x1f]);
                output.Append(eTable[((b4 << 3) | (b5 >> 5)) & 0x1f]);
                output.Append(eTable[b5 & 0x1f]);
            }

            switch (specialLength)
            {
                case 1:
                    {
                        int b1 = input[normalLength] & 0xff;
                        output.Append(eTable[(b1 >> 3) & 0x1f]);
                        output.Append(eTable[(b1 << 2) & 0x1f]);
                        output.Append(padding).Append(padding).Append(padding).Append(padding).Append(padding).Append(
                            padding);
                        break;
                    }

                case 2:
                    {
                        int b1 = input[normalLength] & 0xff;
                        int b2 = input[normalLength + 1] & 0xff;
                        output.Append(eTable[(b1 >> 3) & 0x1f]);
                        output.Append(eTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
                        output.Append(eTable[(b2 >> 1) & 0x1f]);
                        output.Append(eTable[(b2 << 4) & 0x1f]);
                        output.Append(padding).Append(padding).Append(padding).Append(padding);
                        break;
                    }
                case 3:
                    {
                        int b1 = input[normalLength] & 0xff;
                        int b2 = input[normalLength + 1] & 0xff;
                        int b3 = input[normalLength + 2] & 0xff;
                        output.Append(eTable[(b1 >> 3) & 0x1f]);
                        output.Append(eTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
                        output.Append(eTable[(b2 >> 1) & 0x1f]);
                        output.Append(eTable[((b2 << 4) | (b3 >> 4)) & 0x1f]);
                        output.Append(eTable[(b3 << 1) & 0x1f]);
                        output.Append(padding).Append(padding).Append(padding);
                        break;
                    }
                case 4:
                    {
                        int b1 = input[normalLength] & 0xff;
                        int b2 = input[normalLength + 1] & 0xff;
                        int b3 = input[normalLength + 2] & 0xff;
                        int b4 = input[normalLength + 3] & 0xff;
                        output.Append(eTable[(b1 >> 3) & 0x1f]);
                        output.Append(eTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
                        output.Append(eTable[(b2 >> 1) & 0x1f]);
                        output.Append(eTable[((b2 << 4) | (b3 >> 4)) & 0x1f]);
                        output.Append(eTable[((b3 << 1) | (b4 >> 7)) & 0x1f]);
                        output.Append(eTable[(b4 >> 2) & 0x1f]);
                        output.Append(eTable[(b4 << 3) & 0x1f]);
                        output.Append(padding);
                        break;
                    }
            }

            return output.ToString();
        }

        public virtual byte[] Decode(string data)
        {
            var outStream = new List<Byte>();

            int length = data.Length;
            while (length > 0)
            {
                if (!this.Ignore(data[length - 1])) break;
                length--;
            }

            int i = 0;
            int finish = length - 8;
            for (i = this.NextI(data, i, finish); i < finish; i = this.NextI(data, i, finish))
            {
                byte b1 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b2 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b3 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b4 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b5 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b6 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b7 = dTable[data[i++]];
                i = this.NextI(data, i, finish);
                byte b8 = dTable[data[i++]];

                outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
                outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
                outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
                outStream.Add((byte)((b5 << 7) | (b6 << 2) | (b7 >> 3)));
                outStream.Add((byte)((b7 << 5) | b8));
            }
            this.DecodeLastBlock(outStream,
                                 data[length - 8], data[length - 7], data[length - 6], data[length - 5],
                                 data[length - 4], data[length - 3], data[length - 2], data[length - 1]);

            return outStream.ToArray();
        }

        protected virtual int DecodeLastBlock(ICollection<byte> outStream, char c1, char c2, char c3, char c4, char c5,
                                              char c6, char c7, char c8)
        {
            if (c3 == padding)
            {
                byte b1 = dTable[c1];
                byte b2 = dTable[c2];
                outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
                return 1;
            }

            if (c5 == padding)
            {
                byte b1 = dTable[c1];
                byte b2 = dTable[c2];
                byte b3 = dTable[c3];
                byte b4 = dTable[c4];
                outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
                outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
                return 2;
            }

            if (c6 == padding)
            {
                byte b1 = dTable[c1];
                byte b2 = dTable[c2];
                byte b3 = dTable[c3];
                byte b4 = dTable[c4];
                byte b5 = dTable[c5];

                outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
                outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
                outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
                return 3;
            }

            if (c8 == padding)
            {
                byte b1 = dTable[c1];
                byte b2 = dTable[c2];
                byte b3 = dTable[c3];
                byte b4 = dTable[c4];
                byte b5 = dTable[c5];
                byte b6 = dTable[c6];
                byte b7 = dTable[c7];

                outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
                outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
                outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
                outStream.Add((byte)((b5 << 7) | (b6 << 2) | (b7 >> 3)));
                return 4;
            }

            else
            {
                byte b1 = dTable[c1];
                byte b2 = dTable[c2];
                byte b3 = dTable[c3];
                byte b4 = dTable[c4];
                byte b5 = dTable[c5];
                byte b6 = dTable[c6];
                byte b7 = dTable[c7];
                byte b8 = dTable[c8];
                outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
                outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
                outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
                outStream.Add((byte)((b5 << 7) | (b6 << 2) | (b7 >> 3)));
                outStream.Add((byte)((b7 << 5) | b8));
                return 5;
            }
        }

        protected int NextI(string data, int i, int finish)
        {
            while ((i < finish) && this.Ignore(data[i])) i++;

            return i;
        }

        protected bool Ignore(char c)
        {
            return (c == '\n') || (c == '\r') || (c == '\t') || (c == ' ') || (c == '-');
        }

        protected void InitialiseDecodingTable()
        {
            for (int i = 0; i < eTable.Length; i++)
            {
                dTable[eTable[i]] = (byte)i;
            }
        }
    }


    public class ZBase32Encoder : Base32Encoder
    {
        private const string DEF_ENCODING_TABLE = "ybndrfg8ejkmcpqxot1uwisza345h769";
        private const char DEF_PADDING = '=';

        public ZBase32Encoder()
            : base(DEF_ENCODING_TABLE, DEF_PADDING)
        {
        }

        public override string Encode(byte[] input)
        {
            var encoded = base.Encode(input);
            return encoded.TrimEnd(DEF_PADDING);
        }

        public override byte[] Decode(string data)
        {
            //Guess the original data size
            int expectedOrigSize = Convert.ToInt32(Math.Floor(data.Length / 1.6));
            int expectedPaddedLength = 8 * Convert.ToInt32(Math.Ceiling(expectedOrigSize / 5.0));
            string base32Data = data.PadRight(expectedPaddedLength, DEF_PADDING).ToLower();

            return base.Decode(base32Data);
        }
    }

    #endregion


    /// <summary>
    /// Simple class that uses a BitArray to convert a byte to and from a set of booleans.
    /// </summary>
    internal static class BitBasher
    {
        /// <summary>
        /// returns the first byte of data from a BitArray.
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        private static Byte BitArrayToByte(BitArray ba)
        {
            byte[] copytoarray = new byte[1];
            ba.CopyTo(copytoarray, 0);

            return copytoarray[0];
        }

        /// <summary>
        /// changes the value of a given bit within the passed byte.
        /// </summary>
        /// <param name="manipulate"></param>
        /// <param name="position"></param>
        /// <param name="newvalue"></param>
        public static void ChangeBit(ref byte manipulate, int position, bool newvalue)
        {
            BitArray ba = new BitArray(new byte[] { manipulate });
            ba[position] = newvalue;
            manipulate = BitArrayToByte(ba);
        }

        public static bool GetBit(byte check, int position)
        {
            BitArray ba = new BitArray(new byte[] { check });
            return ba[position];
        }

        public static void SetBit(ref byte setwithin, int position)
        {
            ChangeBit(ref setwithin, position, true);
        }
        public static void SetBit(ref byte setwithin, int position, bool value)
        {

            ChangeBit(ref setwithin, position, value);

        }
    }

    /// <summary>
    /// basic class to store Date without Time Information.
    /// </summary>
    public class DateOnly
    {
        public DateOnly(byte pDay, byte pMonth, UInt16 pYear)
        {
            Day = pDay;
            Month = pMonth;
            Year = pYear;
        }

        public UInt16 Year { get; set; }
        public byte Month { get; set; }
        public byte Day { get; set; }
        public int ToInt()
        {
            return Year ^ Month | Day;
        }

        public override string ToString()
        {
            return ((DateTime)this).ToString();
        }

        public static implicit operator DateTime(DateOnly dateobj)
        {
            return new DateTime(dateobj.Year, dateobj.Month, dateobj.Day);
        }

        public static implicit operator DateOnly(DateTime dateobj)
        {
            return new DateOnly((byte)dateobj.Day, (byte)dateobj.Month, (UInt16)dateobj.Year);
        }
    }

    /// <summary>
    /// Exception raised when an invalid key is detected.
    /// </summary>
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException()
            : base("Invalid Key")
        {
        }

        public InvalidKeyException(String message)
            : base(message)
        {
        }

        public InvalidKeyException(String message, Exception innerexception)
            : base(message, innerexception)
        {
        }
    }


    /// <summary>
    /// Class used for dealing with the feature-based License data.
    /// </summary>
    public class ProductKey
    {

        ///<summary>retrieves the String value of a given Enum. This looks for a Description Attribute, returning it if present. 
        ///If not, it just uses Enum.GetName().</summary>
        public static string GetStringValue(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var getattribs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (getattribs.Length > 0)
                return (getattribs[0] as DescriptionAttribute).Description;
            else
                return Enum.GetName(value.GetType(), value);

        }

        #region EditionConstants enum

        /// <summary>
        /// Designates the various Editions. These are packed into a byte.
        /// </summary>
        public enum Editions : byte
        {
            [Description("Standard")]
            Standard,
            [Description("Professional")]
            Professional,
            [Description("Enterprise")]
            Enterprise,
            [Description("Ultimate")]
            Ultimate
        }

        public enum Products : short
        {
            [Description("BASeCamp JobClock")]
            BCJobClock = 0,
            [Description("BASeCamp BASeBlock")]
            BASeBlock = 21
            
        }


        #endregion

        private byte _FeatureTrialBits;

        private byte? _footer;
        private byte? _header;

        public ProductKey()
        {
            MachineID = getLocalMachineID();
        }

        public ProductKey(Stream readFrom)
            : this()
        {
            FromStream(readFrom);
        }

        /// <summary>
        /// attempt Construct a new FeatureLicenseData object from the given product key.
        /// Decryption is done using MachineID, which is initialized to the value in HKLM/Software/Microsoft/Cryptography/MachineGuid
        /// If decryption fails, or another error occurs, an InvalidKeyException is thrown.
        /// </summary>
        /// <param name="fromproductcode">product code to read</param>
        /// <exception cref="InvalidKeyException">The given product code could not be decrypted, or an error occured attempting to do so.</exception>
        public ProductKey(String fromproductcode)
            : this(fromproductcode, getLocalMachineID())
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Header:" + Header.ToString());
            sb.AppendLine("Edition:" + Edition.ToString());
            sb.AppendLine("Trial/Features byte:" + _FeatureTrialBits.ToString());
            sb.AppendLine("Expiry Date:" + this.ExpiryDate.ToString());
            sb.AppendLine("Serial Number:" + this.SerialNumber);
            sb.AppendLine("Product Code:" + this.ProductCode);
            return sb.ToString();
        }

        /// <summary>
        /// Constructs a new FeatureLicenseData object from the given product key, using the passed Machine identifier to
        /// decrypt it's contents.
        /// </summary>
        /// <param name="fromproductcode"></param>
        /// <param name="IDString"></param>
        public ProductKey(String fromproductcode, String IDString)
            : this()
        {
            //remove any hyphens/dashes, first.
            MachineID = IDString;
            fromproductcode = fromproductcode.Replace("-", "");
            //convert the string to a byte array via zBase32:

            ZBase32Encoder zb = new ZBase32Encoder();
            byte[] acquiredcode = zb.Decode(fromproductcode);


            //byte[] acquiredcode = Enumerable.Range(0, fromproductcode.Length)
            //         .Where(x => x % 2 == 0)
            //         .Select(x => Convert.ToByte(fromproductcode.Substring(x, 2), 16))
            //         .ToArray();


            //now, we need to decrypt it...
            byte[] decrypted = Decrypt(acquiredcode, MachineID);

            //armed with the decrypted data, toss it into a memory stream
            MemoryStream ms = new MemoryStream(decrypted);
            ms.Seek(0, SeekOrigin.Begin);

            //now, invoke FromStream...
            FromStream(ms);
        }

        public byte Header
        {
            get
            {
                if (_header != null) return _header.Value;
                return calcheader();
            }
            set { _header = value; }
        }

        public byte Footer
        {
            get
            {
                if (_footer != null) return _footer.Value;
                return calcfooter(Header);
            }
            private set { _footer = value; }
        }
        /// <summary>
        /// Product is the same as  ProductCode, but returned as a Products enum type.
        /// </summary>
        public Products Product { get { return (Products)ProductCode; } set { ProductCode = (short)value; } }
        public Int16 ProductCode { get; set; }
        public Editions Edition { get; set; }
        //trial and the seven features are automatically smacked into and out of the private byte variable.


        public bool Trial
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 0); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 0, value); }
        }

        public bool Feature1
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 1); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 1, value); }
        }

        public bool Feature2
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 2); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 2, value); }
        }

        public bool Feature3
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 3); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 3, value); }
        }

        public bool Feature4
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 4); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 4, value); }
        }

        public bool Feature5
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 5); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 5, value); }
        }

        public bool Feature6
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 6); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 6, value); }
        }

        public bool Feature7
        {
            get { return BitBasher.GetBit(_FeatureTrialBits, 7); }
            set { BitBasher.SetBit(ref _FeatureTrialBits, 7, value); }
        }
        private DateOnly _ExpiryDate = DateTime.Now;
        public UInt16 SerialNumber { get; set; }
        public DateOnly ExpiryDate { get { return _ExpiryDate; } set { _ExpiryDate = value; } }
        public byte MajorVersion { get; set; }
        /// <summary>
        /// This is initialized with the value from GetLocalMachineID().
        /// </summary>
        public String MachineID { get; set; }

        /// <summary>
        /// calculates our header value.
        /// </summary>
        /// <returns></returns>
        private byte calcheader()
        {

            return (byte)(((ProductCode +
                             (Int16)((Math.Pow((double)Edition, (double)_FeatureTrialBits)))) +
                            (Math.Pow(SerialNumber, ExpiryDate.ToInt()))) % 255);
        }

        /// <summary>
        /// calculates our footer value for the given header value.
        /// </summary>
        /// <param name="headervalue"></param>
        /// <returns></returns>
        private byte calcfooter(byte headervalue)
        {
            return
                (byte)
                ((ExpiryDate.ToInt() - (SerialNumber * SerialNumber) / (byte)(Edition + 1) + _FeatureTrialBits ^
                  headervalue) % 255);
        }

        /// <summary>
        /// writes out the data directly to a stream. Note that the data is NOT encrypted.
        /// </summary>
        /// <param name="WriteTo">Stream to write to</param>
        /// <exception cref="ArgumentException">The given Stream is not writable.</exception>
        public void DataToStream(Stream WriteTo)
        {
            if (!WriteTo.CanWrite) throw new ArgumentException("Given Stream must be writable.", "WriteTo");
            //StreamWriter sw = new StreamWriter(WriteTo);
            BinaryWriter bw = new BinaryWriter(WriteTo);
            //byte calculatedheader = calcheader();

            bw.Write(Header);
            bw.Write(ProductCode);
            bw.Write(MajorVersion);
            bw.Write((byte)Edition);
            bw.Write(_FeatureTrialBits);
            bw.Write(SerialNumber);
            bw.Write(ExpiryDate.Day);
            bw.Write(ExpiryDate.Month);
            bw.Write((UInt16)ExpiryDate.Year);

            var gotfooter = calcfooter(Header);
            bw.Write(gotfooter);
        }


      

        /// <summary>
        /// Creates and returns a Machine specific identifier for this machine. 
        /// </summary>
        /// <returns></returns>
        public static String getLocalMachineID()
        {
            const int KEY_WOW64_64KEY = 0x0100; //access 64-bit from 32-bit app
            const int KEY_WOW64_32KEY = 0x0200;
            String ReadKey = "SOFTWARE\\Microsoft\\Cryptography";
            int keyresult = 0;


            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography
            RegistryKey rk = null;
            String mkid = "";
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(ReadKey);

            }
            catch { }
            if (rk != null)
                mkid = (String)rk.GetValue("MachineGuid");


            if (String.IsNullOrEmpty(mkid))
            {



                mkid = System.Environment.MachineName;

            }


            return mkid;
        }

        /// <summary>
        /// determines if the given key is valid; uses the MachineID for the local machine.
        /// </summary>
        /// <param name="ProductKey">ProductKey to test</param>
        /// <returns></returns>
        public static bool IsValidKey(String ProductKey)
        {
            return IsValidKey(ProductKey, getLocalMachineID());
        }

        /// <summary>
        /// Determines if this Product Key is valid for the given machineID.
        /// </summary>
        /// <param name="ProductKey">Product Key to test</param>
        /// <returns>true if key given is valid for the given MachineID; false otherwise.</returns>
        public static bool IsValidKey(String ProductKey, String MachineID)
        {
            //simply try to construct a FeatureLicenseData Object. it will throw an exception
            //from it's constructor if the given values aren't valid.
            try
            {
                ProductKey fld = new ProductKey(ProductKey, MachineID);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static readonly String Licensingregkey = "Software\\BASeCamp\\Licensing\\Key";

        /// <summary>
        /// GetProductInformation: retrieves the product information from the registry. Or, if request is true, will
        /// show the registration dialog and request the information for a key.
        /// </summary>
        /// <param name="AppName">Name of Application. This will be used for looking up the Application name, and should match a previous call to Register.</param>
        /// <param name="AdditionalCheck">Additional Function to perform further checks.</param>
        /// <param name="request">whether to show the dialog and request product key information if there is none accessible.</param>
        /// <param name="ownerwindow">Owner window for any dialogs.</param>
        /// <returns>a ProductKey Object representing valid ProductKey information for the given product if successful. null if the request dialog is cancelled or if request is false and
        /// no information is found for the given product.</returns>
        /// <remarks>This function will also call Register() if request is true and it needs to show the dialog.</remarks>
        public static ProductKey GetProductInformation(String AppName, frmProductReg.AdditionalValidCheckFunction AdditionalCheck, bool request,IWin32Window ownerwindow,String IDString)
        {
            ProductKey pk = GetRegisteredProduct(AppName,IDString);
            if (pk == null && request)
            {
                if (String.IsNullOrEmpty(IDString)) IDString = ProductKey.getLocalMachineID();
                //if request is enabled, we need to show the dialog.
                frmProductReg regdialog = new frmProductReg(AppName, AdditionalCheck,IDString);
                
                if (regdialog.ShowDialog() == DialogResult.Cancel)
                    return null;
                
                //otherwise, return the result.
                return regdialog.constructedPK;


                


            }

            return pk;



        }
        public static ProductKey GetProductInformation(String AppName, ProductKey.Products product, bool request, IWin32Window ownerwindow,String IDString)
        {
            return GetProductInformation(AppName, (w) => w.Product == product?"":"Key not for this Product.", request, ownerwindow,IDString);

        }
        public void Register(String AppName)
        {
          
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(Licensingregkey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            
            //create a new item for AppName
            RegistryKey appkey = rk.CreateSubKey(AppName);
            //each appkey will have a "key" value, which is the ProductKey.
            appkey.SetValue("Key", GetProductCode());
            appkey.SetValue("ID", MachineID);

          



        }
        /// <summary>
        /// retrieves the ProductKey for the given product from the registry.
        /// </summary>
        /// <param name="ProductName">Name of product to retrieve.</param>
        /// <returns>the ProductKey Object for the given product, or null if the application is not registered.</returns>
        public static ProductKey GetRegisteredProduct(String ProductName,String IDString)
        {
            //step one, get the ProductCode.
            try
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(Licensingregkey);
                RegistryKey appkey = rk.CreateSubKey(ProductName);
                String getcode = (String)appkey.GetValue("Key");
                String getID = (String)appkey.GetValue("ID");
                
                ProductKey retrievekey = new ProductKey(getcode,getID);
                return retrievekey;
            }
            catch (Exception exx)
            {
                return null;

            }



        }
        /// <summary>
        /// Reads Class fields and data from a stream.
        /// </summary>
        /// <param name="readFrom"></param>
        /// <exception cref="ArgumentException">passed Stream must be readable.</exception>
        /// <exception cref="InvalidDataException">When the header and footer do not match the calculated values.</exception>
        /// <remarks>The Stream is not closed and the caller remains responsible for doing so.</remarks>
        private void FromStream(Stream readFrom)
        {
            if (!readFrom.CanRead) throw new ArgumentException("Stream must be readable.", "readFrom");
            try
            {
                BinaryReader br = new BinaryReader(readFrom);
                Header = (byte)br.ReadByte();
                //bw.Write(ProductCode);
                ProductCode = br.ReadInt16();
                MajorVersion = br.ReadByte();
                Edition = (Editions)br.ReadByte();

                _FeatureTrialBits = (byte)br.ReadByte();


                SerialNumber = br.ReadUInt16();

                byte exDay, exMonth;
                UInt16 exYear;
                exDay = (byte)br.ReadByte();
                exMonth = (byte)br.ReadByte();
                exYear = br.ReadUInt16();
                byte readfooter = (byte)br.ReadByte();
                ExpiryDate = new DateOnly(exDay, exMonth, exYear);

                byte calculatedfooter = calcfooter(Header);


                if (calculatedfooter != readfooter)
                    throw new InvalidDataException("Invalid License Data in Stream.");
            }
            catch (InvalidCastException exx)
            {
                //occurs when we try to cast a -1 to a byte
                throw new IOException("Unexpected Error Parsing Stream", exx);
            }
        }


        /// <summary>
        /// Encrypts a byte array using a string password.
        /// </summary>
        /// <param name="clearData">Bytes to Encrypt</param>
        /// <param name="password">Password to be used to encrypt this data. This password will be needed to decrypt the 
        /// data as well.</param>
        /// <returns>An array of bytes corresponding to the result of encrypting the passed array with the given password.</returns>
        private static byte[] Encrypt(byte[] clearData, String password)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(password);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] readresults = new byte[ms.Length];
            ms.Read(readresults, 0, readresults.Length);
            //We need a specific amount of data for the key and Initial Vector of the 
            //encryption algorithm, so we'll use the PasswordDeriveBytes class to 
            //salt the data. 
            //the current salt is hard coded, but it could also be made to use some sort of machine-specific value.
            Rfc2898DeriveBytes pdb = GetPdb(password);

            return Encrypt(clearData, pdb.GetBytes(8), pdb.GetBytes(8));
        }
        /// <summary>
        /// Dencrypts a byte array using a string password.
        /// </summary>
        /// <param name="clearData">Bytes to Decrypt</param>
        /// <param name="password">Password to be used to decrypt this data. This password needs to be the same as the password used during encryption.</param>
        /// <returns>An array of bytes corresponding to the result of encrypting the passed array with the given password.</returns>
        /// <exception cref="CryptographicException">The data is invalid, or the password specified is incorrect.</exception>
        private static byte[] Decrypt(byte[] clearData, String password)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(password);

            ms.Seek(0, SeekOrigin.Begin);
            byte[] readresults = new byte[ms.Length];
            ms.Read(readresults, 0, readresults.Length);
            Rfc2898DeriveBytes pdb = GetPdb(password);
            return Decrypt(clearData, pdb.GetBytes(8), pdb.GetBytes(8));
        }
        public static String insertdashes(String usevalue)
        {
            bool flip5 = false;
            int countfrom = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < usevalue.Length; i++)
            {
                countfrom++;
                sb.Append(usevalue[i]);
                if ((countfrom % (flip5 ? 5 : 4)) == 0)
                {
                    sb.Append("-");
                    flip5 = !flip5;
                    countfrom = 0;
                }


            }

            return sb.ToString();

        }
        private static Rfc2898DeriveBytes GetPdb(string password)
        {
            return new Rfc2898DeriveBytes(password,
                                           new byte[]
                                               {
                                                   0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                                                   0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                                               });
        }

        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream to accept the encrypted bytes 
            MemoryStream ms = new MemoryStream();



            RC2 alg = RC2.Create();

            alg.Key = Key;
            alg.IV = IV;


            CryptoStream cs = new CryptoStream(ms,
                                               alg.CreateEncryptor(), CryptoStreamMode.Write);


            cs.Write(clearData, 0, clearData.Length);


            cs.Close();


            byte[] encryptedData = ms.ToArray();

            return encryptedData;
        }

        // Decrypt a byte array into a byte array using a key and an IV 
        private static byte[] Decrypt(byte[] cipherData,
                                      byte[] Key, byte[] IV)
        {
            // Create a MemoryStream that is going to accept the
            // decrypted bytes 
            MemoryStream ms = new MemoryStream();


            RC2 alg = RC2.Create();

            alg.Key = Key;
            alg.IV = IV;

            CryptoStream cs = new CryptoStream(ms,
                                               alg.CreateDecryptor(), CryptoStreamMode.Write);


            cs.Write(cipherData, 0, cipherData.Length);


            cs.Close();


            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }

        /// <summary>
        /// Creates a ProductCode, or Key, from the data stored in this class and the MachineID.
        /// </summary>
        /// <returns></returns>
        public String GetProductCode()
        {
            MemoryStream mstream = new MemoryStream();

            //first, write our data out to a memorystream.
            DataToStream(mstream);


            //seek to the start, read as a string.
            mstream.Seek(0, SeekOrigin.Begin);
            //read it back, as a array of bytes.
            Byte[] readdata = new byte[mstream.Length];
            mstream.Read(readdata, 0, readdata.Length);

            //Encrypt the array of bytes using the MachineID. this is set in the constructor to getLocalMachineID(), but can of course
            //be changed by the caller before calling GetProductCode (for example, for generating the key elsewhere)
            Byte[] encrypted = Encrypt(readdata, MachineID);

            //now we need a readable form, so encode using zBase32, which has 
            //good results for a human readable key.
            ZBase32Encoder zb = new ZBase32Encoder();


            return zb.Encode(encrypted).ToUpper();
        }


        #region file load and save functions

        //private const String datakey = "keydata"; //change as needed. this is the key used to encrypt decrypt a key file.

        public static bool SaveKeyFile(string filename, string ProductKey, string FingerPrint,
                                          DateTime lastCheckedDate, String SuretyKey)
        {
            return SaveKeyFile(filename, ProductKey, FingerPrint, lastCheckedDate, SuretyKey, null);
        }
        public static bool SaveKeyFile(String filename, String ProductKey, String FingerPrint, DateTime lastCheckedDate, String SuretyKey, ISerializable savethis)
        {
            using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
            {
                return SaveKeyFile(fs,ProductKey,FingerPrint,lastCheckedDate,SuretyKey,savethis);
            }
        }
        public static bool SaveKeyFile(Stream output, string ProductKey, string FingerPrint,
                                          DateTime lastCheckedDate, String SuretyKey, ISerializable savethis)
        {
            // Create a file with the ProductKey, FingerPrint, Header/Footer or CRC or Similar, and lastCheckedDate
            // encrypt the file contents.

           

            //create the file...
            

            var fs = output;

                //Initialize the Cryptography objects...
                RC2CryptoServiceProvider rc = new RC2CryptoServiceProvider();

                Rfc2898DeriveBytes pdb = GetPdb(SuretyKey);
                rc.Key = pdb.GetBytes(8);
                rc.IV = pdb.GetBytes(8);

                //open a cryptostream on the file.
                CryptoStream cs =
                    new CryptoStream(fs, rc.CreateEncryptor(), CryptoStreamMode.Write);

                //use a binarywriter to write data to the crypto stream.
                BinaryWriter sw = new BinaryWriter(cs);

                //write each piece of data...
                sw.Write(ProductKey);
                sw.Write(FingerPrint);
                sw.Write(lastCheckedDate.ToUniversalTime().Ticks);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(cs, savethis);


                cs.Close();
           

            return true;
        }
        public static void ReadKeyFile(String readfromfile, out string ProductKey, out String FingerPrint, out DateTime lastCheckedDate, String SuretyKey, out ISerializable readthis)
        {
            using (FileStream usefs = new FileStream(readfromfile, FileMode.Open))
            {
                ReadKeyFile(usefs, out ProductKey, out FingerPrint, out lastCheckedDate, SuretyKey, out readthis);

            }

        }
        /// <summary>
        ///  Loads a key file from disk.
        /// </summary>
        /// <param name="filename">Filename to load from</param>
        /// <param name="ProductKey">ProductKey. will be deserialized from the stream.</param>
        /// <param name="FingerPrint">Machine fingerprint. Will be deserialized from the stream.</param>
        /// <param name="lastCheckedDate">UTC Date corresponding to the time this file was last checked. </param>
        /// <exception cref="FileNotFoundException">File specified does not exist.</exception>
        /// <remarks>In order to verify that the loaded data is valid, call IsValidKey with the ProductKey and FingerPrint.</remarks>
        public static void ReadKeyFile(Stream inputstream, out string ProductKey, out string FingerPrint,
                                          out DateTime lastCheckedDate, String SuretyKey, out ISerializable readthis)
        {

            //throws exceptions for all of the above.
            // Open the file, decrypt the contents, return the ProductKey, FingerPrint and lastCheckedDate
            

            //get the binaryreader, etc.
            var fr = inputstream;
                RC2CryptoServiceProvider rc = new RC2CryptoServiceProvider();
                Rfc2898DeriveBytes pdb = GetPdb(SuretyKey);
                rc.Key = pdb.GetBytes(8);
                rc.IV = pdb.GetBytes(8);

                CryptoStream cs =
                    new CryptoStream(fr, rc.CreateDecryptor(), CryptoStreamMode.Read);

                BinaryReader br = new BinaryReader(cs);

                //sw.Write(ProductKey);
                //sw.Write(FingerPrint);
                //sw.Write(lastCheckedDate.ToUniversalTime().Ticks);
                ProductKey = br.ReadString();
                FingerPrint = br.ReadString();
                long grabticks = br.ReadInt64();
                BinaryFormatter bf = new BinaryFormatter();

            
                lastCheckedDate = DateTime.FromBinary(grabticks);
                try { readthis = (ISerializable)bf.Deserialize(cs); }
                catch { readthis = null; }
            
        }

        #endregion
    }



}