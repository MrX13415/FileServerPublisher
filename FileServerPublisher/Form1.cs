using FileServerPublisher.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileServerPublisher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Share share = FileServer.Publish(@"D:\Downloads\LTB.dlc.txt");

            if (share.Valid)
            {
                MessageBox.Show("Done!\nURL: " + share.URL);
                Clipboard.SetText(share.URL);
            } else
            {
                MessageBox.Show("ERROR: " + share.Error.ToString() + "\n\nDetails: " + Marshal.GetLastWin32Error());
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Share share = FileServer.Publish(@"D:\Downloads");

            if (share.Valid)
            {
                MessageBox.Show("Done!\nURL: " + share.URL);
                Clipboard.SetText(share.URL);
            }
            else
            {
                MessageBox.Show("ERROR: " + share.Error.ToString() + "\n\nDetails: " + Marshal.GetLastWin32Error());
            }
        }

    }
}
