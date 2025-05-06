using OpenCvSharp;
using OpenCvSharp.Features2D;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CompareImages
{
    public partial class Form2 : Form
    {
        private Mat originalImage;
        private Mat scannedImage;

        public Form2()
        {
            InitializeComponent();
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxScanned.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btnSelectOriginal_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                originalImage = Cv2.ImRead(openFileDialog.FileName, ImreadModes.Color); // Dùng ảnh màu
                pictureBoxOriginal.Image = new Bitmap(openFileDialog.FileName);
            }
        }

        private void btnSelectScanned_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                scannedImage = Cv2.ImRead(openFileDialog.FileName, ImreadModes.Color); // Dùng ảnh màu
                pictureBoxScanned.Image = new Bitmap(openFileDialog.FileName);
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (originalImage == null || scannedImage == null)
            {
                MessageBox.Show("Vui lòng chọn cả hai ảnh để so sánh!");
                return;
            }

            // Loại bỏ viền ảnh
            Mat processedScanned = RemoveBorder(scannedImage);

            // Xoay ảnh quét nếu cần thiết
            Mat rotatedScanned = CorrectImageOrientation(processedScanned, originalImage);

            // Resize ảnh đã quét cho phù hợp kích thước ảnh gốc đã loại bỏ viền
            Mat resizedScanned = PreprocessImage(rotatedScanned, originalImage.Size());

            // Clone ảnh để vẽ điểm khác biệt
            Mat displayOriginal = originalImage.Clone();
            Mat displayScanned = resizedScanned.Clone();

            // So sánh ảnh
            var (matches, keypoints1, keypoints2) = CompareImages(originalImage, resizedScanned);

            if (matches.Length > 0)
            {
                lblResult.Text = "Giống (có thể có điểm khác)";
            }
            else
            {
                lblResult.Text = "Khác hoàn toàn";
            }

            // Vẽ điểm khác biệt
            DrawDifferences(displayOriginal, displayScanned, matches, keypoints1, keypoints2);

            // Hiển thị ảnh đã xử lý: ảnh gốc và ảnh đã xử lý
            pictureBoxOriginal.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(displayOriginal);
            pictureBoxScanned.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(displayScanned);

            // So sánh pixel và khoanh vùng khác biệt
            HighlightPixelDifferences(displayOriginal, displayScanned, pictureBoxScanned);
        }

        private Mat RemoveBorder(Mat image)
        {
            // Chuyển ảnh sang ảnh xám
            Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

            // Áp dụng threshold để tách biệt phần viền và phần chính
            Mat thresholdImage = new Mat();
            Cv2.Threshold(grayImage, thresholdImage, 100, 255, ThresholdTypes.Binary);

            // Tìm contours (đoạn viền)
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(thresholdImage, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Tìm bounding rect lớn nhất
            Rect boundingRect = new Rect(0, 0, image.Width, image.Height); // Mặc định toàn bộ ảnh
            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    boundingRect = rect; // Lấy vùng chứa phần nội dung chính
                }
            }

            // Cắt ảnh theo bounding rect để loại bỏ viền
            Mat croppedImage = new Mat(image, boundingRect);
            return croppedImage;
        }

        private Mat CorrectImageOrientation(Mat image, Mat referenceImage)
        {
            // Phát hiện góc và tìm hướng của ảnh
            Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

            // Áp dụng Canny edge detection để tìm các cạnh trong ảnh
            Mat edges = new Mat();
            Cv2.Canny(grayImage, edges, 50, 150);

            // Tìm contours
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Nếu có contours, tìm box nhỏ nhất bao quanh các contour để tính góc
            if (contours.Length > 0)
            {
                // Lọc contours có kích thước nhỏ để tránh lỗi
                contours = contours.Where(c => Cv2.ContourArea(c) > 100).ToArray();
                if (contours.Length > 0)
                {
                    // Tìm box bao quanh các contours
                    RotatedRect box = Cv2.MinAreaRect(contours[0]);

                    // Tính góc để xoay ảnh về đúng hướng
                    double angle = box.Angle;
                    if (angle < -45)
                    {
                        angle += 90;
                    }

                    // Xoay ảnh về góc chính xác
                    return RotateImage(image, angle);
                }
            }

            // Nếu không tìm thấy contour hoặc không có góc rõ ràng, trả về ảnh ban đầu
            return image;
        }


        private Mat RotateImage(Mat image, double angle)
        {
            lblAngle.Text = "Độ nghiêng: " + angle.ToString("F2") + "°";
            // Xoay ảnh theo một góc nhất định
            Mat rotatedImage = new Mat();
            Point2f center = new Point2f(image.Width / 2, image.Height / 2);
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1);
            Cv2.WarpAffine(image, rotatedImage, rotationMatrix, image.Size());
            return rotatedImage;
        }

        private Mat PreprocessImage(Mat image, OpenCvSharp.Size targetSize)
        {
            // Resize mà không xoay ảnh
            Mat resizedImage = new Mat();
            Cv2.Resize(image, resizedImage, targetSize);
            return resizedImage;
        }

        private KeyPoint[] DetectKeyPoints(Mat image)
        {
            ORB orb = ORB.Create();
            KeyPoint[] keypoints = orb.Detect(image);
            return keypoints;
        }

        private (DMatch[] matches, KeyPoint[] keypoints1, KeyPoint[] keypoints2) CompareImages(Mat image1, Mat image2)
        {
            ORB orb = ORB.Create();

            KeyPoint[] keypoints1 = DetectKeyPoints(image1);
            KeyPoint[] keypoints2 = DetectKeyPoints(image2);

            Mat descriptors1 = new Mat();
            Mat descriptors2 = new Mat();

            orb.Compute(image1, ref keypoints1, descriptors1);
            orb.Compute(image2, ref keypoints2, descriptors2);

            BFMatcher matcher = new BFMatcher(NormTypes.Hamming, crossCheck: true);
            DMatch[] matches = matcher.Match(descriptors1, descriptors2);

            return (matches, keypoints1, keypoints2);
        }

        private void DrawDifferences(Mat img1, Mat img2, DMatch[] matches, KeyPoint[] kp1, KeyPoint[] kp2, double threshold = 50)
        {
            foreach (var match in matches)
            {
                if (match.Distance > threshold)
                {
                    OpenCvSharp.Point p1 = new OpenCvSharp.Point(kp1[match.QueryIdx].Pt.X, kp1[match.QueryIdx].Pt.Y);
                    OpenCvSharp.Point p2 = new OpenCvSharp.Point(kp2[match.TrainIdx].Pt.X, kp2[match.TrainIdx].Pt.Y);

                    Cv2.Circle(img1, p1, 10, Scalar.Red, 2);
                    Cv2.Circle(img2, p2, 10, Scalar.Red, 2);
                }
            }
        }

        private void HighlightPixelDifferences(Mat img1, Mat img2, PictureBox box)
        {
            Mat diff = new Mat();
            Cv2.Absdiff(img1, img2, diff); // Tính chênh lệch giữa hai ảnh

            // Chuyển đổi ảnh chênh lệch thành ảnh có các giá trị màu
            Mat mask = new Mat();
            Cv2.InRange(diff, new Scalar(30, 30, 30), new Scalar(255, 255, 255), mask); // Ngưỡng cho chênh lệch màu (có thể điều chỉnh)

            // Tìm contours - tức là vùng khác biệt
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Vẽ vùng khác biệt bằng hình chữ nhật đỏ trên ảnh gốc và ảnh đã quét
            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);

                // Tăng kích thước của hình chữ nhật để dễ thấy sự khác biệt
                rect.X = Math.Max(0, rect.X - 10); // Mở rộng sang trái
                rect.Y = Math.Max(0, rect.Y - 10); // Mở rộng lên trên
                rect.Width = Math.Min(img1.Width - rect.X, rect.Width + 20); // Mở rộng ra ngoài
                rect.Height = Math.Min(img1.Height - rect.Y, rect.Height + 20); // Mở rộng ra ngoài

                // Vẽ hình chữ nhật đỏ lên ảnh gốc và ảnh đã quét
                Cv2.Rectangle(img1, rect, Scalar.Red, 3); // Vẽ trên ảnh gốc
                Cv2.Rectangle(img2, rect, Scalar.Red, 3); // Vẽ trên ảnh đã quét
            }

            // Cập nhật ảnh hiển thị
            box.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(img2);
        }

    }
}
