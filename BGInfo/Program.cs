using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace BGInfo
{
    class Program
    {
        [DllImport("user32.dll")] private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void Main(string[] args)
        {
            Conf conf = new Conf(args);
            List<Line> lines = new List<Line>();
            lines.Add(new Line(Environment.MachineName, new Font("Arial", 48, FontStyle.Regular, GraphicsUnit.Pixel)));
            string ips = GetIp();
            if(ips.Length > 0) lines.Add(new Line(ips, new Font("Arial", 24, FontStyle.Regular, GraphicsUnit.Pixel)));
            lines.Add(new Line("\nLogged in as: " + System.Environment.UserName, new Font("Arial", 24, FontStyle.Regular, GraphicsUnit.Pixel)));
            string hash = conf.randomname ? GetHash(System.DateTime.Now.ToLongTimeString() + Environment.UserName) : Environment.UserName + "_background";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + hash + ".bmp";
            try
            {
                CreateBitmapImage(lines).Save(path);
                if (File.Exists(path)) SetBackground(path);
            }
            catch { }
        }

        private static void SetBackground(string path)
        {
            RegistryKey RegKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            RegKey.SetValue("TileWallpaper", "0");
            RegKey.SetValue("WallpaperStyle", "0");
            RegKey.SetValue("Wallpaper", path);
            RegKey.Close();
            SystemParametersInfo(20, 1, path, 1);
        }

        private static Bitmap CreateBitmapImage(List<Line> lines)
        {
            int BitMapHeight = 1;
            int BitMapWidth = 1;
            int StartY = 0;
            foreach(Line line in lines)
            {
                Graphics  linegraphics = Graphics.FromImage(new Bitmap(2, 2));
                line.height = (int)linegraphics.MeasureString(line.text, line.font).Height;
                line.width = (int)linegraphics.MeasureString(line.text, line.font).Width;            
                line.y = StartY > 0 ? StartY : 0;
                StartY += line.height;
                BitMapWidth = line.width > BitMapWidth ? line.width : BitMapWidth;
                BitMapHeight += line.height;
                linegraphics.Dispose();
            }
            Bitmap BitMap = new Bitmap(BitMapWidth, BitMapHeight);
            Graphics graphics = Graphics.FromImage(BitMap);
            graphics.Clear(Color.Black);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            foreach(Line line in lines)
            {
                line.x = (line.width == BitMapWidth) ? 0 : (BitMapWidth - line.width) / 2;
                graphics.DrawString(line.text, line.font, new SolidBrush(Color.White), line.x, line.y, StringFormat.GenericDefault);
            }
            graphics.Flush();
            return (BitMap);
        }

        private static string GetHash(string basestring)
        {
            MD5 md5 = MD5.Create();
            byte[] hashbyte = md5.ComputeHash(Encoding.UTF8.GetBytes(basestring));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < hashbyte.Length; i++)
            {
                sBuilder.Append(hashbyte[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private static string GetIp()
        {
            string text = String.Empty;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                if(adapterProperties.UnicastAddresses.Count > 0)
                {
                    foreach(UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                    {
                        IPAddress address = ip.Address;
                        if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6 && !IPAddress.IsLoopback(address))
                        {
                            if (text != String.Empty) text += "\n";
                            text += address + " /" + ip .PrefixLength.ToString();
                        }    
                    }
                }
            }
            return text;
        }

    }

    public class Line
    {
        public string text;
        public Font font;
        public int height;
        public int width;
        public int x;
        public int y;

        public Line(string argText, Font argFont)
        {
            this.text = argText;
            this.font = argFont;
        }
    }

    public class Conf
    {
        public bool randomname = false;

        public Conf(string[] args)
        {
            foreach(string arg in args)
            {
                if (arg == "/r") this.randomname = true;
            }
        }
    }
}
