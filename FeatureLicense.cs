using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BASeCamp.Licensing
{
    /// <summary>
    /// Feature Licensing class, generates and decodes a product key to and from a set of features.
    /// </summary>
    /// 
    /// 
    /// 
    public class FeatureLicenseData
    {
        
        public enum EditionConstants:byte
        {
            Edition_Standard,
            Edition_Professional,
            Edition_Enterprise,
            Edition_Ultimate


        }
        public byte Header { get; set; }
        public Int16 ProductCode { get; set; }
        public EditionConstants Edition { get; set; }
        public Int16 VersionCode { get; set; }
        private byte _FeatureTrialBits;

        public bool Trial { get; set; }






    }
    class FeatureLicense
    {
    }
}
