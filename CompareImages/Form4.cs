﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CompareImages
{
    public partial class Form4 : Form
    {
        private string imagePath1, imagePath2;
        public Form4()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(imagePath1) || string.IsNullOrEmpty(imagePath2))
            {
                MessageBox.Show("Hãy chọn 2 ảnh trước!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Mat img1 = Cv2.ImRead(imagePath1, ImreadModes.Color))
            using (Mat img2 = Cv2.ImRead(imagePath2, ImreadModes.Color))
            {
                if (img1.Empty() || img2.Empty())
                {
                    MessageBox.Show("Không thể đọc ảnh!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Mat croppedImg1 = CropBorders(img1);
                Mat croppedImg2 = CropBorders(img2);

                if (croppedImg1 == null)
                {
                    MessageBox.Show("Không tìm thấy nội dung hợp lệ sau khi cắt viền!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (croppedImg2 == null)
                {
                    MessageBox.Show("Không tìm thấy nội dung hợp lệ sau khi cắt viền!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                pictureBox2.Image?.Dispose();
                pictureBox2.Image = BitmapConverter.ToBitmap(croppedImg2);

                Mat resultImage1 = CompareAndHighlightDifferences(croppedImg1, croppedImg2);
                Mat resultImage2 = CompareAndHighlightDifferences(croppedImg2, croppedImg1);

                if (resultImage1 != null)
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(resultImage1);
                    pictureBox2.Image = BitmapConverter.ToBitmap(resultImage2);
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

                if (img1.Width > img2.Width)
                    Cv2.Resize(img1, img1, new OpenCvSharp.Size(img2.Width, img1.Height * img2.Width / img1.Width));
                if (img2.Width > img1.Width)
                    Cv2.Resize(img2, img2, new OpenCvSharp.Size(img1.Width, img2.Height * img1.Width / img2.Width));

                //// Cân chỉnh độ sáng và độ tương phản
                //img1 = HistogramEqualization(img1);
                //img2 = HistogramEqualization(img2);

                //// Làm mịn ảnh để giảm độ răng cưa
                //img1 = SmoothImage(img1);
                //img2 = SmoothImage(img2);

                // Làm net

                using var gray1 = new Mat();
                using var gray2 = new Mat();
                Cv2.CvtColor(img1, gray1, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(img2, gray2, ColorConversionCodes.BGR2GRAY);

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
        private Mat CropBorders(Mat originalImage)
        {
            if (originalImage == null || originalImage.Empty())
                return null;

            Mat gray = new Mat();
            Cv2.CvtColor(originalImage, gray, ColorConversionCodes.BGR2GRAY);

            Mat blurred = new Mat();
            Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);

            Mat edges = new Mat();
            Cv2.Canny(blurred, edges, 50, 150);

            Cv2.FindContours(edges, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
                return null;

            // Tìm contour lớn nhất
            int maxContourIndex = 0;
            double maxArea = 0;
            for (int i = 0; i < contours.Length; i++)
            {
                double area = Cv2.ContourArea(contours[i]);
                if (area > maxArea)
                {
                    maxArea = area;
                    maxContourIndex = i;
                }
            }

            // Lấy rotated rectangle (hình chữ nhật xoay) của contour lớn nhất
            RotatedRect rotatedRect = Cv2.MinAreaRect(contours[maxContourIndex]);

            // Lấy box points (4 điểm góc của rotated rectangle)
            Point2f[] boxPoints = rotatedRect.Points();

            // Tạo mask để crop chính xác vùng rotated rect
            Mat mask = Mat.Zeros(originalImage.Rows, originalImage.Cols, MatType.CV_8UC1);
            OpenCvSharp.Point[] pts = Array.ConvertAll(boxPoints, p => new OpenCvSharp.Point((int)p.X, (int)p.Y));
            Cv2.FillConvexPoly(mask, pts, Scalar.White);

            // Áp mask lên ảnh gốc
            Mat masked = new Mat();
            originalImage.CopyTo(masked, mask);

            Size2f size = rotatedRect.Size;
            float angle = rotatedRect.Angle;

            float doLech = 0;
            int canLe = 0;
            // Xác định góc độ xoay ảnh để căn chỉnh
            var topPoints = boxPoints.OrderBy(p => p.Y).Take(2).ToArray();
            if (topPoints.Length == 2)
            {
                Point2f topLeft = topPoints[0].X < topPoints[1].X ? topPoints[0] : topPoints[1];
                Point2f topRight = topPoints[0].X < topPoints[1].X ? topPoints[1] : topPoints[0];

                if (topLeft.Y < topRight.Y)
                {
                    doLech = 0;
                    canLe = 3;
                }
                else if (topLeft.Y == topRight.Y)
                {
                    doLech = -180;
                    canLe = 0;
                }
                else
                {
                    doLech = -180;
                    canLe = 3;
                }
            }

            if (size.Width < size.Height)
            {
                angle += (90 + doLech);
                size = new Size2f(size.Height, size.Width);
            }

            Point2f center = rotatedRect.Center;

            Mat rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);

            Mat rotated = new Mat();
            Cv2.WarpAffine(masked, rotated, rotationMatrix, originalImage.Size());

            int paddingTop = canLe;
            int paddingBottom = 0;
            int paddingLeft = canLe;
            int paddingRight = 0;

            int x = (int)(center.X - size.Width / 2) + paddingLeft;
            int y = (int)(center.Y - size.Height / 2) + paddingTop;
            int width = (int)size.Width + paddingRight - paddingLeft;
            int height = (int)size.Height + paddingBottom - paddingTop;

            x = Math.Max(x, 0);
            y = Math.Max(y, 0);
            if (x + width > rotated.Width)
            {
                width = rotated.Width - x;
            }
            if (y + height > rotated.Height)
            {
                height = rotated.Height - y;
            }

            Rect roi = new Rect(x, y, width, height);

            return new Mat(rotated, roi);
        }

        private void btnChooseImage1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imagePath1 = ofd.FileName;
                    pictureBox1.Image = new System.Drawing.Bitmap(imagePath1);
                }
            }
        }

        private void btnChooseImage2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imagePath2 = ofd.FileName;
                    pictureBox2.Image = new System.Drawing.Bitmap(imagePath2);
                }
            }
        }
        private Mat HistogramEqualization(Mat img)
        {
            using var gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY); // Chuyển sang ảnh xám

            // Thực hiện Histogram Equalization
            Mat equalized = new Mat();
            Cv2.EqualizeHist(gray, equalized);

            // Chuyển lại ảnh xám thành ảnh màu
            Mat colorEqualized = new Mat();
            Cv2.CvtColor(equalized, colorEqualized, ColorConversionCodes.GRAY2BGR);

            return colorEqualized;
        }
        private Mat SmoothImage(Mat img)
        {
            // Làm mịn ảnh bằng Gaussian Blur
            Mat smoothed = new Mat();
            Cv2.GaussianBlur(img, smoothed, new OpenCvSharp.Size(5, 5), 0);
            return smoothed;
        }

    }
}
