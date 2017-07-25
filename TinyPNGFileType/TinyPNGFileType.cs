using PaintDotNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TinyPNGPlugin
{
    public class TinyPNGFileType : FileType
    {
        private readonly string tempFile;

        internal TinyPNGFileType() : base("TinyPNG", FileTypeFlags.SupportsSaving, new string[] { ".png" })
        {
            Random random = new Random();
            string str = Path.Combine(Path.GetTempPath(), "PDN_TinyPNG_");
            do
            {
                this.tempFile = str + random.Next(10000, 99999) + ".png";
            }
            while (File.Exists(this.tempFile));
        }

        ~TinyPNGFileType()
        {
            File.Delete(this.tempFile);
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = Image.FromStream(input))
                return Document.FromImage(image);
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            using (RenderArgs args = new RenderArgs(scratchSurface))
                input.Render(args, true);

            using (Bitmap aliasedBitmap = scratchSurface.CreateAliasedBitmap())
                aliasedBitmap.Save(this.tempFile, ImageFormat.Png);

            string keyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TinyPNG.txt");
            if (!File.Exists(keyPath))
            {
                ErrorMessage("Your \"TinyPNG.txt\" file can not be found.\nPlease ensure you have installed the plugin correctly.");
            }
            else
            {
                string key = string.Empty;
                try
                {
                    key = File.ReadAllText(keyPath).Trim();
                }
                catch
                {
                    ErrorMessage("An error occurred while trying to read your \"TinyPNG.txt\" file.");
                }

                if (key == string.Empty)
                {
                    ErrorMessage("Your \"TinyPNG.txt\" file appears to be empty.\nPlease ensure it contains your API Key.");
                }
                else
                {
                    const string address = "https://api.tinypng.com/shrink";
                    string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes("api:" + key));

                    WebClient webClient = new WebClient();
                    webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + base64String);
                    try
                    {
                        webClient.UploadData(address, File.ReadAllBytes(this.tempFile));
                        webClient.DownloadFile(webClient.ResponseHeaders["Location"], this.tempFile);
                    }
                    catch (WebException ex)
                    {
                        ErrorMessage($"An error occurred while using TinyPNG.\n{ex.Message}");
                    }
                }
            }

            using (FileStream fileStream = new FileStream(this.tempFile, FileMode.Open))
            {
                int num;
                while ((num = fileStream.ReadByte()) != -1)
                    output.WriteByte((byte)num);
            }

            void ErrorMessage(string message)
            {
                MessageBox.Show($"{message}\n\nA regular PNG file will been saved instead.", "TinyPNG", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
