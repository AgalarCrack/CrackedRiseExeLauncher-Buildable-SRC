using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RiseLauncher.Properties;

namespace RiseLauncher
{
    public class FormMain : Form
    {
        public FormMain()
        {
            base.FormBorderStyle = FormBorderStyle.None;
            this.InitializeComponent();
        }

        public static string Base64Encode(string plainText)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        public void updateText(string text)
        {
            try
            {
                bool invokeRequired = this.statusText.InvokeRequired;
                if (invokeRequired)
                {
                    this.statusText.Invoke(new Action(delegate ()
                    {
                        this.statusText.Text = text;
                    }));
                }
                else
                {
                    this.statusText.Text = text;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void start()
        {
            bool datasFromSite = this.getDatasFromSite();
            bool flag = !datasFromSite;
            if (flag)
            {
                bool flag2 = this.loadDatasFromCache();
                bool flag3 = !flag2;
                if (flag3)
                {
                    this.updateText("CraftRise sunucularına bağlanılamıyor.");
                    return;
                }
            }
            this.updateText("Java sürümü kontrol ediliyor...");
            bool flag4 = this.isJavaInstalled();
            bool flag5 = !flag4;
            if (flag5)
            {
                this.updateText("Java yükleniyor...");
                bool flag6 = !this.DownloadJava();
                if (flag6)
                {
                    this.updateText("Java yükleme işlemi başarısız, antivirüs aktif ise kapatın.");
                }
                else
                {
                    this.updateText("Java dosyaları çıkartılıyor...");
                    string cacheLZMAFile = this.getCacheLZMAFile();
                    string cacheLZMAFileZip = this.getCacheLZMAFileZip();
                    bool flag7 = UtilFile.DecompressLZMA(cacheLZMAFile, cacheLZMAFileZip);
                    bool flag8 = !flag7;
                    if (flag8)
                    {
                        this.updateText("Java kurulum işlemi başarısız, antivirüs aktif ise kapatın.");
                    }
                    else
                    {
                        try
                        {
                            ZipFile zipFile = new ZipFile(cacheLZMAFileZip);
                            zipFile.ExtractAll(UtilJava.getJavaFolderPath());
                            zipFile.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            this.updateText("Java kurulum işlemi başarısız, antivirüs aktif ise kapatın.");
                            return;
                        }
                        bool flag9 = File.Exists(this.getCacheLZMAFile());
                        if (flag9)
                        {
                            File.Delete(this.getCacheLZMAFile());
                        }
                        bool flag10 = File.Exists(this.getCacheLZMAFileZip());
                        if (flag10)
                        {
                            File.Delete(this.getCacheLZMAFileZip());
                        }
                        this.updateText("Java kuruldu, sürüm kontrolü yapılıyor...");
                        string currentJavaVersion = UtilJava.getCurrentJavaVersion();
                        string mojangJavaVersion = this.getMojangJavaVersion();
                        bool flag11 = currentJavaVersion == null || !currentJavaVersion.Equals(mojangJavaVersion);
                        if (flag11)
                        {
                            this.updateText("Sürüm kontrolü başarısız, sürüm geçersiz. (007)");
                        }
                        this.checkLauncherFile();
                    }
                }
            }
            else
            {
                this.checkLauncherFile();
            }
        }

        private bool getDatasFromSite()
        {
            bool flag = true;
            int num = 3;
            int num2 = 0;
            string content = this.getContent(FormMain.API_URL);
            while (content == null || string.IsNullOrEmpty(content))
            {
                content = this.getContent(FormMain.API_URL);
                bool flag2 = content == null || string.IsNullOrEmpty(content);
                if (!flag2)
                {
                    flag = true;
                    break;
                }
                try
                {
                    Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                flag = false;
                bool flag3 = num2++ >= num;
                if (flag3)
                {
                    break;
                }
            }
            bool flag4 = !flag;
            bool result;
            if (flag4)
            {
                result = flag;
            }
            else
            {
                try
                {
                    JObject api_JSON = JObject.Parse(content);
                    FormMain.API_JSON = api_JSON;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.ToString());
                    return false;
                }
                string content2 = this.getContent(this.getSelectedJavaURL());
                bool flag5 = content2 == null || string.IsNullOrEmpty(content2);
                if (flag5)
                {
                    result = false;
                }
                else
                {
                    try
                    {
                        JObject mojang_JSON = JObject.Parse(content2);
                        FormMain.MOJANG_JSON = mojang_JSON;
                    }
                    catch (Exception ex3)
                    {
                        Console.WriteLine(ex3.ToString());
                        return false;
                    }
                    this.updateJSONCache(content, content2);
                    result = true;
                }
            }
            return result;
        }

        private bool loadDatasFromCache()
        {
            bool result;
            try
            {
                string text = File.ReadAllText(this.getHashFileCache());
                string text2 = File.ReadAllText(this.getJavaFileCache());
                try
                {
                    JObject api_JSON = JObject.Parse(text);
                    FormMain.API_JSON = api_JSON;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
                try
                {
                    JObject mojang_JSON = JObject.Parse(text2);
                    FormMain.MOJANG_JSON = mojang_JSON;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.ToString());
                    return false;
                }
                result = (FormMain.API_JSON != null && FormMain.MOJANG_JSON != null && FormMain.API_JSON.Count > 0 && FormMain.MOJANG_JSON.Count > 0);
            }
            catch (Exception ex3)
            {
                result = false;
            }
            return result;
        }

        private void updateJSONCache(string hashs, string java)
        {
            try
            {
                try
                {
                    File.Delete(this.getHashFileCache());
                    File.Delete(this.getJavaFileCache());
                }
                catch (Exception ex)
                {
                }
                File.WriteAllText(this.getHashFileCache(), hashs);
                File.WriteAllText(this.getJavaFileCache(), java);
            }
            catch (Exception ex2)
            {
            }
        }

        private void checkLauncherFile()
        {
            this.updateText("Launcher için güncelleme kontrolü yapılıyor...");
            string webLauncherHash = this.getWebLauncherHash();
            string hashFile = UtilFile.getHashFile(this.getLauncherFile());
            bool flag = !hashFile.Equals(webLauncherHash);
            if (flag)
            {
                bool flag2 = this.DownloadLauncherJAR();
                bool flag3 = !flag2;
                if (flag3)
                {
                    this.updateText("Launcher indirilemedi, tekrar deneyin.");
                    return;
                }
            }
            this.startLauncher();
        }

        private void startLauncher()
        {
            this.updateText("Launcher başlatılıyor...");
            int num = 512;
            try
            {
                int ram = UtilRAM.getRam();
                bool flag = ram > 2;
                if (flag)
                {
                    num = ram * 256;
                    bool flag2 = num > 4096;
                    if (flag2)
                    {
                        num = 4096;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            string text = this.getStartArguments();
            try
            {
                text = text.Replace("%selectedRAM%", num.ToString());
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.ToString());
                this.updateText("RAM değerleri güncellenemedi, bir hata mevcut.");
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = UtilJava.getJavaWExePath(),
                    Arguments = text + " -jar launcher.jar launcherStartup",
                    WorkingDirectory = UtilJava.getLauncherFolderPath(),
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex3)
            {
                Console.WriteLine(ex3.ToString());
                this.updateText("Launcher başlatılamadı, tekrar deneyin.");
                return;
            }
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private string getWebLauncherHash()
        {
            JObject jobject = (JObject)FormMain.API_JSON.GetValue("MAIN");
            return (string)jobject.GetValue("launcher.jar");
        }

        private string getWebLauncherURL()
        {
            JObject jobject = (JObject)FormMain.API_JSON.GetValue("WINDOWS_BS");
            return (string)jobject.GetValue("launcherURL");
        }

        public static string getSelectedJavaType()
        {
            JObject jobject = (JObject)FormMain.API_JSON.GetValue("WINDOWS_BS");
            return (string)jobject.GetValue("javaType");
        }

        private string getSelectedJavaURL()
        {
            JObject jobject = (JObject)FormMain.API_JSON.GetValue("WINDOWS_BS");
            return (string)jobject.GetValue("javaURL");
        }

        private string getStartArguments()
        {
            JObject jobject = (JObject)FormMain.API_JSON.GetValue("WINDOWS_BS");
            return (string)jobject.GetValue("startArguments");
        }

        private string getContent(string URL)
        {
            string result;
            try
            {
                WebClient webClient = new WebClient();
                result = webClient.DownloadString(URL);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = null;
            }
            return result;
        }

        private string getMojangJavaURL()
        {
            JObject jobject = (JObject)FormMain.MOJANG_JSON.GetValue("windows");
            JObject jobject2 = (JObject)jobject.GetValue(Environment.Is64BitOperatingSystem ? "64" : "32");
            JObject jobject3 = (JObject)jobject2.GetValue(FormMain.getSelectedJavaType());
            return (string)jobject3.GetValue("url");
        }

        private string getMojangJavaKey()
        {
            JObject jobject = (JObject)FormMain.MOJANG_JSON.GetValue("windows");
            JObject jobject2 = (JObject)jobject.GetValue(Environment.Is64BitOperatingSystem ? "64" : "32");
            JObject jobject3 = (JObject)jobject2.GetValue(FormMain.getSelectedJavaType());
            return (string)jobject3.GetValue("sha1");
        }

        private string getMojangJavaVersion()
        {
            JObject jobject = (JObject)FormMain.MOJANG_JSON.GetValue("windows");
            JObject jobject2 = (JObject)jobject.GetValue(Environment.Is64BitOperatingSystem ? "64" : "32");
            JObject jobject3 = (JObject)jobject2.GetValue(FormMain.getSelectedJavaType());
            return (string)jobject3.GetValue("version");
        }

        private bool isJavaInstalled()
        {
            string mojangJavaVersion = this.getMojangJavaVersion();
            string currentJavaVersion = UtilJava.getCurrentJavaVersion();
            bool flag = currentJavaVersion == null || !currentJavaVersion.Equals(mojangJavaVersion);
            return !flag;
        }

        private string getLauncherFile()
        {
            return UtilJava.getLauncherFolderPath() + "launcher.jar";
        }

        private string getHashFileCache()
        {
            return UtilJava.getLauncherFolderPath() + "launcher_hashs.json";
        }

        private string getJavaFileCache()
        {
            return UtilJava.getLauncherFolderPath() + "launcher_java.json";
        }

        private string getCacheLZMAFile()
        {
            return UtilJava.getLauncherFolderPath() + "java.lzma";
        }

        private string getCacheLZMAFileZip()
        {
            return UtilJava.getLauncherFolderPath() + "java.zip";
        }

        private bool DownloadJava()
        {
            bool result;
            try
            {
                string cacheLZMAFile = this.getCacheLZMAFile();
                try
                {
                    bool flag = File.Exists(cacheLZMAFile);
                    if (flag)
                    {
                        File.Delete(cacheLZMAFile);
                    }
                    bool flag2 = File.Exists(this.getCacheLZMAFileZip());
                    if (flag2)
                    {
                        File.Delete(this.getCacheLZMAFileZip());
                    }
                    DirectoryInfo directory = new DirectoryInfo(UtilJava.getJavaMainFolderPath());
                    directory.ClearFolder();
                    bool flag3 = !Directory.Exists(UtilJava.getJavaMainFolderPath());
                    if (flag3)
                    {
                        Directory.CreateDirectory(UtilJava.getJavaMainFolderPath());
                    }
                    bool flag4 = !Directory.Exists(UtilJava.getJavaFolderPath());
                    if (flag4)
                    {
                        Directory.CreateDirectory(UtilJava.getJavaFolderPath());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                this.updateText("Java indiriliyor...");
                using (WebClient webClient = new WebClient())
                {
                    Uri address = new Uri(this.getMojangJavaURL());
                    webClient.DownloadProgressChanged += this.javaDownload;
                    webClient.DownloadFileCompleted += this.WebClientDownloadCompleted;
                    webClient.DownloadFile(address, cacheLZMAFile);
                    bool flag5 = File.Exists(cacheLZMAFile);
                    if (flag5)
                    {
                        this.updateText("Java indirildi, dosya kontrol ediliyor...");
                        string hashFile = UtilFile.getHashFile(this.getCacheLZMAFile());
                        string mojangJavaKey = this.getMojangJavaKey();
                        bool flag6 = !hashFile.Equals(mojangJavaKey);
                        if (flag6)
                        {
                            this.updateText("Java indirilemedi, lütfen tekrar deneyin.");
                            result = false;
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.ToString());
                result = false;
            }
            return result;
        }

        private bool DownloadLauncherJAR()
        {
            bool result;
            try
            {
                string launcherFile = this.getLauncherFile();
                bool flag = File.Exists(launcherFile);
                if (flag)
                {
                    File.Delete(launcherFile);
                }
                this.updateText("Launcher indiriliyor...");
                using (WebClient webClient = new WebClient())
                {
                    Uri address = new Uri(this.getWebLauncherURL());
                    webClient.DownloadProgressChanged += this.LauncherDownload;
                    webClient.DownloadFileCompleted += this.WebClientDownloadCompleted;
                    webClient.DownloadFile(address, launcherFile);
                    bool flag2 = File.Exists(launcherFile);
                    if (flag2)
                    {
                        this.updateText("Launcher indirildi, dosya kontrol ediliyor...");
                        string hashFile = UtilFile.getHashFile(this.getLauncherFile());
                        string webLauncherHash = this.getWebLauncherHash();
                        bool flag3 = !hashFile.Equals(webLauncherHash);
                        if (flag3)
                        {
                            this.updateText("Launcher indirilemedi, lütfen tekrar deneyin.");
                            result = false;
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = false;
            }
            return result;
        }

        private void javaDownload(object sender, DownloadProgressChangedEventArgs e)
        {
            this.updateText("Java indiriliyor... (%" + e.ProgressPercentage.ToString() + ")");
        }

        private void LauncherDownload(object sender, DownloadProgressChangedEventArgs e)
        {
            this.updateText("Launcher indiriliyor... (%" + e.ProgressPercentage.ToString() + ")");
        }

        private void WebClientDownloadCompleted(object sender, AsyncCompletedEventArgs args)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            bool flag = disposing && this.components != null;
            if (flag)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.statusText = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusText
            // 
            this.statusText.Font = new System.Drawing.Font("Arial", 12F);
            this.statusText.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.statusText.Location = new System.Drawing.Point(12, 277);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(360, 28);
            this.statusText.TabIndex = 0;
            this.statusText.Text = "Java sürümü kontrol ediliyor...";
            this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statusText.UseCompatibleTextRendering = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(112, 61);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(158, 170);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.ClientSize = new System.Drawing.Size(384, 361);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.statusText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CrackedRise Launcher";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        public static string API_URL = "https://client.craftrise.network/api/launcher/hashs.php";

        public static JObject API_JSON;

        public static JObject MOJANG_JSON;

        private IContainer components = null;

        private Label statusText;

        private PictureBox pictureBox1;

    }
}
