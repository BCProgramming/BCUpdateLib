using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BASeCamp.Updating;
using Timer=System.Threading.Timer;

namespace BASeCamp.Updating
{
    public partial class frmUpdates : Form
    {
        private enum EUpdateMode
        {
            Update_Full =0, //"Full" Update mode; standard "mode" that shows available updates.
            Update_Immediate=1, //Immediate; used when a UpdateInfo object is passed into the constructor, that update will be downloaded, And if it is an MSI invoked, and this application will quit.
            


        }
        private EUpdateMode mUpdateMode = EUpdateMode.Update_Full;
        private BCUpdate.UpdateInfo immediateupdate;
        private bool terminateonupdate = false;
        private class DrawItemData
        {
            public String StringDraw;
            public Exception DownloadError;
            private long _ByteSpeed = 0;
            private float _PercentComplete = 0;
            public long ByteSpeed { get { return SpeedHistory.Count==0?0:(long)SpeedHistory.Average(); } set { SpeedHistory.Add(value); } }

            public float PercentComplete
            {
                get { return _PercentComplete; }
                set { _PercentComplete = value; }
            }

            public ListViewItem lvwitem;
            public List<long> SpeedHistory = new List<long>();
 
            public DrawItemData(String pStringDraw, Exception pDownloadError,ListViewItem lvwi)
            {
                StringDraw=pStringDraw;
                DownloadError=pDownloadError;
                lvwitem = lvwi;
            }


        }


        private int musegameID=-1;
        public frmUpdates(BCUpdate.UpdateInfo DownloadUpdate):this(DownloadUpdate,true)
        {


        }
        public frmUpdates(BCUpdate.UpdateInfo DownloadUpdate,bool TerminateonUpdate)
        {
            Debug.Print("frmUpdates MUpdateInfo constructor");
            //switch mode to immediate..
            mUpdateMode = EUpdateMode.Update_Immediate;
            immediateupdate = DownloadUpdate;
            InitializeComponent();
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            
        }

        public frmUpdates(int gameID):this()
        {
            musegameID = gameID;


        }

        public frmUpdates():base()
        {
            Debug.Print("frmUpdates Main constructor");
            InitializeComponent();
        }

        private void frmUpdates_Resize(object sender, EventArgs e)
        {
            if (mUpdateMode == EUpdateMode.Update_Full)
            {
                fraavailupdates.Location = new Point(ClientRectangle.Left, ClientRectangle.Top);


                panLower.Location = new Point(0, ClientSize.Height - panLower.Height);
                panLower.Width = ClientRectangle.Width;
                fraavailupdates.Size = new Size(ClientSize.Width, panLower.Top - fraavailupdates.Top);
                

            }


        }

        //lvwSortManager lsorter = null;
        GenericListViewSorter lsorter = null;
        BCUpdate updateobj;
        Dictionary<BCUpdate.UpdateInfo, ListViewItem> lookupinfo = new Dictionary<BCUpdate.UpdateInfo, ListViewItem>();

        private Comparison<ListViewItem> sortproc(ColumnHeader columnobj)
        {
            ColumnHeader header = columnobj;
            if(header.Name=="NAME")
            {
                return (a, b) =>
                {
                    var at = a.Tag as BCUpdate.UpdateInfo;
                    var bt = b.Tag as BCUpdate.UpdateInfo;
                    return at.DlName.CompareTo(bt.DlName);


                };

            }
            //"VERSION"
            if (header.Name =="INSTALLEDVER")
            {
                return (a, b) =>
                    {
                        var at = a.Tag as BCUpdate.UpdateInfo;
                        var bt = b.Tag as BCUpdate.UpdateInfo;
                        return BCUpdate.UpdateInfo.CompareVersions(updateobj.getinstalledVersion(at.dlID), updateobj.getinstalledVersion(bt.dlID));


                    };


            }
            if (header.Name == "VERSION")
            {
                return (a, b) =>
                {
                    var at = a.Tag as BCUpdate.UpdateInfo;
                    var bt = b.Tag as BCUpdate.UpdateInfo;
                    return BCUpdate.UpdateInfo.CompareVersions(at.UpdateVersion, bt.UpdateVersion);


                };




            }
            if (header.Name == "PROGRESS")
            {
                return (a, b) =>
                    {
                        var at = a.Tag as BCUpdate.UpdateInfo;
                        var bt = b.Tag as BCUpdate.UpdateInfo;
                        var dat = at.Tag as DrawItemData;
                        var dbt = bt.Tag as DrawItemData;
                        int usepercenta, usepercentb;
                        int progindex = header.ListView.Columns["PROGRESS"].Index;
                        int.TryParse(a.SubItems[progindex].Text, out usepercenta);
                        int.TryParse(b.SubItems[progindex].Text, out usepercentb);
                        return usepercenta.CompareTo(usepercentb);
                    };

            }
            if (header.Name == "SIZE")
            {
                return (a, b) =>
                    {
                        var at = a.Tag as BCUpdate.UpdateInfo;
                        var bt = b.Tag as BCUpdate.UpdateInfo;
                        return at.FileSize.CompareTo(bt.FileSize);


                    };


            }
            
            return lvwSortManager.CompareDefault;

        }

        private void frmUpdates_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Debug.Print("frmUpdates Loaded");
                switch (mUpdateMode)
                {
                    case EUpdateMode.Update_Full:


                        grpBasicUpdate.Visible = false;
                    tryagain:
                        updateobj = new BCUpdate();
                        lvwUpdates.Items.Clear();
                        lvwUpdates.Columns.Clear();
                        lvwUpdates.Columns.Add("NAME", "Name");
                        lvwUpdates.Columns.Add("INSTALLEDVER", "Installed");
                        lvwUpdates.Columns.Add("VERSION", "Version");
                        lvwUpdates.Columns.Add("PROGRESS", "Download Progress", 128);
                        lvwUpdates.Columns.Add("SIZE", "Size", 128);
                        lvwUpdates.Columns.Add("PATCH", "Patch For", 128);
                        lsorter = new GenericListViewSorter(lvwUpdates, sortproc);
                        lookupinfo.Clear();

                        //queued delegates to call after all other elements are added.
                        //this is used for adding patches last.
                        Queue<Action> DeferPatchItems = new Queue<Action>();
                        var Typelookup = new Dictionary<int, BCUpdate.UpdateInfo>();
                        try
                        {
                            foreach (BCUpdate.UpdateInfo looper in updateobj.LoadedUpdates)
                            {
                                var loopupdate = looper; 
                                //we copy to a local variable because otherwise how a foreach control variable
                                //is closed over can be compiler specific.
                                Dictionary<int, BCUpdate.UpdateInfo> typelookup = Typelookup;
                                loopupdate.Tag = new DrawItemData("", null, null);
                                
                                Action loopbody = (() =>
                                {
                                    Color useForeColor = SystemColors.WindowText;
                                    String usepatchString = "";
                                    if (loopupdate.DownloadFor > 0)
                                    {
                                        if (!typelookup.ContainsKey(loopupdate.DownloadFor))
                                            usepatchString = "<Unknown>";
                                        else
                                        {
                                            var appliedto = typelookup[loopupdate.DownloadFor];
                                            usepatchString = appliedto.DlName;
                                            if (updateobj.getinstalledVersion(appliedto.dlID) == "")
                                            {
                                                useForeColor = SystemColors.GrayText;
                                                usepatchString = "<" + appliedto.DlName + " Not Installed>";
                                            }

                                        }
                                    }
                                        string[] createdstrings = new string[] { loopupdate.DlName, updateobj.getinstalledVersion(loopupdate.dlID), loopupdate.UpdateVersion, "0", ByteSizeFormatter.FormatSize(loopupdate.FileSize) ,usepatchString};

                                        ListViewItem newitem = new ListViewItem(createdstrings);
                                        ((DrawItemData)loopupdate.Tag).lvwitem = newitem;
                                        newitem.Tag = loopupdate;
                                        newitem.ForeColor = useForeColor;
                                        lookupinfo.Add(loopupdate, newitem);
                                        lvwUpdates.Items.Add(newitem);
                                        typelookup.Add(loopupdate.dlID, loopupdate);
                                    
                                });

                                //if we need to defer it, add it to the queue. Otherwise, call it now.
                                if (loopupdate.DownloadFor == 0)
                                {
                                    loopbody();
                                }
                                else
                                {
                                    DeferPatchItems.Enqueue(loopbody);
                                }


                            }

                            while (DeferPatchItems.Any())
                            {
                                DeferPatchItems.Dequeue()();
                            }

                        }
                        catch (Exception except)
                        {
                            switch (
                                MessageBox.Show(
                                    "The Following Exception occured trying to retrieve update information:\n" +
                                    except.ToString(), "Unexpected Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
                                )
                            {
                                case DialogResult.Retry:
                                    goto tryagain;

                                case DialogResult.Cancel:
                                    Close();
                                    break;

                            }

                        }
                        break;
                    case EUpdateMode.Update_Immediate:
                        //resize to Grp
                        Debug.Print("Immediate Update...");
                        //grpBasicUpdate.Location = new Point(ClientRectangle.Left, ClientRectangle.Top);
                        ClientSize = grpBasicUpdate.Size;
                        fraavailupdates.Hide();
                        MinimizeBox = true;
                        terminateonupdate = true;
                        grpBasicUpdate.Visible = true;
                        grpBasicUpdate.BringToFront();
                        this.Invalidate();
                        grpBasicUpdate.Invalidate();
                        grpBasicUpdate.Update();
                        this.Invalidate();
                        this.Update();
                        Debug.Print("grpBasicUpdate.Visible=" + grpBasicUpdate.Visible);
                        immediateupdate.CancelDownload();

                        String downloadresult = immediateupdate.DownloadUpdate(progressroutine, completionroutine);
                        if (updatingItems == null) updatingItems = new List<BCUpdate.UpdateInfo>();
                        updatingItems.Add(immediateupdate);
                        


                        break;
                }

                Cursor.Current = Cursors.Default;
            }
            catch (Exception exx)
            {
                panRefreshing.Visible = true;
                panRefreshing.BringToFront();
                panRefreshing.Location = new Point(0, 0);
                panRefreshing.Size = new Size(ClientSize.Width, panLower.Top);
                lblrefreshing.Text = "An Exception occured retrieving update information.";
                    txtException.Text = exx.ToString();

                    btnDownload.Visible = false;

            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            
            Close();
        }
        private long lastBytesreceived = 0;
        private void updateprogressforitem(ListViewItem UpdateItem, DownloadProgressChangedEventArgs args)
        {
            //updates the progress for one of the items. uses the DrawItemData found in the tag of the stored BCUpdate.UpdateInfo which itself is the tag of the given item.

            float percentage = (float)args.BytesReceived/(float)args.TotalBytesToReceive;
            //long bytespeed = args.BytesReceived - lastBytesreceived;
            //UpdateItem.ProgressEvent = args;
            UpdateItem.SubItems[lvwUpdates.Columns["PROGRESS"].Index].Text = percentage.ToString();
           UpdateItem.SubItems[3].Text = percentage.ToString();
           
                
            

            DrawItemData itemdraw=((DrawItemData)((BCUpdate.UpdateInfo)UpdateItem.Tag).Tag);
            long currspeed = args.BytesReceived - lastBytesreceived;
            if(currspeed>0)
                itemdraw.ByteSpeed=currspeed;
            lastBytesreceived = args.BytesReceived;
            long speedpersecond = interpolatepersecond(DateTime.Now - lastprogress, itemdraw.ByteSpeed);
            String speedshow = ByteSizeFormatter.FormatSize(speedpersecond);
            if (0 < percentage && percentage < 100)
            {
                itemdraw.PercentComplete = percentage;
                itemdraw.StringDraw = String.Format("{0}% ({1}/s)", percentage, speedshow);
            }
            else if (args.BytesReceived==args.TotalBytesToReceive)
            {
                //complete
                if (itemdraw.DownloadError == null)
                {
                    itemdraw.StringDraw = "Ready to Install";
                    itemdraw.PercentComplete = 100;


                }
                else
                {
                    itemdraw.StringDraw = "Error";
                    UpdateItem.ToolTipText = "Error:" + itemdraw.DownloadError.Message;
                }


            }
           
            
            



        }
        private String CreateString(Char Character, int Number)
        {
            StringBuilder buildit = new StringBuilder();
            for (int i = 1; i < Number; i++)
            {
                buildit.Append(Character);


            }
            return buildit.ToString();

        }

        private int numperiods = 1;
        private long prevDownloadAmount = 0;
        DateTime lastprogress = DateTime.Now;
        /// <summary>
        /// Interpolates the speed per second given a timespan and the number of bytes that were downloaded during that time.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private long interpolatepersecond(TimeSpan interval, long amount)
        {
            //we want per-second download speed, so get the percentage of a full second...

            double spanof = (((double)interval.TotalMilliseconds) / 1000);
            long result = (long)(((double)amount) * spanof);
            if (result == 0)
            {
                //Debug.Print("Zero");
            }
            Debug.Print("Interpolate Per Second: Span:" + interval.ToString() + " Amount:" + amount + " Result:" + result);
            return result;

        }

        /// <summary>
        /// predicts the time left on a download.
        /// </summary>
        /// <param name="ByteSpeed">Current or average download speed.</param>
        /// <param name="amountacquired">amount of data downloaded.</param>
        /// <param name="totalSize">Total Size of the download</param>
        /// <returns></returns>
        private TimeSpan PredictETA(long ByteSpeed, long amountacquired, long TotalSize)
        {
            return PredictETA(ByteSpeed, TotalSize - amountacquired);

        }
        /// <summary>
        /// routine that predicts how long a download will complete.
        /// </summary>
        /// <param name="Bytespeed">Current(or average) download speed to use in estimation.</param>
        /// <param name="totalsize">remaining size of the download</param>
        /// <returns></returns>
        private TimeSpan PredictETA(long Bytespeed, long remainingsize)
        {
            //pretty obvious, really- divide remainingsize by Bytespeed and use that as seconds in a TimeSpan.
            try
            {
                return new TimeSpan(0, 0, 0, (int)remainingsize / (int)Bytespeed);
            }
            catch (OverflowException)
            {
                return new TimeSpan(long.MaxValue);


            }

        }
        private String FormatTimeSpan(TimeSpan ts)
        {
            List<String> elements = new List<string>();
            StringBuilder buildit = new StringBuilder();
            if (ts.Days > 1)
                elements.Add(ts.Days.ToString() + " Days");
            else if (ts.Days == 1) elements.Add(ts.Days.ToString() + " Day");
            if (ts.Hours > 1)
                elements.Add(ts.Hours.ToString() + " Hours");
            else if(ts.Hours == 1) elements.Add(ts.Hours.ToString() + " Hour");

            if (ts.Minutes > 1)
                elements.Add(ts.Minutes.ToString() + " Minutes");
            else if (ts.Minutes == 1) elements.Add(ts.Minutes.ToString() + "Minute");

            if (ts.Seconds > 1)
                elements.Add(ts.Seconds.ToString() + " Seconds");
            else if (ts.Seconds == 1) elements.Add(ts.Seconds.ToString() + " Second");

            return String.Join(",", elements.ToArray());



        }
       
        private List<long> AllSamples = new List<long>(); 
        private long lastaveragebyterate = -1;
        private bool isWin7()
        {

            var osverinfo = Environment.OSVersion;
            //Win7 is version 6.1; anything larger supports the API codepack, presumably.
            return (osverinfo.Version > new Version(6, 1)) && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported;


        }
        private float getAverageCompletion()
        {
            double runningtotal = 0;
            if (updatingItems.Count == 0) return 0;
            foreach (var UpdateItem in this.updatingItems)
            {
                
                DrawItemData did = ((DrawItemData)(UpdateItem).Tag);
                runningtotal += did.PercentComplete;



            }
            return (float)(runningtotal / this.updatingItems.Count);
            
        }
        private void progressroutine(BCUpdate.UpdateInfo updateobject, DownloadProgressChangedEventArgs args)
        {
            
            if (mUpdateMode == EUpdateMode.Update_Full)
            {
                ListViewItem upobj = lookupinfo[updateobject];
                
                updateprogressforitem(upobj, args);

                //we want to invalidate the areas we want to repaint.

                lvwUpdates.Invalidate(upobj.SubItems[3].Bounds);        

                lvwUpdates.Update();

                //calculate the average % complete of all items.
                float percentshow = getAverageCompletion();
                String usetitle = String.Format("{0:0.0}% of {1} Items", percentshow, updatingItems.Count);
                Text = usetitle;
                if (percentshow > 0)
                {
                    if (isWin7())
                    {
                        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.
                            SetProgressValue((int)(percentshow), 100);
                    }
                }
            }
            else
            {
                TimeSpan interval = DateTime.Now - lastprogress;

                long currentspeed = args.BytesReceived -prevDownloadAmount;
                //immediate
                if ((DateTime.Now - lastprogress).TotalSeconds >= 1)
                {
                    numperiods++;
                    if (numperiods == 5) numperiods = 1;
                }
                float percentcomplete = (float)(args.BytesReceived) / (float)(args.TotalBytesToReceive);
                float percentshow = Math.Min(percentcomplete * 100,100);
                if (isWin7())
                {
                    Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.
                        SetProgressValue((int)(percentshow), 100);


                }
                Debug.Print("Received " + args.BytesReceived.ToString() + " of " + args.TotalBytesToReceive.ToString() + "(" + percentcomplete + "%" + ")");
                lblImmAction.Text = "Downloading" + CreateString('.', numperiods);
                //place the two sizes in an array...
                long[] sizes = new long[] { args.BytesReceived,args.TotalBytesToReceive};
                long persecondspeed = interpolatepersecond(interval, currentspeed);
                
                String scurrentspeed = persecondspeed>0?ByteSizeFormatter.FormatSize(persecondspeed):"(Please wait...)";
                //call the formatting routine..
                String[] formattedsizes  = ByteSizeFormatter.FormatSizes(sizes).ToArray();

                lblImmProgress.Text = String.Format("{0} of {1} ({2:0.0}%)", formattedsizes[0], formattedsizes[1], percentshow);
                
                if (lastaveragebyterate == -1) lastaveragebyterate = persecondspeed;
                long avgspeed = lastaveragebyterate / 2 + persecondspeed / 2;
                AllSamples.Add(avgspeed);
                avgspeed = (int)(AllSamples.Average());
                lblRemaining.Text = FormatTimeSpan(PredictETA(avgspeed, sizes[0], sizes[1]));
                lblDlRate.Text = ByteSizeFormatter.FormatSize(avgspeed) + "/s";
                Text = String.Format("Updating...{0:0.0}%", percentshow);
                prevDownloadAmount = args.BytesReceived;
                pbarImmediate.Value = (int)(percentshow);
                pbarImmediate.Invoke((MethodInvoker)(() => { pbarImmediate.Invalidate(); pbarImmediate.Update(); }));
                Thread.Sleep(10);
                
            }
        }
        
        List<BCUpdate.UpdateInfo> completeddownloads = new List<BCUpdate.UpdateInfo>();
        private void completionroutine(BCUpdate.UpdateInfo updateobject, AsyncCompletedEventArgs args)
        {
            
            AllSamples = new List<long>();
            if (mUpdateMode == EUpdateMode.Update_Full)
            {
                ListViewItem upobj = lookupinfo[updateobject];
                Debug.Print("Completed download of " + updateobject.DlName);
                if (args.Error != null)
                    ((DrawItemData) updateobject.Tag).DownloadError = args.Error;


                {
                    completeddownloads.Add(updateobject);
                }
                alldownloads--;
                upobj.Checked = false;




                if (alldownloads == 0)
                {
                    Debug.Print("All Downloads Finished.");
                    Text = "Installing...";
                    InstallDownloaded_Thread();


                }
            }
            else
            {
                //Immediate mode update, completed, so start the installer and exit.
                //updateobject.DownloadedFilename 
                lblImmAction.Text = "Installing...";
                string installfile = updateobject.DownloadedFilename;
                try
                {
                    Process executableProgram = Process.Start(installfile);
                    //terminate this one during the update.
                    if(terminateonupdate)
                        Application.Exit();

                }
                catch (Exception p)
                {
                    MessageBox.Show("failed to run update:" + p.Message);


                }

            }

        }
        private void ZipInstall(String installfile)
        {
         


        }
        
        /// <summary>
        /// installs all downloaded updates.
        /// EXE and MSI files will be run directly; .ZIP files will be handled specially (via ZipInstall() which starts out as a stub)
        /// </summary>
        private void InstallDownloaded()
        {
            foreach (BCUpdate.UpdateInfo loopdownloaded in completeddownloads)
            {
                //MessageBox.Show("Downloaded file:" + loopdownloaded.DownloadedFilename);
                String installfile = loopdownloaded.DownloadedFilename;
                String grabextension = Path.GetExtension(installfile).ToUpper();
                DrawItemData updatedrawdata = (DrawItemData)loopdownloaded.Tag;
                if (grabextension == ".EXE" || grabextension == ".MSI")
                {
                    //MessageBox.Show("Running external Program:" + installfile);
                    try
                    {

                        updatedrawdata.StringDraw = "Installing...";
                        this.Invoke((MethodInvoker)(() => lvwUpdates.Invalidate()));
                    Process executableProgram = Process.Start(installfile);
                    //wait for it to complete.

                    
                    updatedrawdata.StringDraw = "Installing...";
                    while (!executableProgram.HasExited)
                    {
                        Thread.Sleep(0);
                    }
                    if (executableProgram.ExitCode != 0)
                    {
                        updatedrawdata.StringDraw = "Install Error: " + executableProgram.ExitCode.ToString();


                    }
                    else
                    {
                        updatedrawdata.StringDraw = "Installed.";

                    }
                    }
                    catch (Exception ex)
                    {

                        updatedrawdata.StringDraw = "Exec Error:" + ex.Message;
                    }
                }
                else if (grabextension == ".ZIP")
                {
                    ZipInstall(installfile);



                }



                try
                {

                    this.Invoke((MethodInvoker)(() => lvwUpdates.Invalidate()));
                }
                catch (InvalidOperationException)
                {
                    return;

                }


            }    



        }
        Thread AsyncInstallthread;
        private void InstallDownloaded_Thread()
        {
            AsyncInstallthread = new Thread(InstallDownloaded);
            AsyncInstallthread.Start();



        }

        int alldownloads = 0;
       

        private void btnDownload_Click(object sender, EventArgs e)
        {

            if (!lvwUpdates.Columns.ContainsKey("PROGRESS"))
            {
                lvwUpdates.Columns.Add("PROGRESS", "Progress");


            }
            foreach (ListViewItem loopitem in lvwUpdates.Items)
            {
                if (loopitem.Checked) alldownloads++;


            }
            completeddownloads = new List<BCUpdate.UpdateInfo>();
            updatingItems = new List<BCUpdate.UpdateInfo>();
            foreach (ListViewItem loopitem in lvwUpdates.Items)
            {
                if (!loopitem.Checked) continue;

                BCUpdate.UpdateInfo upinfo = (BCUpdate.UpdateInfo) loopitem.Tag;
                // MessageBox.Show("DIAG:" + upinfo.ToString());
                upinfo.Tag = new DrawItemData("0%", null,loopitem);
                upinfo.DownloadUpdate(progressroutine, completionroutine);
                updatingItems.Add(upinfo);
            }
            

        }
        private List<BCUpdate.UpdateInfo> updatingItems = null;
        private void lvwUpdates_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            //
        }

        private void lvwUpdates_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //e.DrawDefault = true;
            //
        }
      
        Brush progressbrush;
        Dictionary<ListViewItem,Bitmap> useitemimages = new Dictionary<ListViewItem,Bitmap>();
        private void lvwUpdates_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            //
            ListViewItem thisitem = e.Item;
            BCUpdate.UpdateInfo upinfo = (BCUpdate.UpdateInfo)thisitem.Tag;
            DrawItemData usedrawdata = (DrawItemData)upinfo.Tag;
            //upinfo.DrawSubItem(sender, e)
            
            DrawItemData drawdata = (DrawItemData)upinfo.Tag;
            if (e.ColumnIndex == 3)
            {
              
                StringFormat centeralign = new StringFormat();
                centeralign.Alignment = StringAlignment.Center;
                e.DrawDefault = false;
                e.DrawBackground();
                int usepercent;
                int.TryParse(e.SubItem.Text, out usepercent);
                double percentfraction = (double)usepercent / 100;
                e.Graphics.DrawRectangle(new Pen(Color.Black), e.Bounds.Left+2, e.Bounds.Top+2, e.Bounds.Width - 4, e.Bounds.Height - 4);

                
                ThemePaint.DrawProgress(lvwUpdates.Handle, e.Graphics, e.Bounds, (float)percentfraction, ThemePaint.FILLSTATES.PBFS_NORMAL);
                    


                e.Graphics.DrawString(drawdata.StringDraw, new Font("Consolas", 10), Brushes.Black,
                                      e.Bounds, centeralign);
            }
            else
            {
                e.DrawDefault = true;
            }



        }

        private void fraavailupdates_Resize(object sender, EventArgs e)
        {
            lvwUpdates.Location = new Point(fraavailupdates.ClientRectangle.Left, fraavailupdates.ClientRectangle.Top);
            lvwUpdates.Size = new Size(fraavailupdates.ClientRectangle.Right - lvwUpdates.Location.X,
                fraavailupdates.ClientRectangle.Bottom - lvwUpdates.Location.Y);

        }

        private void StripContext_Opening(object sender, CancelEventArgs e)
        {
            if (lvwUpdates.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            



            }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selitem = lvwUpdates.SelectedItems[0];
                BCUpdate.UpdateInfo upinfo = (BCUpdate.UpdateInfo)selitem.Tag;
                //upinfo.DownloadedFilename 
                String execname = "Explorer.exe";
                
                String parameters = "/" + upinfo.DownloadedFilename.Substring(upinfo.DownloadedFilename.Length-Path.GetFileName(upinfo.DownloadedFilename).Length) + "\"";

                
                parameters += " /select \"" + upinfo.DownloadedFilename + "\"";


        }

        private void lblImmAction_Click(object sender, EventArgs e)
        {

        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmUpdates_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;
            String singleitemformat = "{0} has not finished downloading. Are you sure you want to close?";
            String multiitemformat = "Cancel {0} Updates?";
            //confirm if updates/downloads in progress.
            string fmtresult = "";
            if (updatingItems != null)
            {
                if (updatingItems.Count == 1)
                {
                    fmtresult = String.Format(singleitemformat, updatingItems.First().DlName);


                }
                else if (updatingItems.Count > 1)
                {

                    fmtresult = String.Format(multiitemformat, updatingItems.Count);


                }
            }
            if (fmtresult != "")
            {
                DialogResult res = MessageBox.Show(fmtresult, "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.No)
                    e.Cancel = true;
            }
        }

        private void frmUpdates_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (updatingItems == null) return;
            foreach (var ii in updatingItems)
            {

                ii.CancelDownload();

            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selitem = lvwUpdates.SelectedItems[0];
            BCUpdate.UpdateInfo uinfo = selitem.Tag as BCUpdate.UpdateInfo;
            uinfo.CancelDownload();
        }

        private void lvwUpdates_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView lvwchange = (ListView)sender;
            List<ListViewItem> selecteditems = new List<ListViewItem>();
            foreach (ListViewItem lvix in lvwchange.Items)
            {
                if (lvix.Selected)
                    selecteditems.Add(lvix);


            }

            if (selecteditems.Count == 1)
            {
                ListViewItem selitem = selecteditems.First();
                lvwUpdates.ContextMenuStrip = StripContext;
                BCUpdate.UpdateInfo uinfo = selitem.Tag as BCUpdate.UpdateInfo;
                if (completeddownloads.Contains(uinfo))
                {
                    cancelToolStripMenuItem.Enabled = false;
                    openContainingFolderToolStripMenuItem.Enabled = true;
                }
                else
                {
                    cancelToolStripMenuItem.Enabled = true;
                    openContainingFolderToolStripMenuItem.Enabled = true;

                }

            }
            else
            {
                lvwUpdates.ContextMenuStrip = null;
            }
        }

        private void panLower_Resize(object sender, EventArgs e)
        {

        }

        private void panRefreshing_Resize(object sender, EventArgs e)
        {
            txtException.Location = new Point(0, lblrefreshing.Bottom);
            txtException.Size = new Size(panRefreshing.ClientSize.Width, panRefreshing.ClientSize.Height - lblrefreshing.Top);
        }

        private void panLower_Resize_1(object sender, EventArgs e)
        {
            cmdClose.Location = new Point(panLower.ClientSize.Width - cmdClose.Width - 10, cmdClose.Top);
        }
    }




    public class modListView : ListView
    {

        public modListView()
            : base()
        {
            base.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            base.DoubleBuffered = true;
            
        }
        


    }
    }

