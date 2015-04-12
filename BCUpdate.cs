using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using ZipInputStream = Ionic.Zip.ZipInputStream;

namespace BASeCamp.Updating
{
    //URL to retrieve info on all updates...
    //http://bc-programming.com/update.php?action=getupdate
    //returns a list, delimited by $$$; each item is a set of items further delimited by "&&&":
    // DlID = splitvalue(0)
    // Version = splitvalue(1)
    // fullURL = splitvalue(2)
    // DlSummary = splitvalue(3)
    // DocURL = splitvalue(4)
    // FileSize = splitvalue(5)
    // dateuse = splitvalue(6)
    /// DlName = splitvalue(7)
    public class BCUpdate
    {
        private const string updateURL = @"http://bc-programming.com/update.php?action=getupdate";
        private const string updateURLwithID = @"http://bc-programming.com/update.php?action=getupdate&dlid={0}";

        public static Dictionary<String, ApplicationRegData> RegisteredApps =
            new Dictionary<string, ApplicationRegData>();

        private static String sRegAppKey = @"Software\BASeCamp\Update\Registered Applications";
        public List<UpdateInfo> LoadedUpdates = null;
        private WebClient usecli = new WebClient();
        public BCUpdate() : this(true) { }
        public BCUpdate(bool showloadmarquee)
        {
            MarqueeProgress mp = null;
            bool cancelled = false;
            if (showloadmarquee)
            {
                mp = new MarqueeProgress("Update Check", "Checking for Available Update...", (a,b)=>cancelled=true);
                mp.Show();
            }
           //deal with this on a separate thread...
            String urlData=String.Empty;
            Thread temploader = new Thread(() =>
            {
                try { urlData = GetURL(updateURL); }catch{}

                //split at "$$$"

                LoadRegisteredApps();
            }
            );
            temploader.Start();
            temploader.Join();
            
            if (cancelled) throw new TaskCanceledException("Update loading cancelled by user.");
            
            String[] updatelines = urlData.Split(new string[] {"$$$"}, StringSplitOptions.None);
            //loop through each, create a new Update object for each one, and add it to our list.
            LoadedUpdates = new List<UpdateInfo>();
            foreach (String loopupdate in updatelines)
            {
                try
                {
                    UpdateInfo addthis = new UpdateInfo(loopupdate);
                    LoadedUpdates.Add(addthis);
                }
                catch (Exception exx)
                {
                    Debug.Print("Didn't add item for update, " + loopupdate + " Ex:" + exx.ToString());
                }
            }
            if (mp != null) {
                mp.Close();
            mp.Dispose();
            mp = null;


            }
        }
        private static Object LoadRegisteredAppsLock = new Object();
        private static void LoadRegisteredApps()
        {
            lock (LoadRegisteredAppsLock) //make the static method thread-safe.
            {
                RegisteredApps = new Dictionary<string, ApplicationRegData>();
                //as admin look in localmachine, otherwise current user
                RegistryKey RegAppKey = Registry.CurrentUser.OpenSubKey(sRegAppKey);
                //enumerate all subkeys.
                if (RegAppKey != null)
                {
                    foreach (String subkeyname in RegAppKey.GetSubKeyNames())
                    {
                        RegistryKey subkey = RegAppKey.OpenSubKey(subkeyname);
                        String GotAppName = (String)subkey.GetValue("Name", "");
                        int gotGameID = (int)subkey.GetValue("GameID", 0);
                        String gotInstalledVersion = (String)subkey.GetValue("InstalledVersion", "0.0.0.0");
                        String GotEXEPath = (String)subkey.GetValue("EXEPath", "");


                        ApplicationRegData newappdata = new ApplicationRegData(GotAppName, gotGameID, gotInstalledVersion,
                                                                               GotEXEPath);
                        RegisteredApps.Add(GotAppName, newappdata);
                    }
                    RegAppKey.Close();
                }
            }
        }

        public string getUpdateVersion(int gameid)
        {
            try
            {
                return (from x in LoadedUpdates where x.dlID == gameid select x).First().UpdateVersion;
            }
            catch
            {
                return "";
            }
        }

        public String getinstalledVersion(int gameid)
        {
            try
            {
                var resultget = (from x in RegisteredApps where x.Value.GameID == gameid select x).FirstOrDefault();
                if (resultget.Value == null) return "";
                return resultget.Value.InstalledVersion;
            }
            catch
            {
                return "";
            }
        }

        public void DoUpdateCheck(int GameID, bool forceupdate)
        {
            //performs an update check for the given GameID, or forces an update.
        }

        public String CheckUpdate(int GameID)
        {
            //checks for any available update.
            //returns the latest version number of available, or an empty string if no update is found.
            String CurrentVersion = getinstalledVersion(GameID);
            String newversion = "";


            foreach (UpdateInfo loopinfo in LoadedUpdates)
            {
                if (loopinfo.dlID == GameID)
                {
                    newversion = loopinfo.UpdateVersion;
                    break;
                }
            }
            if (CurrentVersion != "" && newversion != "")
            {
                if (UpdateInfo.VersionsChanged(CurrentVersion, newversion) != 0)
                {
                    if (UpdateInfo.IsVersionNewer(CurrentVersion, newversion))
                        return newversion;
                    else
                        return "";
                }
            }
            return "";
        }

        public static void RegisterApplication(String ApplicationName, int GameID, String InstalledVersion,
                                               String EXEPath)
        {
            RegisterApplication(ApplicationName, GameID, InstalledVersion, EXEPath, false);
        }

        public static void RegisterApplication(String ApplicationName, int GameID, String InstalledVersion,
                                               String EXEPath, bool forceupdate)
        {
            //Applications using this library should call this routine at every startup. Basically, BCUpdate tries to keep track of the installed version of all
            //the software that uses it in a central location; in this case, a registry key. This is used later on in comparisons to see what the current installed version is.
            //The problem arises that this information can get outdated- uninstalling a program that uses it, for example, may leave registry data behind.
            //what we do is simply verify that the executable exists as well; if it doesn't exist, we say "not installed." or something.

            RegisteredApps = new Dictionary<string, ApplicationRegData>();
            RegistryKey RegAppKey;
            try
            {
                RegAppKey = Registry.CurrentUser.CreateSubKey(sRegAppKey);
            }
            catch (UnauthorizedAccessException ave)
            {
                //curses.
                throw ave;
            }
            var RegisteredKey = RegAppKey.CreateSubKey(ApplicationName);


            RegisteredKey.SetValue("Name", ApplicationName);
            RegisteredKey.SetValue("GameID", GameID);
            RegisteredKey.SetValue("InstalledVersion", InstalledVersion);
            RegisteredKey.SetValue("EXEPath", EXEPath);


            LoadRegisteredApps();


            //last, check for updates to this app.
            String UpdateText = "An Update is available for {0}- Version {1}.\nUpdate now?";
            String ForceUpdateText = "An Update (for {0}) is being forced to version {1}. Continue?";

            String UpdateWithSpecific = "An Update is available for {0}- Version {1}.\n{2}\nUpdate Now?";
            String ForceUpdateTextSpecific = "An Update (for {0}) is being forced to version {1}.\n{2}\n Continue?";
            BCUpdate updater = new BCUpdate();
            String strid = updater.CheckUpdate(GameID);
            if (forceupdate)
            {
                
                strid = updater.getUpdateVersion(GameID);
                UpdateText = ForceUpdateText;
            }
            if (strid != "")
            {
                BCUpdate.UpdateInfo objupdate =
                     (from n in updater.LoadedUpdates where n.dlID == GameID select n).
                         First
                         ();


                if (objupdate.UpdateSpecific.Length > 0)
                {
                    UpdateText = forceupdate ? ForceUpdateText : UpdateText;

                }
                else
                {
                    UpdateText = forceupdate ? ForceUpdateTextSpecific : UpdateWithSpecific;
                }


                if (DialogResult.Yes ==
                    MessageBox.Show(String.Format(UpdateText, ApplicationName, strid,objupdate.UpdateSpecific), ApplicationName + " Update",
                                    MessageBoxButtons.YesNo))
                {
                 
                    bool updatefinish = false;
                    objupdate.DownloadUpdate((g) => updatefinish = true);

                    //idle loop, then return.
                    while (!updatefinish)
                    {
                        Thread.Sleep(0);
                    }
                    if (objupdate.UpdateSuccessful)
                    {
                        Process.Start(objupdate.DownloadedFilename);
                        Application.Exit();
                    }
                }
            }
        }


        private string GetURL(String urlload)
        {

            StreamReader sreader;
            String returnthis = "" ;
            
            sreader = new StreamReader(usecli.OpenRead(urlload)); 
            returnthis = sreader.ReadToEnd();
            sreader.Close();
            return returnthis;
        }

        public class ApplicationRegData
        {
            public ApplicationRegData(String pApplicationName, int pGameID, String pInstalledVersion, String pEXEPath)
            {
                ApplicationName = pApplicationName;
                GameID = pGameID;
                InstalledVersion = pInstalledVersion;
                EXEPath = pEXEPath;
            }

            public String ApplicationName { get; set; }
            public int GameID { get; set; }
            public String InstalledVersion { get; set; }
            public String EXEPath { get; set; }
        }

        public class UpdateInfo
        {
            public XDocument AdvancedUpdateInformation { get; private set; }
            public delegate void DownloadUpdateCompletedFunction(
                UpdateInfo updatecompleted, System.ComponentModel.AsyncCompletedEventArgs e);

            public delegate void DownloadUpdateProgressChangedFunction(
                UpdateInfo updateobject, DownloadProgressChangedEventArgs e);

            public delegate void ImmediateUpdateCompletedRoutine(UpdateInfo objinfo);

            [Flags]
            public enum compareVersionEnum
            {
                EVersion_Major = 1,
                EVersion_Minor = 2,
                EVersion_Revision = 3,
                EVersion_Build = 4
            }

            private Thread ImmediateUpdate;
            private DownloadUpdateProgressChangedFunction ProgressRoutine;
            public bool UpdateSuccessful = false;
            private String _DlSummary = "";
            private int _DownloadFor = -1;
            private int _Length = -1;
            private long bytespeed = 0;
            private DownloadUpdateCompletedFunction completionRoutine;
            public bool downloadinprogress = false;
            private bool gotbrutesize = false;
            private bool immShown = false;
            private long LastBytesReceived = 0;
            private Brush progressbrush;
            private WebClient usecli = new WebClient();
            public long UpdateSize { get; private set; }



            public UpdateInfo(int GameID)
            {
                //updates with a specific gameID
                String useurl = String.Format(updateURLwithID, GameID);
                String resulttext = GetURL(useurl);
                SetParameters(resulttext);
            }

            internal UpdateInfo(string updatestring)
            {
                //split at "&&&"
                SetParameters(updatestring);
            }

            public int dlID { get; set; }
            public String UpdateVersion { get; set; }
            public String UpdateSpecific { get; set; }
            public String fullURL { get; set; }

            public String DlSummary
            {
                get { return _DlSummary; }

                set
                {
                    _DlSummary = value;
                    if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    {
                        //download the actual summary information.
                        WebClient wcSummary = new WebClient();
                        try
                        {
                            _DlSummary = wcSummary.DownloadString(new Uri(value));
                        }
                        catch (Exception exx)
                        {
                            Debug.Print("Exception downloading Summary:" + exx.ToString());
                        }
                    }
                }
            }

            public String DocURL { get; set; }
            public long FileSize { get; set; }
            public String dateuse { get; set; }
            public String DlName { get; set; }

            public int DownloadFor
            {
                get { return _DownloadFor; }
                set { _DownloadFor = value; }
            }

            public Object Tag { get; set; }
            public String DownloadedFilename { get; set; }
            public DownloadProgressChangedEventArgs ProgressEvent { get; set; }

            public UpdateInfo getDownloadFor()
            {
                if (_DownloadFor <= 0)
                {
                    return null;
                }
                try
                {
                    return new UpdateInfo(_DownloadFor);
                }
                catch
                {
                    return null;
                }
            }



            private static double AscSum(String sumit)
            {
                double DblRunner = 0;
                int currpos = 0;
                foreach (char loopchar in sumit)
                {
                    currpos++;
                    DblRunner += ((int) loopchar)*(sumit.Length - currpos + 1);
                }


                //r.GetValueNames
                return DblRunner;
            }

            public static bool IsVersionNewer(String InstalledVersion, String CheckVersion)
            {
                String[] SplitInstalled = InstalledVersion.Split('.');
                String[] SplitNew = CheckVersion.Split('.');
                int result = 0;
                //loop only through present entries.
                //int loopto = (SplitInstalled.Length < SplitNew.Length) ? SplitInstalled.Length : SplitNew.Length;
                int loopto = Math.Min(SplitInstalled.Length, SplitNew.Length);
                for (int i = 0; i < loopto; i++)
                {
                    if (!int.TryParse(SplitInstalled[i], out result))
                    {
                        SplitInstalled[i] = AscSum(SplitInstalled[i]).ToString();
                    }
                    if (!int.TryParse(SplitNew[i], out result))
                    {
                        SplitNew[i] = AscSum(SplitNew[i]).ToString();
                    }
                    if (Double.Parse(SplitInstalled[i]) > Double.Parse(SplitNew[i]))
                    {
                        return false;
                    }
                    else if (Double.Parse(SplitInstalled[i]) < Double.Parse(SplitNew[i]))
                    {
                        return true;
                    }
                }
                return false;
            }

            public static int CompareVersions(String VersionA, String VersionB)
            {
                String[] SplitA = VersionA.Split('.');
                String[] SplitB = VersionB.Split('.');
                for (int i = 0; i < Math.Min(SplitA.Length, SplitB.Length); i++)
                {
                    int compareA = 0, compareB = 0, result;

                    if (!int.TryParse(SplitA[i], out compareA))
                    {
                        compareA = (int) (AscSum(SplitA[i]));
                    }
                    if (!int.TryParse(SplitB[i], out compareB))
                    {
                        compareB = (int) (AscSum(SplitB[i]));
                    }
                    int resultval = compareA.CompareTo(compareB);
                    if (resultval != 0)
                        return resultval;
                }
                return 0;
            }

            public static compareVersionEnum VersionsChanged(String InstalledVersion, String NewVersion)
            {
                String[] SplitInstalled = InstalledVersion.Split('.');
                String[] SplitNew = NewVersion.Split('.');
                int runchange = 0;
                int result;
                //loop only through present entries.
                //int loopto = (SplitInstalled.Length < SplitNew.Length) ? SplitInstalled.Length : SplitNew.Length;
                int loopto = Math.Min(SplitInstalled.Length, SplitNew.Length);
                for (int i = 0; i < loopto; i++)
                {
                    if (!int.TryParse(SplitInstalled[i], out result))
                    {
                        SplitInstalled[i] = AscSum(SplitInstalled[i]).ToString();
                    }
                    if (!int.TryParse(SplitNew[i], out result))
                    {
                        SplitNew[i] = AscSum(SplitNew[i]).ToString();
                    }
                    if (Double.Parse(SplitInstalled[i]) < Double.Parse(SplitNew[i]))
                    {
                        runchange += (int) Math.Pow(2, i);
                    }
                }
                return (compareVersionEnum) runchange;
            }

     


            [DllImport("kernel32.dll", EntryPoint = "GetTempPathA")]
            private static extern int GetTempPath(int nBufferLength, string lpBuffer);


            private static String GetTempFile(String useextension)
            {
                String tpath = Path.GetTempPath();
                //GetTempPath(1023,tpath);
                tpath = tpath.Replace('\0', ' ').Trim();
                String destfilename = Guid.NewGuid().ToString() + "." + useextension;
                return Path.Combine(tpath, destfilename);
            }

            public override string ToString()
            {
                return "DLID:" + dlID + "\nDLName:" + this.DlName + "\nSummary:" + this.DlSummary + "\nFullURL:" +
                       this.fullURL + "\nDownloadedfilename:" + this.DownloadedFilename;
            }

            private string GetURL(String urlload)
            {
                StreamReader sreader = new StreamReader(usecli.OpenRead(urlload));
                String returnthis = sreader.ReadToEnd();
                sreader.Close();
                return returnthis;
            }

            public void ImmUpdateThread(Object parameter)
            {
                ImmediateUpdateCompletedRoutine icr = parameter as ImmediateUpdateCompletedRoutine;
                frmUpdates fupdate = new frmUpdates(this);
                fupdate.Show();

                immShown = true;
                try
                {
                    while (fupdate.Visible)
                    {
                        Application.DoEvents();
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Unexpected exception:" + e.Message + " in " + e.Source);
                    Debug.Print("unhandled exception:" + e.Message + " in " + e.Source);
                }

                if (icr != null)
                    icr(this);
            }

            public void AbortUpdate()
            {
                if (downloadinprogress)
                {
                    usecli.CancelAsync();
                    ImmediateUpdate.Abort();
                }
            }

            //downloads this update; shows a UI as well.

            public void DownloadUpdate(ImmediateUpdateCompletedRoutine Callback)
            {
                UpdateSuccessful = false;
                ImmediateUpdate = new Thread(ImmUpdateThread);
                //download the update,
                ImmediateUpdate.Start(Callback);
                downloadinprogress = false;
                while (immShown == false)
                {
                    Application.DoEvents();
                }


                //return fupdate.ImmediateFilename;
            }

            public void CancelDownload()
            {
                downloadinprogress = false;
                usecli.CancelAsync();
                UpdateSuccessful = false;
            }

            /// <summary>
            /// Downloads the update file to a temporary location.
            /// </summary>
            /// <returns>the filename the data was/is being downloaded to.</returns>
            public String DownloadUpdate(DownloadUpdateProgressChangedFunction pprogressroutine,
                                         DownloadUpdateCompletedFunction pcompletionroutine)
            {
                //set class level variable to be the two specified update routines
                if (!downloadinprogress)
                {
                    try
                    {
                        UpdateSuccessful = false;
                        LastBytesReceived = 0;
                        bytespeed = 0;
                        downloadinprogress = true;
                        downloadinprogress = true;
                        completionRoutine = pcompletionroutine;
                        ProgressRoutine = pprogressroutine;
                        string useextension = fullURL.Substring(fullURL.LastIndexOf('.') + 1);
                        String usetempfile = GetTempFile(useextension);
                        Debug.Print("using temp file:" + usetempfile);
                        Uri makeuri = new Uri(fullURL);
                        DownloadedFilename = usetempfile;
                        usecli.DownloadFileAsync(makeuri, usetempfile);

                        return usetempfile;
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("Unhandled Exception:" + exc.Message + " Source:" + exc.Source + " stacktrace:" +
                                        exc.StackTrace);
                    }
                }
                return "";
            }

            private void GetFileSizeBrute()
            {
                System.Net.WebRequest req = System.Net.HttpWebRequest.Create(fullURL);
                req.Method = "HEAD";
                using (System.Net.WebResponse resp = req.GetResponse())
                {
                    int ContentLength;
                    if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                    {
                        FileSize = ContentLength;
                    }
                }
            }
            private int GetSizeAndName(String useURL)
            {
                String temppath = Path.Combine(Path.GetTempPath(), "BC_TEMP");
                try
                {
                    System.Net.WebRequest req = System.Net.HttpWebRequest.Create(useURL);
                    req.Method = "HEAD";
                    using (System.Net.WebResponse resp = req.GetResponse())
                    {
                        int ContentLength;
                        if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                        {
                            UpdateSize = ContentLength;
                        }

                        string header_contentDisposition = resp.Headers["content-disposition"];


                        Directory.CreateDirectory(temppath);

                        this.DownloadedFilename = Path.Combine(temppath, new ContentDisposition(header_contentDisposition).FileName);
                        Debug.Print("Downloading to:" + this.DownloadedFilename);
                        //this.DownloadedFile = resp.Headers.Get("content-disposition");

                    }
                }
                catch (Exception exx)
                {

                    //hmm, well, I guess the best we can do is try to parse the URL itself, hoping it refers to a filename.
                    int lastidx = useURL.LastIndexOfAny(new char[] { '\\', '/' });
                    this.DownloadedFilename = Path.Combine(temppath, useURL.Substring(lastidx + 1));
                }
                return 0;
            }

            private void SetParameters(string updatestring)
            {
                String[] splitvalue = updatestring.Split(new string[] {"&&&"}, StringSplitOptions.None);
                int outdlid;
                int.TryParse(splitvalue[0], out outdlid);
                this.dlID = outdlid;
                UpdateVersion = splitvalue[1];

                fullURL = splitvalue[2];
                DlSummary = splitvalue[3];
                DocURL = splitvalue[4];
                int outsize;

                try
                {
                    int.TryParse(splitvalue[5], out outsize);
                    FileSize = outsize;
                }
                catch (FormatException fe)
                {
                    FileSize = 0;
                }
                if (FileSize == 0)
                {
                    GetFileSizeBrute();
                }
                dateuse = splitvalue[6];
                DlName = splitvalue[7];
                int dlfor = -1;
                if (int.TryParse(splitvalue[8], out dlfor))
                {
                    DownloadFor = dlfor;
                }
                UpdateSpecific = splitvalue[9];
                Debug.Print("Online version of " + DlName + " is version " + UpdateVersion + "dlid=" + dlID);

                usecli = new WebClient();
                usecli.DownloadProgressChanged += new DownloadProgressChangedEventHandler(usecli_DownloadProgressChanged);
                usecli.DownloadFileCompleted +=
                    new System.ComponentModel.AsyncCompletedEventHandler(usecli_DownloadFileCompleted);
            }


            private void usecli_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                downloadinprogress = false;
                if (e.Error != null)
                {
                    progressbrush = new SolidBrush(Color.Red);
                    UpdateSuccessful = false;
                }
                else
                {
                    UpdateSuccessful = true;
                    //initialize our completed data.
                    if (Path.GetExtension(this.DownloadedFilename).Equals(".ZIP"))
                    {
                        //a zip file.
                        //check if the zip has an XML file named "update.xml"...
                        using (var zf = new Ionic.Zip.ZipFile(this.DownloadedFilename))
                        {
                            foreach (var iterate in zf.Entries)
                            {
                                if (!iterate.IsDirectory && Path.GetExtension(iterate.FileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                                {
                                    //we found the file.
                                    
                                    //save the file here.
                                    MemoryStream TempOutputStream = new MemoryStream();
                                    iterate.Extract(TempOutputStream);
                                    TempOutputStream.Seek(0, SeekOrigin.Begin);
                                    String readXML = new StreamReader(TempOutputStream).ReadToEnd();
                                    this.AdvancedUpdateInformation = XDocument.Parse(readXML);
                                    Debug.Print("Update for " + this.DlName + " included advanced Update Information.");
                                }
                            }


                        }

                    }



                }
                if (completionRoutine != null)
                    completionRoutine(this, e);
                LastBytesReceived = 0;
                bytespeed = 0;
            }
            DateTime LastProgressEvent = DateTime.Now;
            private void usecli_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                bytespeed = e.BytesReceived - LastBytesReceived;
                DateTime NowTime = DateTime.Now;

                if (LastProgressEvent != NowTime)
                {

                    if (ProgressRoutine != null)
                        ProgressRoutine(this, e);

                    LastBytesReceived = e.BytesReceived;
                    //throw new NotImplementedException();
                    LastProgressEvent = NowTime;
                }

                
            }
        }
    }
}