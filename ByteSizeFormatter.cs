using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BASeCamp.Updating
{
    public static class ByteSizeFormatter
    {

        

        private static String[] stdbytesuffixes = new String[]
    {
        " Bytes",
        "KB",
        "MB",
        "GB",
        "TB",
        "PB",
        "EB",
        "YB"
    };
    private static String[] isobytesuffixes = new string[] {
        " Bytes",
    "KiB",
    "MiB",
    "GiB",
    "TiB",
    "PiB",
    "EiB",
    "ZiB",
    "YiB"
            
    };

        /// <summary>
        /// gets the byte prefix index of the private arrays to use for the given value.
        /// </summary>
        /// <param name="bytevalue"></param>
        /// <returns></returns>
        private static int getbyteprefixindex(long bytevalue)
        {

            return (int)(Math.Floor(Math.Log(bytevalue) / Math.Log(1024)));
            
        }
        public static String FormatSize(long amount,int numdecimalplaces=2,bool useISO=false)
        {
            return FormatSizeDirect(amount, getbyteprefixindex(amount), numdecimalplaces, useISO);
            

        }

        private static String FormatSizeDirect(long amount, int index,int digitsafterdecimal=2,bool useISO=false)
        {
            if(amount==0) return "0 Bytes";
            String[] usesuffixes = useISO ? isobytesuffixes : stdbytesuffixes;
            String buildresult;
            double amountuse = amount;
            amountuse = amountuse / (Math.Pow(1024, index));
            String formatstring = "{0:0." + String.Join("", Enumerable.Repeat("0", digitsafterdecimal).ToArray()) + "}";
            Debug.Print(formatstring);
            buildresult = String.Format(formatstring, amountuse);
            buildresult += " " + usesuffixes[index];
            
            return buildresult;
        }

        /// <summary>
        /// formats a set of byte values to use the most honest display; that is, if we have 23 bytes and 1440 bytes, both will be displayed as bytes, but if it is 1330 and 1440, it shows as KB.
        /// 
        /// </summary>
        /// <param name="bytesizes"></param>
        /// <returns></returns>
        public static IEnumerable<String> FormatSizes(IEnumerable<long> bytesizes)
        {

            //iterate through all the elements, and find the lowest byteprefixindex...
            int currlowest = stdbytesuffixes.Length + 1;
            currlowest = bytesizes.Select(getbyteprefixindex).Concat(new[] {currlowest}).Min();
            return bytesizes.Select(iteratesize => FormatSizeDirect(iteratesize, currlowest));

        }
    }
}
