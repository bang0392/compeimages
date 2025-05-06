using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CompareImages
{
    public partial class Form3 : Form
    {
        private Bitmap originalImage;
        private Bitmap croppedImage;

        public Form3()
        {
            InitializeComponent();
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(openDialog.FileName);
                pictureBox1.Image = originalImage;
                MessageBox.Show("Đã chọn ảnh thành công!");
            }
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Vui lòng chọn ảnh trước khi lưu.");
                return;
            }

            // Cắt ảnh tại đây
            croppedImage = CropImageContent(originalImage);

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "JPEG Image|*.jpg";
            saveDialog.FileName = "cropped.jpg";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                croppedImage.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                MessageBox.Show("Đã cắt và lưu ảnh thành công!");
            }
        }

        private Bitmap CropImageContent(Bitmap original)
        {
            // Chuyển ảnh Bitmap sang Mat
            Mat matImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(original);

            // Chuyển ảnh sang ảnh xám
            Mat gray = new Mat();
            Cv2.CvtColor(matImage, gray, ColorConversionCodes.BGR2GRAY);

            // Áp dụng Canny edge detection để phát hiện biên của đối tượng
            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 100, 200);

            // Tìm kiếm các contour
            Rect boundingBox = GetBoundingBox(edges);

            // Cắt phần ảnh có nội dung
            Mat croppedMat = new Mat(matImage, boundingBox);

            // Chuyển lại Mat thành Bitmap
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(croppedMat);
        }

        private Rect GetBoundingBox(Mat edges)
        {
            // Tìm các contour trong ảnh edges
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            Rect boundingBox = new Rect();
            foreach (var contour in contours)
            {
                // Tìm bounding box của mỗi contour
                var rect = Cv2.BoundingRect(contour);
                // Kết hợp các bounding box nếu có nhiều contour
                boundingBox = boundingBox == null ? rect : boundingBox | rect; 
            }
            return boundingBox;
        }
    }
}
