using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
namespace BASeCamp.Licensing
{
    //
    public class ProtectedFunctionAttribute : Attribute
    {



    }

    public class UnlicensedCodeException : Exception
    {
        
        public UnlicensedCodeException(String message)
            : base(message)
        {

        }
        public UnlicensedCodeException(String message, Exception InnerException):base(message,InnerException)
        {


        }

    }

    public static class LicensedFeatureData
    {


        [DllImport("kernel32")]
        static extern int GetVersion();


        public static bool SecureStringEqual(SecureString ssa, SecureString ssb)
        {
            if ((ssa == null) || ssb == null) return false;
            if (ssa.Length != ssb.Length) return false;
            IntPtr ssaptr = IntPtr.Zero;
            IntPtr ssbptr = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                ssaptr = Marshal.SecureStringToBSTR(ssa);
                ssbptr = Marshal.SecureStringToBSTR(ssb);


                unsafe
                {
                    for (char* ptr1 = (char*)ssaptr.ToPointer(), ptr2 = (char*)ssbptr.ToPointer();
                        *ptr1 != 0 && *ptr2 != 0;
                        ++ptr1, ++ptr2)
                    {
                        if (*ptr1 != *ptr2)
                        {
                            return false;
                        }

                    }




                }

                return true;


            }
            finally
            {
                if (ssaptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ssaptr);

                if (ssbptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ssbptr);

            }


        }


        public static bool OnWindows()
        {

            try
            {
                int retval = GetVersion();
                return true;
            }
            catch (DllNotFoundException dllfind)
            {
                return false;

            }



        }

        public static Dictionary<String, Dictionary<String, Assembly>> ProtectedAssemblies = new Dictionary<String, Dictionary<String, Assembly>>();

        public static String getProtectedAssemblyFolder(String ProductName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BASeCamp" + Path.DirectorySeparatorChar + ProductName + Path.DirectorySeparatorChar + "ProtectedAssemblies");


        }
        /// <summary>
        /// copies the given File to the ProtectedAssembly store.
        /// </summary>
        /// <param name="ProductName">Name of the product</param>
        /// <param name="CurrentFilename">filename of the assembly file</param>
        public static void StoreProtectedAssembly(String ProductName, String CurrentFilename)
        {
            String usefolder = getProtectedAssemblyFolder(ProductName);


        }
        public static String SecureStringstr(SecureString stringconvert)
        {

            IntPtr bstr = Marshal.SecureStringToBSTR(stringconvert);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);

            }
            finally
            {
                if (bstr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr);

            }

        }
        public static void LoadProtectedAssemblies(String ProductName)
        {
            String SearchFolder = getProtectedAssemblyFolder(ProductName);
            LicenseData ld = new LicenseData(ProductName);
            String genkey = SecureStringstr(GenKey(ld.LicensedName, ld.LicensedOrganization, ProductName));
            //we will use that as the key...
            String ProtectedasmFolder = getProtectedAssemblyFolder(ProductName);

            //make sure the product exists in the dictionary, if not add it. Either way,
            //grab a ref to that dictionary.
            Dictionary<String,Assembly> productdictionary=null;
            if (!ProtectedAssemblies.ContainsKey(ProductName))
                ProtectedAssemblies.Add(ProductName, new Dictionary<string, Assembly>());
                
                
                productdictionary = ProtectedAssemblies[ProductName];
            
            

            

            //iterate on all .CRYPT files.

            DirectoryInfo asmfolder = new DirectoryInfo(ProtectedasmFolder);
            List<Assembly> protectedlist = new List<Assembly>();

            if (asmfolder.Exists)
            {

                foreach (FileInfo cryptfile in asmfolder.GetFiles("*.crypt"))
                {
                    //Read the protected Assembly using the generated key.
                    Assembly protasm = ReadProtectedAssembly(cryptfile.FullName, genkey);
                    if (protasm != null)
                    {
                        //if it loaded, plonk it into the array.
                        productdictionary.Add(Path.GetFileNameWithoutExtension(cryptfile.FullName), protasm);

                    }


                }

            }

        }
        /// <summary>
        /// Finds and invokes a "protected" Function, which is found in one of the Protected Assemblies that will have been 
        /// decrypted and loaded using the Product's Product Key at startup.
        /// </summary>
        /// <param name="Product">Name of the product. Protected Assemblies are indexed by product name.</param>
        /// <param name="AssemblyName">Name of the assembly- this will in fact be the name of the file it was found in, without the extension. (and has no relation to the actual Assembly Name despite the name)</param>
        /// <param name="ClassName">Name of the class/Type</param>
        /// <param name="FunctionName">Method Name to invoke. This must be a static method of the class, not an instance method.</param>
        /// <param name="arglist">Argument List to pass to the routine.</param>
        /// <returns>returns null if the method has no return type, or the value returned by the method.</returns>
        public static Object CallProtectedFunction(String Product, String AssemblyName, String ClassName, String FunctionName, params Object[] arglist)
        {
            //if the key doesn't exist, raise an exception.
            if (!ProtectedAssemblies.ContainsKey(Product)) throw new ArgumentException("Product not found in Dictionary","Product");

            //otherwise, grab that dictionary...
            var protasm = ProtectedAssemblies[Product];
            //if it doesn't have a key with the Assembly name, throw an exception.
            if (!protasm.ContainsKey(AssemblyName)) throw new ArgumentException("Key with given parameter not found", "AssemblyName");
            //otherwise, it does...
            Assembly acquiredasm = protasm[AssemblyName]; 

            //look for the given class.
            Type acquiredtype = null ;
            try
            {
                acquiredtype = acquiredasm.GetType(ClassName);
            }
            catch (Exception GetTypeException)
            {
                throw new ArgumentException("ClassName invalid or threw exception with GetType()", "ClassName", GetTypeException);

            }

            //using acquired type, look for the method "FunctionName". 
            //Note that we can ONLY call static functions.
            Type[] paramtypes;
            if (arglist.Length > 0)
                paramtypes = new Type[arglist.Length];
            else
                paramtypes = Type.EmptyTypes;
                
            
            
            for (int i = 0; i < arglist.Length; i++)
            {

                paramtypes[i] = arglist[i].GetType();

            }


            MethodInfo getmethod = acquiredtype.GetMethod(FunctionName, paramtypes);



            Object result = getmethod.Invoke(null, BindingFlags.Static, null, arglist, Thread.CurrentThread.CurrentCulture);

            return result;

        }

        public static Assembly ReadProtectedAssembly(String Filename, String pwd)
        {
            byte[] pwdbytes = new byte[pwd.Length];
            for (int i = 0; i < pwd.Length; i++)
                pwdbytes[i] = (byte)(pwd[i] % 255);


            System.Security.Cryptography.AesManaged aes = new AesManaged();
            aes.Key = pwdbytes;
            aes.Mode = CipherMode.CBC;
            var decryptor = aes.CreateDecryptor();

            FileStream readassemblyfile = new FileStream(Filename, FileMode.Open);
            try
            {
                System.Security.Cryptography.CryptoStream cs = new CryptoStream(readassemblyfile, decryptor,
                                                                                CryptoStreamMode.Read);

                byte[] readstream = new byte[cs.Length];
                cs.Read(readstream, 0, readstream.Length);
                Assembly readthisassembly = Assembly.Load(readstream);



                return readthisassembly;
            }
            catch (Exception exx)
            {
                Debug.Print("Exception in ReadProtectedAssembly:" + exx.ToString());
                return null;
            }

        }
        public static void EncryptFile(String Filename, String Outputfile, String pwd)
        {
            byte[] pwdbytes = new byte[pwd.Length];
            for (int i = 0; i < pwd.Length; i++)
                pwdbytes[i] = (byte)(pwd[i] % 255);


            System.Security.Cryptography.AesManaged aes = new AesManaged();
            aes.Key = pwdbytes;
            aes.Mode = CipherMode.CBC;
            var Encryptor = aes.CreateEncryptor();

            FileStream readassemblyfile = new FileStream(Filename, FileMode.Open);
            FileStream writeEncryptedfile = new FileStream(Outputfile, FileMode.Create);
            System.Security.Cryptography.CryptoStream cs = new CryptoStream(writeEncryptedfile, Encryptor, CryptoStreamMode.Write);
            byte[] bytesread = new byte[readassemblyfile.Length];
            readassemblyfile.Read(bytesread, 0, (int)readassemblyfile.Length);
            cs.Write(bytesread, 0, bytesread.Length);

            readassemblyfile.Close();
            writeEncryptedfile.Close();
            Debug.Print(Filename + " Encrypted to " + Outputfile + " using pwd:" + pwd);

        }


        public class LicenseData
        {
            public String LicensedName { get; set; }
            public String LicensedOrganization { get; set; }
            public String LicensedProduct { get; set; }
            public String LicenseKey { get; set; }
            public static void DeleteApplicationRegData(String AppName)
            {
                RegistryKey getdata = Registry.CurrentUser.OpenSubKey(getproductkey(AppName));
                getdata.DeleteValue("LicensedName");
                getdata.DeleteValue("LicensedOrganization");
                getdata.DeleteValue("Key");


            }

            public LicenseData(String ProductName)
            {
                LicensedProduct = ProductName;
                RegistryKey getdata = Registry.CurrentUser.OpenSubKey(getproductkey(ProductName));

                try
                {
                    LicensedName = (String)getdata.GetValue("LicensedName");
                    LicensedOrganization = (String)getdata.GetValue("LicensedOrganization");
                    LicenseKey = (String)getdata.GetValue("Key");
                    SecureString localkey = new SecureString();
                    foreach (char character in LicenseKey)
                        localkey.AppendChar(character);
                    //if (LicenseKey != GenKey(LicensedName, LicensedOrganization, ProductName))
                    if(!SecureStringEqual(GenKey(LicensedName,LicensedOrganization,LicensedProduct),localkey))
                    {
                        DeleteApplicationRegData(ProductName);
                        throw new IOException("License Data Key does not match generated key");
                    }

                }
                catch (Exception exx)
                {
                    throw new IOException("License Data Not Found", exx);


                }

            }


        }

        public static char[] validkeycharacters = new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
        '0','1','2','3','4','5','6','7','8','9'};
        public static Random rgenerator = new Random();


        private static int SumString(String sumit)
        {
            int sum = 0;
            foreach (char iterate in sumit)
            {
                sum += iterate;

            }
            return sum;

        }
        private static byte[] StringBytes(String bytesfor)
        {
            byte[] createarray = new byte[bytesfor.Length];
            for (int i = 0; i < bytesfor.Length; i++)
            {
                createarray[i] = (byte)(bytesfor[i] % 255);



            }
            return createarray;



        }
        private static String getproductkey(String productname)
        {

            return "Software\\BASeCamp\\Licenses\\" + productname;

        }

        public static void StoreLicenseData(String ProductName, String LicensedName, String LicensedOrganization, String Key)
        {
            //stores in the registry.
            //key:
            //HKCU/Software/BASeCamp/Licenses/ProductName
            //values "LicensedName,LicensedOrganization,Key"

            RegistryKey rk = Registry.CurrentUser.CreateSubKey(getproductkey(ProductName));
            rk.SetValue("LicensedName", LicensedName, RegistryValueKind.String);
            rk.SetValue("LicensedOrganization", LicensedOrganization, RegistryValueKind.String);
            rk.SetValue("Key", Key, RegistryValueKind.String);



        }
        public static void DoRegister(String ApplicationName,IWin32Window owner)
        {
            frmRegister.DoRegister(ApplicationName, owner);


        }
        private static WebClient usecli = new WebClient();
        private static readonly string Licensephp = "http://satellite/license.php";
        private static readonly string Checkkeyurlformat = Licensephp + "?action=check&key={0}";
        private static string GetURL(String urlload)
        {
            if (urlload == null) throw new ArgumentNullException("urlload");
            StreamReader sreader = new StreamReader(usecli.OpenRead(urlload));
            String returnthis = sreader.ReadToEnd();
            sreader.Close();
            return returnthis;
        }
        public static void NetCheck(String ApplicationName)
        {
            return;
            try
            {
                LicenseData ld = new LicenseData(ApplicationName);
                String loadurl = String.Format(Checkkeyurlformat, ld.LicenseKey);
                String urlresults = GetURL(loadurl);
                urlresults = urlresults.Replace("\n", "");
                bool checkresult;
                if (!bool.TryParse(urlresults, out checkresult))
                    checkresult = false;


                if (!checkresult) //blocked...
                    LicenseData.DeleteApplicationRegData(ApplicationName); //delete our data, this will "unregister" the given application.


            }
            catch (Exception ee)
            {
                //either a WebException or another problem. Ignore it. The application being functional to possibly legit users
                //is more important than preventing illegitimate users from using the program.

            }


        }
        
        public static bool IsLicensed(String ProductName)
        {
            if (!OnWindows()) return true; //free on other operating systems...
            try
            {
                LicenseData ld = new LicenseData(ProductName);
                return true;
            }
            catch (Exception exx)
            {
                return false;

            }


        }
        private static readonly String[] validCallers = new[]
        {
            "BASeBlock",
            "BCMasterKeygen"
        };

        private static void VerifyCaller(Assembly Caller)
        {
            if (Caller != Assembly.GetExecutingAssembly())
            {


                if (!validCallers.Any((w) => w.Equals(Caller.GetName().Name, StringComparison.OrdinalIgnoreCase))) 
                    throw new UnlicensedCodeException(Caller.FullName);

            }

        }
        public static SecureString GenKey(String LicensedName, String LicensedOrganization, String ApplicationName)
        {
            //first, verify the caller.
#if Debug
#else 
            VerifyCaller(Assembly.GetCallingAssembly());
#endif 
            //a product key is in this format:
            //XXXX-XXXX-XXXX-XXXX or 8 bytes.

            //Generate 8 bytes of Random data.
            byte[] randbytes = new byte[16];

            foreach (String hashthis in new String[] { LicensedName, LicensedOrganization, ApplicationName })
            {


                HashStringToBytes(hashthis, ref randbytes);
            }

            rgenerator = new Random(SumString(LicensedName + LicensedOrganization + ApplicationName));

            rgenerator.NextBytes(randbytes);
            SecureString results = new SecureString();

            for (int i = 0; i < randbytes.Length; i++)
            {

                int charactervalue = (int)randbytes[i] % validkeycharacters.Length;
                if (i > 1 && ((i) % 4) == 0) results.AppendChar('-');
                //results += validkeycharacters[charactervalue];
                results.AppendChar(validkeycharacters[charactervalue]);


            }

            return results;

        }


        private static void HashStringToBytes(String stringhash, ref byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                int sumadd = 0;
                for (int j = 0; j < stringhash.Length; j++)
                {
                    int ordinal = stringhash[j];
                    sumadd += ordinal;
                    sumadd += (int)(Math.Log(sumadd) / Math.Log(i));


                }
                bytes[i] = (byte)((((int)bytes[i]) + sumadd) % 255);

            }





        }

    }

    public static class Licensing
    {
        static Licensing()
        {


        }


    }



   
}
