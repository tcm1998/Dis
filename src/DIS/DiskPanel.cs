using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DIS
{
    public partial class DiskPanel : UserControl, ICloneable
    {
        private Point _dragOrigin;
        private Label _titleBar;
        private Button _closeButton;
        private ListView _fileList;
        private Label _freeSpace;
        private LogicalEntity _disk;
        private bool _inUse;
        
        public delegate void DragDropDelegate(object sender, MouseEventArgs e);
        
        [Browsable(true)]
        public event DragDropDelegate Dragging;

        [Browsable(true)]
        public event DragDropDelegate Dropping;


        public Point DragOrigin
        { 
            get
            {
                return _dragOrigin;    
            }           
        }

        public Label TitleBar
        {
            get { return _titleBar; }
        }

        public Button CloseButton
        {
            get { return _closeButton; }
        }

        public Label FreeSpace
        {
            get { return _freeSpace; }
        }

        public ListView FileList
        {
            get { return _fileList; }
        }

        //public DiskImage Reader
        //{
        //    get { return _disk; }
        //}

        public bool IsInUse
        {
            get { return _inUse; }
        }

        private Point DragOffset = Point.Empty;

        public DiskPanel()
        {
            InitializeComponent();
            _titleBar = lblDiskTitle;
            _closeButton = btnClose;
            _freeSpace = lblFree;
            _fileList = lstFiles;
        }
        
        private void lblDiskName_MouseDown(object sender, MouseEventArgs e)
        {            
            if (e.Button == MouseButtons.Left)
            {
                DragOffset = new Point(e.X, e.Y);
                _dragOrigin = new Point(this.Location.X, this.Location.Y);
            }    
        }

        private void lblDiskName_MouseMove(object sender, MouseEventArgs e)
        {            
            if (DragOffset != Point.Empty)
            {
                this.BorderStyle = BorderStyle.FixedSingle;
                Point newLocation = this.Location;
                newLocation.X += e.X - DragOffset.X;
                newLocation.Y += e.Y - DragOffset.Y;
                this.Location = newLocation;
                if (Dragging != null)
                {
                    Dragging(this, e);
                }
            }
            
        }

        private void lblDiskName_MouseUp(object sender, MouseEventArgs e)
        {            
            this.BorderStyle = BorderStyle.None;
            DragOffset = Point.Empty;
            if (Dropping != null)
            {
                Dropping(this, e);
            }
        }

        private void btnClosePane_Click(object sender, EventArgs e)
        {

        }

        private void lstFiles_ItemActivate(object sender, EventArgs e)
        {
            ListView view = (ListView)sender;
            Object tag = view.FocusedItem.Tag;
            LogicalEntity entity = tag as LogicalEntity;
            if (entity != null)
            {
                List<LogicalEntity> items = entity.GetItems();
                DisplayItems(items);
            }


            //BasicReader br = new BasicReader();
//            DiskReader.FileContents contents = _reader.ReadFile(view.FocusedItem.Text);
            //string[] test = br.Translate(data);
            //new FileReader(test).ShowDialog();            
        }

        private void lstFiles_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void General_DragEnter(object sender, DragEventArgs e)
        {

        }

        public object Clone()
        {
            DiskPanel retVal = new DiskPanel();
            retVal.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            retVal.lstFiles = new System.Windows.Forms.ListView();
            retVal.hdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            retVal.CaptionBarLayout = new System.Windows.Forms.TableLayoutPanel();
            retVal.lblDiskTitle = new System.Windows.Forms.Label();
            retVal.btnClose = new System.Windows.Forms.Button();
            retVal.lblFree = new System.Windows.Forms.Label();
            retVal.mainLayout.SuspendLayout();
            retVal.CaptionBarLayout.SuspendLayout();
            retVal.SuspendLayout();
            // 
            // mainLayout
            // 
            retVal.mainLayout.ColumnCount = 1;
            retVal.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            retVal.mainLayout.Controls.Add(retVal.lstFiles, 0, 1);
            retVal.mainLayout.Controls.Add(retVal.CaptionBarLayout, 0, 0);
            retVal.mainLayout.Controls.Add(retVal.lblFree, 0, 2);
            retVal.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            retVal.mainLayout.Location = new System.Drawing.Point(0, 0);
            retVal.mainLayout.Name = "mainLayout";
            retVal.mainLayout.RowCount = 3;
            retVal.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            retVal.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            retVal.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            retVal.mainLayout.Size = new System.Drawing.Size(153, 511);
            retVal.mainLayout.TabIndex = 0;
            // 
            // lstFiles
            // 
            retVal.lstFiles.Margin = new System.Windows.Forms.Padding(0);            
            retVal.lstFiles.AllowDrop = true;
            retVal.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            retVal.hdrName});
            retVal.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            retVal.lstFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            retVal.lstFiles.Location = new System.Drawing.Point(3, 26);
            retVal.lstFiles.Name = "lstFiles";
            retVal.lstFiles.Size = new System.Drawing.Size(147, 467);
            retVal.lstFiles.TabIndex = 0;
            retVal.lstFiles.UseCompatibleStateImageBehavior = false;
            retVal.lstFiles.View = System.Windows.Forms.View.Details;
            retVal.lstFiles.ItemActivate += new System.EventHandler(retVal.lstFiles_ItemActivate);
            retVal.lstFiles.DragDrop += new System.Windows.Forms.DragEventHandler(retVal.lstFiles_DragDrop);
            retVal.lstFiles.DragEnter += new System.Windows.Forms.DragEventHandler(retVal.General_DragEnter);
            // 
            // hdrName
            // 
            retVal.hdrName.Text = "Name";
            retVal.hdrName.Width = 132;
            // 
            // CaptionBarLayout
            // 
            retVal.CaptionBarLayout.AutoSize = true;
            retVal.CaptionBarLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            retVal.CaptionBarLayout.ColumnCount = 2;
            retVal.CaptionBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            retVal.CaptionBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            retVal.CaptionBarLayout.Controls.Add(retVal.lblDiskTitle, 0, 0);
            retVal.CaptionBarLayout.Controls.Add(retVal.btnClose, 1, 0);
            retVal.CaptionBarLayout.Location = new System.Drawing.Point(0, 0);
            retVal.CaptionBarLayout.Margin = new System.Windows.Forms.Padding(0);
            retVal.CaptionBarLayout.Name = "CaptionBarLayout";
            retVal.CaptionBarLayout.RowCount = 1;
            retVal.CaptionBarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            retVal.CaptionBarLayout.Size = new System.Drawing.Size(153, 23);
            retVal.CaptionBarLayout.TabIndex = 9;
            // 
            // lblDiskTitle
            // 
            retVal.lblDiskTitle.BackColor = System.Drawing.SystemColors.InactiveCaption;
            retVal.lblDiskTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            retVal.lblDiskTitle.Location = new System.Drawing.Point(0, 0);
            retVal.lblDiskTitle.Margin = new System.Windows.Forms.Padding(0);
            retVal.lblDiskTitle.Name = "lblDiskTitle";
            retVal.lblDiskTitle.Size = new System.Drawing.Size(137, 23);
            retVal.lblDiskTitle.TabIndex = 6;
            retVal.lblDiskTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            retVal.lblDiskTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(retVal.lblDiskName_MouseDown);
            retVal.lblDiskTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(retVal.lblDiskName_MouseMove);
            retVal.lblDiskTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(retVal.lblDiskName_MouseUp);
            // 
            // btnClose
            // 
            retVal.btnClose.Location = new System.Drawing.Point(137, 0);
            retVal.btnClose.Margin = new System.Windows.Forms.Padding(0);
            retVal.btnClose.Name = "btnClose";
            retVal.btnClose.Size = new System.Drawing.Size(16, 20);
            retVal.btnClose.TabIndex = 5;
            retVal.btnClose.Text = "x";
            retVal.btnClose.UseVisualStyleBackColor = true;
            retVal.btnClose.Click += new System.EventHandler(retVal.btnClosePane_Click);
            // 
            // lblFree
            // 
            retVal.lblFree.BackColor = System.Drawing.SystemColors.ActiveCaption;
            retVal.lblFree.Location = new System.Drawing.Point(3, 496);
            retVal.lblFree.Name = "lblFree";
            retVal.lblFree.Size = new System.Drawing.Size(136, 15);
            retVal.lblFree.TabIndex = 4;
            retVal.lblFree.Text = "label1";
            // 
            // DiskPanel
            // 
            retVal.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            retVal.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            retVal.Controls.Add(retVal.mainLayout);
            retVal.Name = "DiskPanel";
            retVal.Size = new System.Drawing.Size(153, 511);
            retVal.mainLayout.ResumeLayout(false);
            retVal.mainLayout.PerformLayout();
            retVal.CaptionBarLayout.ResumeLayout(false);
            retVal.ResumeLayout(false);

            return retVal;            
        }

        private void lblDiskTitle_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void lblDiskTitle_DragDrop(object sender, DragEventArgs e)
        {            
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            HandleSetContent(files[0]);
        }

        public void SetContent(String filename)
        {
            HandleSetContent(filename);
        }

        private void HandleSetContent(String filename)
        {
            _inUse = true;
            DiskFactory.DiskType diskType = DiskFactory.DiskType.UNSPECIFIED;

            if (File.Exists(filename))
            {
                if (new FileInfo(filename).Length > 3000000)
                {
                    diskType = DiskFactory.DiskType.HD;
                }
            }
            _disk = DiskFactory.CreateDisk(filename, diskType);                                   
            List<LogicalEntity> names = _disk.GetItems();
            DisplayItems(names);            
            _titleBar.Text = Path.GetFileName(filename);                      
        }

        private void DisplayItems(List<LogicalEntity> items)
        {
            _fileList.Items.Clear();
            foreach (LogicalEntity entity in items)
            {
                ListViewItem item = new ListViewItem(entity.name);
                item.Tag = entity;
                _fileList.Items.Add(item);
            }    
        }
    }
}
