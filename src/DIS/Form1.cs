using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DIS
{
    public partial class Form1 : Form
    {
        int numViews = 1;
        Dictionary<int, DiskPanel> panels = null;
        List<Label> seperators = null;        
        
        private Point RefPoint = Point.Empty;

        public Form1()
        {
            InitializeComponent();           
            RefPoint = diskPanel1.Location;
            panels = new Dictionary<int, DiskPanel>();
            seperators = new List<Label>();
            seperators.Add(lblSeperator);
            Label newSeperator = new Label();
            newSeperator.Size = lblSeperator.Size;
            newSeperator.Location = new Point(lblSeperator.Location.X + 156, lblSeperator.Location.Y);
            newSeperator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            newSeperator.BorderStyle = BorderStyle.FixedSingle;
            newSeperator.BackColor = lblSeperator.BackColor;
            newSeperator.Visible = false;
            seperators.Add(newSeperator);
            splitContainer1.Panel1.Controls.Add(newSeperator);            
            panels.Add(numViews, diskPanel1);            
        }  

        private void General_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void btnAddDisk_Click(object sender, EventArgs e)
        {
            AddDisk();    
        }

        private DiskPanel AddDisk()
        {
            this.Size = new Size(this.Size.Width + 156, this.Size.Height);

            DiskPanel newPanel = (DiskPanel)diskPanel1.Clone();
            newPanel.Dragging += diskPanel1_Dragging;
            newPanel.Dropping += diskPanel1_Dropping;
            newPanel.Location = new Point(RefPoint.X + (numViews * 156), RefPoint.Y);
            newPanel.Size = diskPanel1.Size;
            newPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            btnAddDisk.Location = new Point(btnAddDisk.Location.X + 156, btnAddDisk.Location.Y);
            btnAddFiles.Location = new Point(btnAddFiles.Location.X + 156, btnAddFiles.Location.Y);

            Label newSeperator = new Label();
            newSeperator.Location = new Point(lblSeperator.Location.X + ((numViews+1) * 156), lblSeperator.Location.Y);
            newSeperator.Size = lblSeperator.Size;
            newSeperator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            newSeperator.BackColor = lblSeperator.BackColor;
            newSeperator.BorderStyle = BorderStyle.FixedSingle;
            newSeperator.Visible = false;
            seperators.Add(newSeperator);

            splitContainer1.Panel1.Controls.Add(newPanel);
            splitContainer1.Panel1.Controls.Add(newSeperator);

            numViews++;
            if (numViews >= (Screen.PrimaryScreen.Bounds.Width / 156))
            {
                btnAddDisk.Visible = false;
                this.Size = new Size(this.Size.Width - btnAddDisk.Size.Width, this.Size.Height);
            }
            panels.Add(numViews, newPanel);
            return newPanel;
        }        

        private void btnAddDisk_DragDrop(object sender, DragEventArgs e)
        {            
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            int currentPane = 1;
            int numFiles = files.Length;
            int fileIndex = 0;
            
            while (panels.ContainsKey(currentPane) && (fileIndex < numFiles))
            {
                string filename = files[fileIndex];
                if (!panels[currentPane].IsInUse)
                {
                    panels[currentPane].SetContent(filename);
                    fileIndex++;
                }
                currentPane++;
            }
            while ((fileIndex < numFiles) && (fileIndex < 10))
            {
                string filename = files[fileIndex];
                DiskPanel newPanel = AddDisk();
                newPanel.SetContent(filename);
                fileIndex++;                
            }              
        }    

        private bool IsOverlapped(Control source, Control target)
        { 
            int targetLeft = target.Location.X;
            int targetRight = target.Location.X + target.Size.Width - 1;
            int sourceLeft = source.Location.X;
            int sourceRight = source.Location.X + source.Size.Width -1;
            return (((targetLeft >= sourceLeft) && (targetLeft <= sourceRight)) ||
                   ((targetRight >= sourceLeft) && (targetRight <= sourceRight)));   
        }        

        private void diskPanel1_Dragging(object sender, MouseEventArgs e)
        {
            DiskPanel dragControl = (DiskPanel)sender;
            dragControl.BringToFront();
            foreach (Label sep in seperators)
            {
                sep.Visible = IsOverlapped(dragControl, sep);
            }  
        }

        private void diskPanel1_Dropping(object sender, MouseEventArgs e)
        {
            DiskPanel dragControl = (DiskPanel)sender;

            int found = -1;
            int numSeperators = seperators.Count;
            for (int index = 0; (found == -1) && (index < numSeperators); index++)
            {
                if (IsOverlapped(dragControl, seperators[index]))
                {
                    found = index;
                }
            }
            if (found != -1)
            {
                int numPanel = ((dragControl.DragOrigin.X - RefPoint.X) / 156)+1; 
                if (found < numPanel)
                {
                    for (int i = numPanel; i > (found + 1); i--)
                    {
                        panels[i] = panels[i - 1];
                        panels[i].Location = new Point(RefPoint.X + ((i - 1) * 156), RefPoint.Y);                        
                    }
                    panels[found + 1] = dragControl;
                    dragControl.Location = new Point(RefPoint.X + (found * 156), RefPoint.Y);
                
                }
                else
                {
                    for (int i = numPanel; i < found; i++)
                    {
                        panels[i] = panels[i + 1];
                        panels[i].Location = new Point(RefPoint.X + ((i - 1) * 156), RefPoint.Y);                        
                    }
                    panels[found] = dragControl;
                    dragControl.Location = new Point(RefPoint.X + ((found - 1) * 156), RefPoint.Y);                    
                }
                seperators[found].Visible = false;
            }
            else
            {
                dragControl.Location = dragControl.DragOrigin;
            }               
        }     
    }
}
