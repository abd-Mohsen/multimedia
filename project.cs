using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Material;
using MaterialSkin.Controls;
using MaterialSkin;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Microsoft.VisualBasic;
using NAudio;
using NAudio.Wave;
using Report;
using System.IO.Compression;

namespace Project
{
    public partial class MainForm : MaterialForm
    {
        PictureBox pictureBox;
        //downscale picturebox    
        public Bitmap? originalImage;

        string? patientName;
        AudioFileReader? audio;
        MaterialButton load1Button = new();
        MaterialButton load2Button = new();
        MaterialButton classifyButton = new();
        MaterialButton enhanceButton = new();
        MaterialButton smoothButton = new();
        MaterialButton textButton = new();
        MaterialButton audioButton = new();
        MaterialButton reportButton = new();
        MaterialButton searchButton = new();
        MaterialButton compressButton = new();
    

        public MainForm()
        {
            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            //BackColor = ColorTranslator.FromHtml("#202123");
            Size = new Size(700, 550);
            Text = "x-ray";

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(300, 300),
                Anchor = AnchorStyles.None,
                BackColor = ColorTranslator.FromHtml("#202123"),
            };
            
            load1Button = new()
            {
                Text = "إدخال صورة",
                Dock = DockStyle.Top
            };
            load1Button.Click += (sender, e) => LoadImage(sender, e, true);
            

            load2Button = new()
            {
                Text = "مقارنة",
                Dock = DockStyle.Top,
                Visible = false,
            };
            load2Button.Click += (sender, e) => LoadImage(sender, e, false);
            

            classifyButton = new()
            {
                Text = "تصنيف",
                Dock = DockStyle.Top,
                Visible = false,
            };
            classifyButton.Click += ClassifyImage;

            enhanceButton = new()
            {
                Text = "تحسين الصورة",
                Dock = DockStyle.Top,
                Visible = false,
            };
            enhanceButton.Click += (sender, e) => EnhanceImage(sender, e, true);

            smoothButton = new()
            {
                Text = "تنعيم الصورة",
                Dock = DockStyle.Top,
                Visible = false,
            };
            smoothButton.Click += (sender, e) => EnhanceImage(sender, e, false);

            textButton = new()
            {
                Text = "إضافة تعليق نصي",
                Dock = DockStyle.Top,
                Visible = false,
            };
            textButton.Click += AddTextToImage;

            audioButton = new()
            {
                Text = "إضافة تعليق صوتي",
                Dock = DockStyle.Top,
                Visible = true,
            };
            audioButton.Click += OpenAudioForm;

            reportButton = new()
            {
                Text = "تقرير",
                Dock = DockStyle.Top,
                Visible = true,
            };
            reportButton.Click += OpenReportForm;

            searchButton = new()
            {
                Text = "بحث",
                Dock = DockStyle.Top,
                Visible = true,
            };
            searchButton.Click += OpenSearchForm;

            compressButton = new()
            {
                Text = "ضغط وحفظ الملفات",
                Dock = DockStyle.Top,
                Visible = false,
            };
            compressButton.Click += CompressAndSave;

        
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
            };
            //layout.RowCount = 7;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            // layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            
            layout.Controls.Add(pictureBox, 0, 0);
            layout.Controls.Add(load1Button, 0, 2);
            layout.Controls.Add(load2Button, 0, 3);
            layout.Controls.Add(classifyButton, 0, 3);
            layout.Controls.Add(enhanceButton, 0, 4);
            //layout.Controls.Add(smoothButton, 0, 4);
            layout.Controls.Add(textButton, 0, 5);
            layout.Controls.Add(audioButton, 0, 5);
            layout.Controls.Add(reportButton, 0, 6);
            layout.Controls.Add(searchButton, 0, 6);
            layout.Controls.Add(compressButton, 0, 7);
            //layout.SetRowSpan(pictureBox2, 20);

            Controls.Add(layout);
        }

        private void LoadImage(object? sender, EventArgs e, bool og)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap selectedImage = new(openFileDialog.FileName);
                    if(og){
                        originalImage = selectedImage;
                        pictureBox.Image = originalImage;
                        load2Button.Visible = true;
                        classifyButton.Visible = true;
                        enhanceButton.Visible = true;
                        smoothButton.Visible = true;
                        textButton.Visible = true;
                        audioButton.Visible = true;
                        reportButton.Visible = true;
                        compressButton.Visible = true;
                        load1Button.Text = "تبديل الصورة";
                        originalImage.Save("temp/selected.jpeg");
                        //ConvertToFormat(originalImage!, PixelFormat.Format16bppGrayScale);
                    }
                    else CompareImages(selectedImage); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("\nخطا في اختيار الصورة" + ex.Message);
                }
            }
            // else{
            //     MessageBox.Show(DialogResult.ToString());
            // }
        }

        private void CompareImages(Bitmap newImage)
        {
            // what if they werent the same size, return false?
            double simillar = 0;
     
            for (int x = 0; x < Math.Min(originalImage!.Width, newImage.Width); x++)
            {
                for (int y = 0; y < Math.Min(originalImage!.Height, newImage.Height); y++)
                {
                    Color first = originalImage.GetPixel(x, y);
                    Color second = newImage.GetPixel(x, y);
                    if (first.R == second.R && first.G == second.G && first.B == second.B) simillar++;
                }
            }
            double percentage = simillar/(originalImage.Width*originalImage.Height)*100;
           
            MessageBox.Show(
                (percentage > 95 ? "لا يوجد تقدم او تأخر ملحوظ في المرض" : "يوجد تغير في حالة المرض") + 
                $"\nنسبة التغير %{100 - percentage:0.00}"
            );
        }

        private void ClassifyImage(object? sender, EventArgs e)
        {
            double totalLuminance = 0;
            for (int x = 0; x < originalImage!.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    double luminance = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0;
                    totalLuminance += luminance;
                }
            }
            double avgLuminance = totalLuminance / (originalImage!.Width*originalImage.Height);
            if(avgLuminance < 85) MessageBox.Show("حالة مرضية خفيفة");
            else if (avgLuminance < 170) MessageBox.Show("حالة مرضية متوسطة");
            else MessageBox.Show("حالة مرضية شديدة");
           // MessageBox.Show(avgLuminance.ToString());
        }

        //not working for all pixxel formats, its blurring the immage rather than sharpening it
        private void EnhanceImage(object? sender, EventArgs e, bool sharp){
            int newWidth = NearestPowerOfTwo(originalImage!.Width);
            int newHeight = NearestPowerOfTwo(originalImage!.Height);

            if(newWidth != originalImage.Width || newHeight != originalImage.Height){
                ResizeBilinear filter1 = new(newWidth, newHeight);
                originalImage = filter1.Apply(originalImage);
            }

            Grayscale filter = new(0.2125, 0.7154, 0.0721);
            Bitmap greyImage = originalImage;

            if(originalImage!.PixelFormat != PixelFormat.Format8bppIndexed){
                greyImage = filter.Apply(originalImage);
            }
           
            //FFT
            ComplexImage complexImage = ComplexImage.FromBitmap(greyImage);
            complexImage.ForwardFourierTransform();

            double[,] filterMask;

            if(sharp){
                filterMask = CreateHighPassFilterMask(complexImage.Width, complexImage.Height, 1);
            } else {
                filterMask = CreateLowPassFilterMask(complexImage.Width, complexImage.Height, 50);
            }
           
            ApplyFilterMask(complexImage, filterMask);

            // IFFT
            complexImage.BackwardFourierTransform();
            pictureBox.Image = complexImage.ToBitmap();
        }

        private void AddTextToImage(object? sender, EventArgs e)
        {
            string input = Interaction.InputBox("أدخل اسم المريض", "الاسم", "");

            // Store the entered name
            if (string.IsNullOrEmpty(input)){
                MessageBox.Show("No name entered!");
                return;
            }
            patientName = input;
            
            Bitmap newImage = originalImage!.Clone(new Rectangle(0, 0, originalImage.Width, originalImage.Height), PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                Font font = new("Arial", 28);
                Brush brush = new SolidBrush(Color.Red);
                PointF position = new(10, 10); // Adjust the position as needed
                graphics.DrawString(patientName, font, brush, position);
            }

            pictureBox.Image = newImage;
        }
        
        private void OpenAudioForm(object? sender, EventArgs e){
            using (var audioForm = new AudioForm())
            {
                if (audioForm.ShowDialog() == DialogResult.OK)
                {
                    var recordedAudio = audioForm.RecordedAudio;
                    if (recordedAudio != null)
                    {
                        // Handle the recorded audio (e.g., save, play, or process it)
                    }
                }
            }
        }

        private void OpenReportForm(object? sender, EventArgs e){
            using (var reportForm = new ReportForm())
            {
                if (reportForm.ShowDialog() == DialogResult.OK)
                {
                    //
                }
            }
        }

        private void OpenSearchForm(object? sender, EventArgs e){
            using (var searchForm = new SearchForm())
            {
                if (searchForm.ShowDialog() == DialogResult.OK)
                {
                    //
                }
            }
        }

        private void CompressAndSave(object? sender, EventArgs e){
            File.SetAttributes("temp", FileAttributes.Normal);
            string directoryPath = "temp";
            string zipFilePath = "C:/Users/ABD/Desktop/res.zip";

            using ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
            string[] files = Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(file));
            }
        }


        static class Program
        {
            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }

        static int NearestPowerOfTwo(int val){
            int power = 1;
            while(power < val) power <<= 1;
            return power;
        }

        static double[,] CreateLowPassFilterMask(int width, int height, double cutoffFreq){
            double[,] filterMask = new double[height,width];

            int centerX = width/2;
            int centerY = height/2;

            for(int y=0; y<height; y++){
                for(int x=0; x<width; x++){
                    double distance = Math.Sqrt((x - centerX)*(x - centerX) + (y - centerY)*(y - centerY));

                    if(distance <= cutoffFreq) filterMask[y,x] = 1;
                    else filterMask[y,x] = 0;
                }
            }
            return filterMask;
        }

        static double[,] CreateHighPassFilterMask(int width, int height, double cutoffFreq){
            double[,] filterMask = new double[height, width];

            int centerX = width / 2;
            int centerY = height / 2;

            for (int y = 0; y < height; y++){
                for (int x = 0; x < width; x++){
                    double distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

                    if (distance <= cutoffFreq) filterMask[y, x] = 0; // Low frequency component
                    else filterMask[y, x] = 1; // High frequency component
                }
            }

            return filterMask;
        }

        static void ApplyFilterMask(ComplexImage complexImage, double[,] filterMask){
            for(int y=0; y<complexImage.Height; y++){
                for(int x=0; x<complexImage.Width; x++){
                    complexImage.Data[y,x] *= filterMask[y,x];
                }
            }
        }

    }
}
