using FileServerPublisher.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileServerPublisher
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show("Usage: FileServerPublisher.exe <file path|Directory path>", "FileServerPublisher v1.0", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string path = args[0].Trim();

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                MessageBox.Show("Error: The given path does not exist!\n\nPath: " + path, "FileServerPublisher v1.0", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Share share = FileServer.Publish(path);

            if (share.Valid)
            {
                MessageBox.Show("Public link successfully created!\n\nPath: " + path + "\n\nURL: " + share.URL + "\n\nThe link has been copied to the clipboard.", "FileServerPublisher v1.0", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Clipboard.SetText(share.URL);
                System.Diagnostics.Process.Start(share.URL);
            }
            else
            {
                MessageBox.Show("Error: Unable to create public link!\n\nError Details: " + share.Error.ToString(), "FileServerPublisher v1.0", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new Form1());
        }
    }
}
