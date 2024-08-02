using System.Diagnostics;

namespace VLCWithJava
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("java_config.ini"))
            {
                try
                {
                    File.Create("java_config.ini").Dispose();
                    // Write the default values to the ini file
                    IniFile ini = new("java_config.ini");
                    ini.Write("JAVA_HOME", "[vlcdir]\\java", "Config");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("You need to run this program as an Administrator at least once.");
                    Environment.Exit(1);
                }
            }

            // get directory of the current executable
            string currentDir = AppContext.BaseDirectory;

            // Read the ini file
            IniFile iniFile = new("java_config.ini");
            string javaHome = iniFile.Read("JAVA_HOME", "Config").Replace("[vlcdir]", currentDir);
            // set environment variable
            Environment.SetEnvironmentVariable("JAVA_HOME", javaHome);
            Environment.SetEnvironmentVariable("JRE_HOME", javaHome);
            // set the PATH variable (to avoid conflicts with other Java installations, overwrite the PATH variable)
            Environment.SetEnvironmentVariable("PATH", javaHome);

            string[] modifiedArgs = args;

            // check if there's "--started-from-file" in the arguments
            if (args.Length > 0 && args[0] == "--started-from-file")
            {
                string fileProtocol = args[1];
                Console.WriteLine(fileProtocol);
                // does it say "dvd:///" at the beginning?
                if (fileProtocol.StartsWith("dvd:///"))
                {
                    // if it does, let's check if it's actually a blu-ray disc
                    string filePath = fileProtocol.Replace("dvd:///", "");
                    // despite saying 'file', it's actually a directory
                    if (Directory.Exists(filePath))
                    {
                        // check if there's a BDMV folder
                        if (Directory.Exists(Path.Combine(filePath, "BDMV")))
                        {
                            // if there is, it's a blu-ray disc
                            modifiedArgs[1] = fileProtocol.Replace("dvd:///", "bluray:///");
                            Console.WriteLine(modifiedArgs[1]);
                        }
                    }
                }
            }


            // start VLC
            Process vlc = new();
            vlc.StartInfo.FileName = "vlc.exe";
            vlc.StartInfo.Arguments = string.Join(" ", modifiedArgs);
            vlc.Start();
            Environment.Exit(0);
        }
    }
}
