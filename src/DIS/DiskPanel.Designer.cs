namespace DIS
{
    partial class DiskPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.hdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CaptionBarLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblDiskTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblFree = new System.Windows.Forms.Label();
            this.mainLayout.SuspendLayout();
            this.CaptionBarLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.lstFiles, 0, 1);
            this.mainLayout.Controls.Add(this.CaptionBarLayout, 0, 0);
            this.mainLayout.Controls.Add(this.lblFree, 0, 2);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.Size = new System.Drawing.Size(153, 511);
            this.mainLayout.TabIndex = 0;
            // 
            // lstFiles
            // 
            this.lstFiles.AllowDrop = true;
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrName});
            this.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstFiles.Location = new System.Drawing.Point(0, 23);
            this.lstFiles.Margin = new System.Windows.Forms.Padding(0);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(153, 473);
            this.lstFiles.TabIndex = 0;
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.ItemActivate += new System.EventHandler(this.lstFiles_ItemActivate);
            this.lstFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.lstFiles_DragDrop);
            this.lstFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.General_DragEnter);
            // 
            // hdrName
            // 
            this.hdrName.Text = "Name";
            this.hdrName.Width = 132;
            // 
            // CaptionBarLayout
            // 
            this.CaptionBarLayout.AutoSize = true;
            this.CaptionBarLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CaptionBarLayout.ColumnCount = 2;
            this.CaptionBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CaptionBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.CaptionBarLayout.Controls.Add(this.lblDiskTitle, 0, 0);
            this.CaptionBarLayout.Controls.Add(this.btnClose, 1, 0);
            this.CaptionBarLayout.Location = new System.Drawing.Point(0, 0);
            this.CaptionBarLayout.Margin = new System.Windows.Forms.Padding(0);
            this.CaptionBarLayout.Name = "CaptionBarLayout";
            this.CaptionBarLayout.RowCount = 1;
            this.CaptionBarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.CaptionBarLayout.Size = new System.Drawing.Size(153, 23);
            this.CaptionBarLayout.TabIndex = 9;
            // 
            // lblDiskTitle
            // 
            this.lblDiskTitle.AllowDrop = true;
            this.lblDiskTitle.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.lblDiskTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiskTitle.Location = new System.Drawing.Point(0, 0);
            this.lblDiskTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblDiskTitle.Name = "lblDiskTitle";
            this.lblDiskTitle.Size = new System.Drawing.Size(137, 23);
            this.lblDiskTitle.TabIndex = 6;
            this.lblDiskTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDiskTitle.DragDrop += new System.Windows.Forms.DragEventHandler(this.lblDiskTitle_DragDrop);
            this.lblDiskTitle.DragEnter += new System.Windows.Forms.DragEventHandler(this.lblDiskTitle_DragEnter);
            this.lblDiskTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblDiskName_MouseDown);
            this.lblDiskTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblDiskName_MouseMove);
            this.lblDiskTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblDiskName_MouseUp);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(137, 0);
            this.btnClose.Margin = new System.Windows.Forms.Padding(0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(16, 20);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "x";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClosePane_Click);
            // 
            // lblFree
            // 
            this.lblFree.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblFree.Location = new System.Drawing.Point(3, 496);
            this.lblFree.Name = "lblFree";
            this.lblFree.Size = new System.Drawing.Size(136, 15);
            this.lblFree.TabIndex = 4;
            this.lblFree.Text = "label1";
            // 
            // DiskPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayout);
            this.Name = "DiskPanel";
            this.Size = new System.Drawing.Size(153, 511);
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.CaptionBarLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.TableLayoutPanel CaptionBarLayout;
        private System.Windows.Forms.Label lblDiskTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblFree;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.ColumnHeader hdrName;




    }
}
