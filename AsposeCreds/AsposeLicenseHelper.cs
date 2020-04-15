using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWords = Aspose.Words;
namespace AsposeCreds
{
    public class AsposeLicenseHelper
    {
        public static void SetLicense()
        {
            AWords.License license = new AWords.License();
            var licenseFileInfo = GetLicenseFile();
            var licenseFileFullPath = licenseFileInfo.FullName;
            license.SetLicense(licenseFileFullPath);
            Aspose.Cells.License cellsLic = new Aspose.Cells.License();
            cellsLic.SetLicense(licenseFileFullPath);
        }
        private static FileInfo GetLicenseFile()
        {
            var current = Process.GetCurrentProcess().MainModule.FileName;
            var dir = new DirectoryInfo(current);
            var root = dir.Root;
            var licenseDirectory = new DirectoryInfo(Path.Combine(root.FullName, "invisettings"));
            licenseDirectory.Create();
            var settingsFile = new FileInfo(Path.Combine(licenseDirectory.FullName, "Aspose.Total.lic"));
            return settingsFile;
        }
    }
}
