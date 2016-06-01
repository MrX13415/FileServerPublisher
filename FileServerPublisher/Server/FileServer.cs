using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace FileServerPublisher.Server
{
    public static class FileServer
    {
        private static string urlTemplate = @"{0}/{1}";   // <GUID>/<linkName>
        private static string _ServerParentUrl = @"https://dl.icelane.net/share";
        private static DirectoryInfo _LocalServerShareDirectory = new DirectoryInfo(@"D:\Web\FileServer\Public\share\");

        public static DirectoryInfo LocalServerShareDirectory
        {
            get { return _LocalServerShareDirectory; }
            set
            {
                if (value.Exists) _LocalServerShareDirectory = value;
            }
        }

        public static string ServerParentUrl
        {
            get { return _ServerParentUrl;  }
            set
            {
                if (value.Trim() == "") return;
                _ServerParentUrl = value.Trim();
                if (_ServerParentUrl.EndsWith("/")) _ServerParentUrl = _ServerParentUrl.Substring(0, _ServerParentUrl.Length - 1);
            }
        }

        public static Share Publish(string path)
        {
            FileInfo file = new FileInfo(path);
            DirectoryInfo dir = new DirectoryInfo(path);
            
            if (dir.Exists) return Publish(dir);

            return Publish(file);
        }

        public static Share Publish(FileInfo file)
        {
            Share share = Share.CreateShare(file);
            return share;
        }

        public static Share Publish(DirectoryInfo dir)
        {
            Share share = Share.CreateShare(dir);
            return share;
        }

        public static DirectoryInfo CreateShareBase(Share share)
        {
            return Directory.CreateDirectory(Path.Combine(LocalServerShareDirectory.FullName, share.Guid.ToString()));
        }

        public static string BuildURL(Guid guid, string shareName)
        {
            string url = String.Format(urlTemplate, guid.ToString(), shareName.Trim());
            url = String.Format("{0}/{1}", ServerParentUrl, url);

            return url;
        }

        public static void SetupDirectoryBrowse(string path)
        {
            /*
                <?xml version="1.0" encoding="UTF - 8"?>
                  < configuration >
                      < system.webServer >
                          < directoryBrowse enabled = "true" />
                       </ system.webServer >
                   </ configuration >
            */

            try
            {
                string config = "";
                config += @"<?xml version=""1.0"" encoding=""UTF-8""?>" + "\r\n";
                config += @"<configuration>" + "\r\n";
                config += @"    <system.webServer>" + "\r\n";
                config += @"        <directoryBrowse enabled=""true"" />" + "\r\n";
                config += @"    </system.webServer>" + "\r\n";
                config += @"</configuration>" + "\r\n";

                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.Write(config);
                }

                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
            }
            catch (Exception)
            {

            }
        }
    }
}

