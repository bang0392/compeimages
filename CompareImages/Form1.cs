using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CompareImages
{
    public partial class Form1 : Form
    {
        private string imagePath1, imagePath2;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btnChooseImages_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.png;*.bmp";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileNames.Length == 2)
                {
                    imagePath1 = ofd.FileNames[0];
                    imagePath2 = ofd.FileNames[1];

                    pictureBox1.Image = new System.Drawing.Bitmap(imagePath1);
                    pictureBox2.Image = new System.Drawing.Bitmap(imagePath2);
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn đúng 2 ảnh!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(imagePath1) || string.IsNullOrEmpty(imagePath2))
            {
                MessageBox.Show("Hãy chọn 2 ảnh trước!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Đọc ảnh
            using (Mat img1 = Cv2.ImRead(imagePath1, ImreadModes.Color))
            using (Mat img2 = Cv2.ImRead(imagePath2, ImreadModes.Color))
            {
                if (img1.Empty() || img2.Empty())
                {
                    MessageBox.Show("Không thể đọc ảnh!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Loại bỏ viền và giữ lại phần nội dung chính
                Mat croppedImg1 = CropBorders(img1);
                Mat croppedImg2 = CropBorders(img2);

                if (croppedImg1 == null || croppedImg2 == null)
                {
                    MessageBox.Show("Không tìm thấy nội dung hợp lệ sau khi cắt viền!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tiến hành so sánh ảnh đã loại bỏ viền
                Mat resultImage = CompareAndHighlightDifferences(croppedImg1, croppedImg2);

                if (resultImage != null)
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(resultImage);
                    pictureBox2.Image = BitmapConverter.ToBitmap(resultImage);
                    lblResult.Text = "Kết quả: Có sự khác biệt!";
                }
                else
                {
                    lblResult.Text = "Kết quả: Ảnh giống nhau!";
                }
            }
        }

        private Mat CompareAndHighlightDifferences(Mat img1, Mat img2)
        {
            try
            {
                if (img1.Empty() || img2.Empty())
                    throw new Exception("Không đọc được ảnh.");

                // Tiến hành resize ảnh về kích thước 500x500 để so sánh
                int targetHeight = 500; // Chiều cao mới
                int originalWidth = img1.Width;
                int originalHeight = img1.Height;

                // Tính toán tỷ lệ giữa chiều cao và chiều rộng
                double aspectRatio = (double)originalWidth / originalHeight;

                // Tính chiều rộng mới dựa trên chiều cao đã thay đổi
                int targetWidth = (int)(targetHeight * aspectRatio);

                // Điều chỉnh kích thước ảnh sao cho không bị méo
                Cv2.Resize(img1, img1, new OpenCvSharp.Size(targetWidth, targetHeight));
                Cv2.Resize(img2, img2, new OpenCvSharp.Size(targetWidth, targetHeight));

                // Áp dụng bộ lọc làm sắc nét (sharpening) cho cả 2 ảnh
                img1 = SharpenImage(img1);
                img2 = SharpenImage(img2);

                using var gray1 = new Mat();
                using var gray2 = new Mat();
                Cv2.CvtColor(img1, gray1, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(img2, gray2, ColorConversionCodes.BGR2GRAY);

                // Dùng ORB để tìm các điểm đặc trưng
                var orb = ORB.Create();
                KeyPoint[] keypoints1, keypoints2;
                Mat descriptors1 = new Mat(), descriptors2 = new Mat();
                orb.DetectAndCompute(gray1, null, out keypoints1, descriptors1);
                orb.DetectAndCompute(gray2, null, out keypoints2, descriptors2);

                var bf = new BFMatcher(NormTypes.Hamming, crossCheck: true);
                var matches = bf.Match(descriptors1, descriptors2);

                if (matches.Length < 4)
                    return null;

                matches = matches.OrderBy(m => m.Distance).Take(50).ToArray();

                var srcPoints = matches.Select(m => keypoints1[m.QueryIdx].Pt).ToArray();
                var dstPoints = matches.Select(m => keypoints2[m.TrainIdx].Pt).ToArray();

                var srcMat = new Mat(srcPoints.Length, 1, MatType.CV_32FC2);
                var dstMat = new Mat(dstPoints.Length, 1, MatType.CV_32FC2);

                for (int i = 0; i < srcPoints.Length; i++)
                {
                    srcMat.Set(i, 0, new Point2f((float)srcPoints[i].X, (float)srcPoints[i].Y));
                    dstMat.Set(i, 0, new Point2f((float)dstPoints[i].X, (float)dstPoints[i].Y));
                }

                var homography = Cv2.FindHomography(srcMat, dstMat, HomographyMethods.Ransac);

                if (homography.Empty())
                    return null;

                using var alignedImg2 = new Mat();
                Cv2.WarpPerspective(img2, alignedImg2, homography, img1.Size());

                using var diff = new Mat();
                Cv2.Absdiff(img1, alignedImg2, diff);

                using var diffGray = new Mat();
                Cv2.CvtColor(diff, diffGray, ColorConversionCodes.BGR2GRAY);

                using var thresh = new Mat();
                Cv2.Threshold(diffGray, thresh, 30, 255, ThresholdTypes.Binary);

                Cv2.FindContours(thresh, out OpenCvSharp.Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                if (contours.Length == 0)
                    return null;

                var resultImg = img1.Clone();

                foreach (var contour in contours)
                {
                    var rect = Cv2.BoundingRect(contour);
                    var center = new OpenCvSharp.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                    int radius = Math.Max(rect.Width, rect.Height) / 2 + 10;

                    // Khoanh đỏ sự khác biệt
                    Cv2.Circle(resultImg, center, radius, new Scalar(0, 0, 255), 3);
                }

                return resultImg;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private Mat SharpenImage(Mat image)
        {
            Mat sharpenKernel = new Mat(3, 3, MatType.CV_32F);
            sharpenKernel.Set<float>(0, 0, -1);
            sharpenKernel.Set<float>(0, 1, -1);
            sharpenKernel.Set<float>(0, 2, -1);
            sharpenKernel.Set<float>(1, 0, -1);
            sharpenKernel.Set<float>(1, 1, 9);
            sharpenKernel.Set<float>(1, 2, -1);
            sharpenKernel.Set<float>(2, 0, -1);
            sharpenKernel.Set<float>(2, 1, -1);
            sharpenKernel.Set<float>(2, 2, -1);

            Mat sharpenedImage = new Mat();
            Cv2.Filter2D(image, sharpenedImage, -1, sharpenKernel);
            return sharpenedImage;
        }


        private Mat CropBorders(Mat image)
        {
            int width = image.Width;
            int height = image.Height;

            int left = width;
            int top = height;
            int right = 0;
            int bottom = 0;

            // Ngưỡng độ chênh lệch màu sắc (bạn có thể điều chỉnh theo nhu cầu)
            int colorThreshold = 50;

            using (var grayImage = new Mat())
            {
                Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte pixelValue = grayImage.At<byte>(y, x);

                        // Kiểm tra nếu pixel không phải là màu viền (màu gần như đen hoặc sáng)
                        if (pixelValue > colorThreshold)
                        {
                            if (x < left) left = x;
                            if (y < top) top = y;
                            if (x > right) right = x;
                            if (y > bottom) bottom = y;
                        }
                    }
                }
            }

            // Nếu không tìm thấy nội dung hợp lệ
            if (right <= left || bottom <= top)
                return null;

            Rect cropRect = new Rect(left, top, right - left + 1, bottom - top + 1);
            return new Mat(image, cropRect); // Cắt phần viền
        }
    }
}
