using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

// Get The Serial Number of System Drive (where the OS was installed)
class Program
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetVolumeInformation(
        string rootPathName,
        StringBuilder volumeNameBuffer,
        int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        StringBuilder fileSystemNameBuffer,
        int nFileSystemNameSize);

    static void Main()
    {
        var (deviceID, serialNumber) = GetSystemDriveInfo();

        if (deviceID != null && serialNumber != null)
        {
            Console.WriteLine($"System Drive - DeviceID: {deviceID}, Serial Number: {serialNumber}");
        }
        else
        {
            Console.WriteLine("Unable to retrieve information for the system drive.");
        }

        Console.ReadKey();
    }

    static (string, string) GetSystemDriveInfo()
    {
        try
        {
            DriveInfo systemDrive = DriveInfo.GetDrives()
                .FirstOrDefault(drive => drive.DriveType == DriveType.Fixed && IsSystemDrive(drive));

            if (systemDrive != null)
            {
                string deviceID = systemDrive.Name; // Drive letter is used as DeviceID in this case
                string serialNumber = GetDriveSerialNumber(systemDrive);

                return (deviceID, serialNumber);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }





        return (null, null);
    }

    static bool IsSystemDrive(DriveInfo drive)
    {
        // Check if the drive is the system drive by comparing its root directory
        return Path.GetPathRoot(Environment.SystemDirectory).Equals(drive.RootDirectory.FullName, StringComparison.OrdinalIgnoreCase);
    }

    static string GetDriveSerialNumber(DriveInfo drive)
    {
        try
        {
            uint serialNumber;
            uint _, __, ___; // Unused parameters
            
            // Allocate buffer for file system name (max size is 256 characters)
            var fileSystemNameBuffer = new System.Text.StringBuilder(256);

            // Retrieve volume serial number
            if (GetVolumeInformation(drive.RootDirectory.FullName, null, 0, out serialNumber, out _, out ___, fileSystemNameBuffer, fileSystemNameBuffer.Capacity))
            {
                return serialNumber.ToString("X");
            }
        }
        catch (Exception)
        {
            // Handle exceptions if needed
        }

        return null;
    }
}
