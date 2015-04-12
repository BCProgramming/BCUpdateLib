﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BASeCamp.Updating
{

    public static class ListViewExtensions
    {
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public int mask;
            public int cxy;
            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,  // HDI_FORMAT
            };

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            };
        };
        private const int HDM_FIRST = 0x1200;
        private const int LVM_FIRST = 0x1000;
        private const int HDM_GETITEMCOUNT = (HDM_FIRST + 0);
        private const int HDM_SETITEM = (HDM_FIRST + 4);

        private const int LVM_GETHEADER = (LVM_FIRST + 31);
        private const int HDM_GETITEM = (HDM_FIRST + 3);
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);


        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessageHDITEM(IntPtr hWnd, uint Msg, IntPtr wParam, ref HDITEM hdItem);


        public static ListViewItem getSelectedItem(this ListView ListViewControl)
        {
            if (ListViewControl.SelectedItems.Count == 0) return null;
            return ListViewControl.SelectedItems[0];



        }

        //extension method for setting "Sort Icon" of the listview.




        public static void SetSortIcon(this System.Windows.Forms.ListView ListViewControl, int ColumnIndex, System.Windows.Forms.SortOrder Order)
        {
            IntPtr ColumnHeader = SendMessage(ListViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            for (int ColumnNumber = 0; ColumnNumber <= ListViewControl.Columns.Count - 1; ColumnNumber++)
            {
                IntPtr ColumnPtr = new IntPtr(ColumnNumber);
                HDITEM item = new HDITEM();
                item.mask = (int)HDITEM.Mask.Format;
                SendMessageHDITEM(ColumnHeader, HDM_GETITEM, ColumnPtr, ref item);

                if (Order != SortOrder.None && ColumnNumber == ColumnIndex)
                {
                    switch (Order)
                    {
                        case SortOrder.Ascending:
                            item.fmt &= ~(int)HDITEM.Format.SortDown;
                            item.fmt |= (int)HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~(int)HDITEM.Format.SortUp;
                            item.fmt |= (int)HDITEM.Format.SortDown;
                            break;
                    }
                }
                else
                {
                    item.fmt &= ~(int)HDITEM.Format.SortDown & ~(int)HDITEM.Format.SortUp;
                }

                SendMessageHDITEM(ColumnHeader, HDM_SETITEM, ColumnPtr, ref item);
            }
        }
    }
    /// <summary>
    /// GenericSortHandler is used for when the ListView's ListItem's have a special Tag Object.
    /// Each Column is mappable to a Field of that Object, which can be specified here. If that field is IComparable, sorts
    /// on those columns will use that Comparison.
    /// </summary>
    public class GenericSortHandler
    {
        private ListView _mListView = null;
        private String[] ColumnKeys;
        private String[] FieldNames;
        private Dictionary<String, String> FieldNameIndex = new Dictionary<string, string>(); 
        public GenericSortHandler(ListView useList, String[] pColumnKeys, String[] pFieldNames)
        {
            if (useList == null) throw new ArgumentNullException("useList");
            if (pColumnKeys == null) throw new ArgumentNullException("pColumnKeys");
            if (pFieldNames==null) throw new ArgumentNullException("pFieldNames");
            if (pColumnKeys.Length != pFieldNames.Length) throw new ArgumentException("pColumnKeys Argument array must be the same size as the pFieldNames Array.");
            _mListView = useList;
            ColumnKeys = pColumnKeys;
            FieldNames = pFieldNames;
            for (int i = 0; i < ColumnKeys.Length; i++)
            {

                FieldNameIndex.Add(ColumnKeys[i], FieldNames[i]);

            }


        }

        // public delegate Object GetCompareValue(GenericListViewSorter Sorter, String ColumnName, ListViewItem Item);
        public Object CompareValue(GenericListViewSorter pSorter, String ColumnName, ListViewItem Item)
        {
            Object oTag = Item.Tag;
            if (oTag == null) /*hrrrm....*/ return null;
            String propname = FieldNameIndex[ColumnName];
            var ttype = oTag.GetType();

            foreach (FieldInfo fi in ttype.GetFields())
            {
                if (fi.Name.Equals(propname, StringComparison.OrdinalIgnoreCase))
                    return fi.GetValue(oTag);




            }
            foreach (PropertyInfo pi in ttype.GetProperties())
            {
                if (pi.Name.Equals(propname, StringComparison.OrdinalIgnoreCase))
                    return pi.GetValue(oTag,null);


            }

            return null;


        }

    }

    
    public class GenericListViewSorter : IComparer
    {

        //declarations for finding header rect.

        //listview header rect.
        private Rectangle _HeaderRect;

        //delegate called by EnumWindows. In this context, for finding the Header control within the ListView.

        private delegate bool EnumWindowCallBack(IntPtr hwnd, IntPtr lParam);

        //Calls the callback for each child window. In this case, the only child control ought to be the Header.

        [DllImport("user32.dll")]
        private static extern int EnumChildWindows(IntPtr hwndParent, EnumWindowCallBack callbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public override string ToString()
            {
                return String.Format("RECT-Left={0},Top={1},Right={2},Bottom={3}.", Left, Top, Right, Bottom);
            }

        }

        /// <summary>
        /// ListView we are handling. This is sort of self-documenting, but this comment is here just in case it isn't.
        /// </summary>
        private ListView OurListView;
        

        //GetCompareValue: given a columnname and a ListViewItem, should return any more specific type.
        //For example, if Column represents a date value, it would return a DateTime. 
        public delegate Object GetCompareValue(GenericListViewSorter Sorter, String ColumnName, ListViewItem Item);
        //advanced delegate used to retrieve comparison routine given a specific columnheader.
        public delegate Comparison<ListViewItem> GetAdvancedComparer(ColumnHeader forColumn);

        //GetCompareValue and GetAdvancedComparison (or rather, their variables below- CompareValueFunc and _AdvancedCompare) are mutually exclusive.
        //That is, "GetAdvancedComparer" is for "advanced" comparisons.

        private GetCompareValue CompareValueFunc;
        private GetAdvancedComparer _AdvancedCompare;
        private ColumnHeader CurrentSortColumn = null;
        private SortOrder[] SortOrders = new SortOrder[] { SortOrder.None, SortOrder.Ascending, SortOrder.Descending };
        private String[] SortOrderImageKey = new string[] { "CLEAR", "ASCENDING", "DESCENDING" };
        private int CurrSortIndex = 0;

        private ContextMenuStrip _ghoststrip = null; //a "ghost" menu strip created when we discover the listview doesn't have a context menu strip set.
        private ContextMenuStrip _HeaderContextMenuStrip = null; //the header context menu that we will show.
        //callback called via EnumChildWindows to retrieve the rect of the header control of the listview.

        private bool EnumWindowCallback(IntPtr hwnd, IntPtr lParam)
        {
            //determine rect of the header and save it in the private field.
            RECT rct;
            if (!GetWindowRect(hwnd, out rct))
            {
                _HeaderRect = Rectangle.Empty; //probably, the listview isn't in Details mode and therefore is not showing columns.

            }
            else
            {
                Debug.Print("Set HeaderRect..");
                _HeaderRect = new Rectangle(rct.Left, rct.Top, rct.Right - rct.Left, rct.Bottom - rct.Top);
            }
            return false; //cancel enumeration.
        }


        //Method used to get the list of Columns in order of their appearance  (left to right).
        private static ColumnHeader[] GetOrderedHeaders(ListView lv)
        {
            ColumnHeader[] arr = new ColumnHeader[lv.Columns.Count];

            foreach (ColumnHeader loopheader in lv.Columns)
            {
                arr[loopheader.DisplayIndex] = loopheader;


            }

            return arr;

        }
        //used by the "generic" part of the code (not advanced).
        //this returns a string representation of the listview item data in the given column, for comparison.
        private Object GetCompareValue_Default(GenericListViewSorter Sorter, String ColumnName, ListViewItem Item)
        {
            //default just returns the String, for now. Later, add special conditions that detect when something is a valid date. Or something...
            int indexuse = Sorter.OurListView.Columns[ColumnName].Index;
            return Item.SubItems[indexuse].Text;
            

        }
        public GenericListViewSorter(ListView handleListView)
            : this(handleListView, (GetCompareValue)null)
        {



        }
        private Dictionary<ColumnHeader, Comparison<ListViewItem>> AdvancedDictionary = 
            new Dictionary<ColumnHeader, Comparison<ListViewItem>>();
        public GenericListViewSorter(ListView handleListView, GenericSortHandler ghandler) :
            this(handleListView, ghandler.CompareValue)
        {

        }
        public GenericListViewSorter(ListView handleListView, GetAdvancedComparer advancedcompare)
            : this(handleListView, (GetCompareValue)null)
        {
            if (advancedcompare == null) throw new ArgumentNullException("advancedcompare");
            _AdvancedCompare = advancedcompare;
            //initialize with columns...
            foreach (ColumnHeader ch in handleListView.Columns)
            {
                AdvancedDictionary.Add(ch, _AdvancedCompare(ch));


            }
        }
        private Comparison<ListViewItem> getitemcomparer(ColumnHeader forcolumn)
        {
            if (!AdvancedDictionary.ContainsKey(forcolumn))
            {
                AdvancedDictionary.Add(forcolumn, _AdvancedCompare(forcolumn));

            }
                return AdvancedDictionary[forcolumn];



        }
        public GenericListViewSorter(ListView handleListView, GetCompareValue GetCompareRoutine)
        {
            OurListView = handleListView;

            if (GetCompareRoutine != null)
                CompareValueFunc = GetCompareRoutine;
            else
                CompareValueFunc = GetCompareValue_Default;

            handleListView.ColumnClick += new ColumnClickEventHandler(handleListView_ColumnClick);

            //other checks, for headers and whatnot.

            if (handleListView.ContextMenuStrip == null)
            {
                //if it is null, make a new one and add some items to it. We need the Opening Event to fire.
                handleListView.ContextMenuStrip = new ContextMenuStrip();
                handleListView.ContextMenuStrip.Items.Add("GHOST");
                _ghoststrip = handleListView.ContextMenuStrip;
            }
            //set the context menu to our... err, context menu.
            //create and initialize it, first.


            _HeaderContextMenuStrip = new ContextMenuStrip();
            //add a ghost item, so that the Opening Event will fire.
            _HeaderContextMenuStrip.Items.Add("GHOST");
            // _HeaderContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(_HeaderContextMenuStrip_Opening);
            handleListView.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            handleListView.ContextMenuStripChanged += new EventHandler(handleListView_ContextMenuStripChanged);


        }

        void handleListView_ContextMenuStripChanged(object sender, EventArgs e)
        {
            if (OurListView==null || OurListView.ContextMenuStrip == null) return;
            OurListView.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
        }
        private ColumnHeader HeaderAtOffset(ListView textview, int xOffset)
        {

            int sum = 0;
            foreach (ColumnHeader header in GetOrderedHeaders(textview))
            {
                sum += header.Width;
                if (sum > xOffset)
                {


                    return header;
                }


            }

            return null;


        }
        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int X;
            public int Y;

        }
        [DllImport("user32")]
        static extern int GetCursorPos(out POINTAPI papi);
        void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //fired when the listview context menu is displayed. If we discover the click was within 
            //the area of the header, we will show the appropriate menu for that header.
            //otherwise, we will allow the "normal" (client-set) context menu to display (but only if the current contextstrip is not _ghoststrip, otherwise we cancel.

            POINTAPI gapi;
            GetCursorPos(out gapi);

            //var gotposition = Control.MousePosition;
            Point gotposition = new Point(gapi.X, gapi.Y);
            //FIRST: retrieve the headerrect of the control...
            EnumChildWindows(OurListView.Handle, new EnumWindowCallBack(EnumWindowCallback), IntPtr.Zero);
            //if the mouse position is within the rectangle, cancel the display of the normal context menu,
            // and create and populate a new contextmenu based on the column being clicked.
            
            if (_HeaderRect.Contains(gotposition))
            {
                e.Cancel = true; //cancel
                Debug.Print("HeaderRect contains the mouse position...");
                //xoffset is the distance the mouse is from the left side of the header.
                int xoffset = gotposition.X - _HeaderRect.Left;


                ColumnHeader clickedheader = HeaderAtOffset(OurListView, xoffset);

                if (clickedheader != null)
                {
                    //create a context menu for the given header, set the tag of the context menu to the column...

                    _HeaderContextMenuStrip = new ContextMenuStrip();
                    _HeaderContextMenuStrip.Tag = clickedheader;

                    //we want two items: one for sorting in Ascending order, and one for Descending Order.

                    ToolStripMenuItem AscendingHeaderItem = new ToolStripMenuItem(String.Format("Sort Column \"{0}\" Ascending", clickedheader.Text));
                    ToolStripMenuItem DescendingHeaderItem = new ToolStripMenuItem(String.Format("Sort Column \"{0}\" Descending", clickedheader.Text));
                    //if the current sort column is this header, check off the strip item if we are ascending or descending as appropriate.
                    if (CurrentSortColumn == clickedheader)
                    {
                        if (OurListView.Sorting == SortOrder.Ascending)
                        {
                            AscendingHeaderItem.Checked = true;
                            AscendingHeaderItem.Enabled = false; //no reason to select that option if it's already set...
                        }
                        else if (OurListView.Sorting == SortOrder.Descending)
                        {
                            DescendingHeaderItem.Checked = true;
                            DescendingHeaderItem.Enabled = false;
                        }


                    }

                    AscendingHeaderItem.Tag = clickedheader;
                    DescendingHeaderItem.Tag = clickedheader;
                    //set the Event handlers of the two MenuItems.
                    AscendingHeaderItem.Click += new EventHandler(AscendingHeaderItem_Click);
                    DescendingHeaderItem.Click += new EventHandler(DescendingHeaderItem_Click);


                    //add it to the HeaderContextStrip.
                    _HeaderContextMenuStrip.Items.Add(AscendingHeaderItem);
                    _HeaderContextMenuStrip.Items.Add(DescendingHeaderItem);
                    //add a separator...

                    _HeaderContextMenuStrip.Items.Add(new ToolStripSeparator());
                    //also add a submenu that will populate with all elements EXCEPT for the one that was clicked....
                    //iterate through all the columns we know of.
                    foreach (ColumnHeader loopheader in OurListView.Columns)
                    {
                        //if(loopheader==clickedheader)
                        ToolStripMenuItem SortColumnItem = new ToolStripMenuItem("Sort By " + loopheader.Text);

                        if (loopheader == CurrentSortColumn) SortColumnItem.Enabled = false; //disable the item if it's the clicked column.

                        var clonedelement = loopheader;
                        //add a Ascending and Descending element to it.
                        SortColumnItem.DropDown.Items.Add("GHOST");
                        SortColumnItem.DropDownOpening += (sent, eargs) =>
                        {
                            //on dropdown open, clear the dropdown and add ascending and descending elements.
                            ToolStripMenuItem element = (ToolStripMenuItem)sent;
                            element.DropDown.Items.Clear();
                            //add ascending and descending elements.
                            var AddAscending = new ToolStripMenuItem("Ascending");
                            var AddDescending = new ToolStripMenuItem("Descending");
                            AddAscending.Tag = loopheader;
                            AddDescending.Tag = loopheader;

                            if (CurrentSortColumn == loopheader)
                            {
                                if (OurListView.Sorting == SortOrder.Ascending)
                                {
                                    AddAscending.Checked = true;
                                    AddAscending.Enabled = false; //no reason to select that option if it's already set...
                                }
                                else if (OurListView.Sorting == SortOrder.Descending)
                                {
                                    AddDescending.Checked = true;
                                    AddDescending.Enabled = false;
                                }


                            }


                            AddAscending.Click += AscendingHeaderItem_Click;
                            AddDescending.Click += DescendingHeaderItem_Click;
                            element.DropDown.Items.AddRange(new[] { AddAscending, AddDescending });
                        };

                        _HeaderContextMenuStrip.Items.Add(SortColumnItem);

                    }




                    //show the menu.

                    _HeaderContextMenuStrip.Show(gotposition);


                }






            }
            else
            {
                //alow the default menu to show- but only if it isn't the ghost strip.
                if (OurListView.ContextMenuStrip == _ghoststrip)
                    e.Cancel = true;



            }

        }

        void DescendingHeaderItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickeditem = (ToolStripMenuItem)sender;

            //set the currentsort column...
            CurrentSortColumn = (ColumnHeader)clickeditem.Tag;
            //set the sort mode to descending...
            OurListView.Sorting = SortOrder.Descending;

            //set the sort icon.
            OurListView.SetSortIcon(CurrentSortColumn.Index, OurListView.Sorting);
            //and sort...
            OurListView.Sort();

            //throw new NotImplementedException();
        }

        void AscendingHeaderItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickeditem = (ToolStripMenuItem)sender;

            //set the currentsort column...
            CurrentSortColumn = (ColumnHeader)clickeditem.Tag;
            //set the sort mode to descending...
            OurListView.Sorting = SortOrder.Ascending;
            //set the sort icon.
            OurListView.SetSortIcon(CurrentSortColumn.Index, OurListView.Sorting);
            //and sort...
            OurListView.Sort();

        }



        void handleListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //throw new NotImplementedException();
            //First thing is first: is this the same column that was clicked before?
            ColumnHeader clickedcolumn = OurListView.Columns[e.Column];
            if (CurrentSortColumn == null)
            {
                CurrentSortColumn = clickedcolumn;

            }
            if (CurrentSortColumn != clickedcolumn)
            {
                //if not, set the current sort Index to 0...

                // CurrentSortColumn.ImageKey = "CLEAR"; //don't want it to keep the image...

                CurrentSortColumn = clickedcolumn;

            }
            else
            {
                //if it is the same, increment it and take the modulus...

                CurrSortIndex = (CurrSortIndex + 1) % (SortOrders.Length);
                Debug.Print("CurrSortIndex:" + CurrSortIndex);

            }

            //CurrentSortColumn.ImageKey = SortOrderImageKey[CurrSortIndex];

            OurListView.Sorting = SortOrders[CurrSortIndex];
            OurListView.SetSortIcon(CurrentSortColumn.Index, OurListView.Sorting);
            if (CurrSortIndex == 0)
            {
                OurListView.ListViewItemSorter = null;
            }
            else
            {
                OurListView.ListViewItemSorter = this;
            }
            OurListView.Sort();
        }

        #region IComparer Members

        public int Compare(object x, object y)
        {
            ListViewItem a = (ListViewItem)x;
            ListViewItem b = (ListViewItem)y;
            String columnnameuse = CurrentSortColumn.Name;

            if (_AdvancedCompare != null)
            {
                Comparison<ListViewItem> compareresult = _AdvancedCompare(CurrentSortColumn);
                if (OurListView.Sorting == SortOrder.Ascending)
                    return compareresult(a, b);
                else if(OurListView.Sorting==SortOrder.Descending)
                    return compareresult(b, a);
            }
            else
            {
                Object checkA = CompareValueFunc(this, columnnameuse, a);
                Object checkB = CompareValueFunc(this, columnnameuse, b);


                if ((checkA is IComparable) && (checkB is IComparable))
                {
                    if (OurListView.Sorting == SortOrder.Ascending)
                        return ((IComparable)checkA).CompareTo(checkB);
                    else
                    {
                        return ((IComparable)checkB).CompareTo(checkA);
                    }

                }
            }
            return 0;




        }

        #endregion
    }
}
