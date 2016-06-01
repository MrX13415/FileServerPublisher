using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FileServerPublisher.Server
{
    public class Share
    {
        public enum ShareType
        {
            FileShare,
            DirectoryShare
        }

        public enum ShareError
        {
            None,
            NotElevated,
            SymbolicLinkCreationFailed,
            SymbolicLinkMissing, 
            ShareBaseMissing, 
            SourcePathNotExist
        }


        public string _Name;
        public string Name
        {
            get { return _Name; }
            private set
            {
                _Name = Path.GetInvalidFileNameChars().Aggregate(value, (current, c) => current.Replace(c.ToString(), string.Empty));
            }
        }

        public string ShareBase { get; private set; }
        public string SourcePath { get; private set; }
        public ShareType Type { get; private set; }

        public Guid Guid { get; private set; }
        public string URL { get; private set; }

        public bool Valid { get; private set; }
        public ShareError Error { get; private set; }


        public string SharePath
        {
            get { return Path.Combine(this.ShareBase, this.Name); }
        }

        private Share(string path, ShareType type)
        {
            this.Guid = Guid.NewGuid();
            this.Type = type;
            this.Valid = false;
            this.Error = ShareError.None;
            this.SourcePath = path;
        }

        public static Share CreateShare(FileInfo file)
        {
            Share share = new Share(file.FullName, ShareType.FileShare);
            share.Name = String.Format("{0}_{1}", file.Directory.Name, file.Name);
            share.ShareBase = FileServer.CreateShareBase(share).FullName;
            share.URL = FileServer.BuildURL(share.Guid, share.Name);

            if (file.Exists)
            {
                if (IsElevated)
                {
                    bool ok = WinAPI.Kernel32.CreateSymbolicLink(share.SharePath, share.SourcePath, WinAPI.Kernel32.SymbolicLink.File);
                    if (ok) share.Check();
                    else share.Error = ShareError.SymbolicLinkCreationFailed;
                }
                else share.Error = ShareError.NotElevated;
            }
            else share.Error = ShareError.SourcePathNotExist;

            // clean ...
            if (!share.Valid) share.Delete();

            return share;
        }

        public static Share CreateShare(DirectoryInfo dir)
        {
            Share share = new Share(dir.FullName, ShareType.DirectoryShare);
            share.Name = String.Format("{0}_{1}", dir.Parent.Name, dir.Name);
            share.ShareBase = FileServer.CreateShareBase(share).FullName;
            share.URL = FileServer.BuildURL(share.Guid, share.Name);

            if (dir.Exists)
            {
                if (IsElevated)
                {
                    bool ok = WinAPI.Kernel32.CreateSymbolicLink(share.SharePath, share.SourcePath, WinAPI.Kernel32.SymbolicLink.Directory);

                    FileServer.SetupDirectoryBrowse(Path.Combine(share.SourcePath, "web.config"));

                    if (ok) share.Check();
                    else share.Error = ShareError.SymbolicLinkCreationFailed;
                }
                else share.Error = ShareError.NotElevated;
            }
            else share.Error = ShareError.SourcePathNotExist;

            // clean ...
            if (!share.Valid) share.Delete();
            
            return share;
        }

        private static bool IsElevated
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public void Delete()
        {
            try
            {
                Directory.Delete(this.ShareBase, true);
            }
            catch { }
        }

        public bool Check()
        {
            this.Valid = false;
            this.Error = ShareError.None;

            if (this.Type == ShareType.FileShare)
            {
                // error?
                if (!Directory.Exists(this.ShareBase)) this.Error = ShareError.ShareBaseMissing;
                else if (!File.Exists(this.SourcePath)) this.Error = ShareError.SourcePathNotExist;
                else if (!File.Exists(this.SharePath)) this.Error = ShareError.SymbolicLinkMissing;
            }

            if (this.Type == ShareType.DirectoryShare)
            {
                // error?
                if (!Directory.Exists(this.ShareBase)) this.Error = ShareError.ShareBaseMissing;
                else if (!Directory.Exists(this.SourcePath)) this.Error = ShareError.SourcePathNotExist;
                else if (!Directory.Exists(this.SharePath)) this.Error = ShareError.SymbolicLinkMissing;
            }

            // success?
            if (this.Error == ShareError.None) this.Valid = true;
            return this.Valid;
        }

    }
}
