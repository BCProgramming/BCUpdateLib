using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BASeCamp.Updating
{

    internal static class SortColumnImageListHeader
    {
        public static ImageList Listuse=null;
        public static ImageList getsortimagelist()
        {
            if (Listuse != null) return Listuse;

            Listuse = Createsortimagelist();
            return Listuse;




        }
        private static Bitmap DrawAscending()
        {
            Bitmap drawthis = new Bitmap(16, 16);
            Graphics canvas = Graphics.FromImage(drawthis);
            canvas.Clear(Color.Transparent);
            GraphicsPath usepath = new GraphicsPath();
            usepath.AddPolygon(new Point[]{new Point(0,0),new Point(16,0),new Point(8,16),new Point(0,0),    });
            
            usepath.CloseFigure();
            canvas.FillPath(new SolidBrush(Color.Black), usepath);
            return drawthis;


        }
        private static Bitmap DrawDescending()
        {
            Bitmap drawthis = new Bitmap(16, 16);
            Graphics canvas = Graphics.FromImage(drawthis);
            canvas.Clear(Color.Transparent);
            GraphicsPath usepath = new GraphicsPath();
            usepath.AddPolygon(new Point[] { new Point(0, 16), new Point(16, 16), new Point(8, 0), new Point(0, 16), });

            usepath.CloseFigure();
            canvas.FillPath(new SolidBrush(Color.Black), usepath);
            return drawthis;


        }

        private static ImageList Createsortimagelist()
        {

            ImageList genimagelist = new ImageList();
            genimagelist.ImageSize= new Size(16,16);
            genimagelist.Images.Add("ASC",DrawAscending());
            genimagelist.Images.Add("DESC",DrawDescending());



            return genimagelist;

        }


    }
    //public Comparison<ListViewItem> compare
    
    /// <summary>
    /// lvwSortManager class; given a ListView, hooks events from that listview and sorts the columns appropriately.
    /// </summary>
    /// 
    
    
    public class lvwSortManager:IComparer
    {
        


        public delegate Comparison<ListViewItem> GetSortRoutineProc(ColumnHeader header);
        private GetSortRoutineProc getsorter;
        private ListView mListView=null;
         
        //return -1: ItemA< ItemB
        //return 0 ItemA==ItemB
        //return 1 ItemA> ItemB
        public static int CompareDefault(ListViewItem ItemA, ListViewItem ItemB)
        {
            //default is, well, shite.
            return ItemA.Text.CompareTo(ItemB.Text);


        }
        
        private Comparison<ListViewItem> DefaultGetSortRoutine(ColumnHeader header)
        {
            return CompareDefault;
            


        }
        public lvwSortManager(ListView listviewhandle)
            : this(listviewhandle, null)
        {
            getsorter = DefaultGetSortRoutine;


        }

        public lvwSortManager(ListView listviewhandle, GetSortRoutineProc getsorterroutine)
        {
            getsorter = getsorterroutine;

            mListView=listviewhandle;
            mListView.ColumnClick += new ColumnClickEventHandler(mListView_ColumnClick);
            mListView.ListViewItemSorter =this;
            
        }
        ColumnHeader sortedcol=null;
        private SortOrder nextorder(SortOrder flipthis)
        {
            if (flipthis == SortOrder.Ascending)
                return SortOrder.Descending;
            else if (flipthis == SortOrder.Descending)
                return SortOrder.Ascending;
            

            return SortOrder.Descending;

            
        }

        void mListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            ColumnHeader colclicked = mListView.Columns[e.Column];


            if (colclicked == sortedcol)
                mListView.Sorting = nextorder(mListView.Sorting);
            else
            {
                mListView.Sorting = SortOrder.Descending;
            }
            string iconkeyuse = "";
            switch (mListView.Sorting)
            {
                case SortOrder.Ascending:
                    iconkeyuse = "ASC";
                    break;
                case SortOrder.Descending:
                    iconkeyuse = "DESC";
                    break;

            }

            
            sortedcol = colclicked;
            if (mListView.SmallImageList!=null&& mListView.SmallImageList.Images.ContainsKey(iconkeyuse))
            {
                sortedcol.ImageKey = iconkeyuse;
            }
        

        mListView.Sort();
            Comparison<ListViewItem> sortroutine = getsorter(mListView.Columns[e.Column]);
            
            
            



            //throw new NotImplementedException();
        }







        #region IComparer Members

        public int Compare(object x, object y)
        {

            ListViewItem itema = x as ListViewItem;
            ListViewItem itemb = y as ListViewItem;
            Comparison<ListViewItem> sortroutine = getsorter(sortedcol);
            return sortroutine(itema, itemb);


        }

        #endregion
    }
}
