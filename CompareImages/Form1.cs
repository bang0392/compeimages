using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;

namespace CompareImages
{
    public partial class Form1 : Form
    {
        private string imagePath1, imagePath2;
        public Form1()
        {
            InitializeComponent();
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

            Mat resultImage = CompareAndHighlightDifferences(imagePath2, imagePath1);
            Mat resultImage1 = CompareAndHighlightDifferences(imagePath1, imagePath2);

            if (resultImage != null)
            {
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(resultImage1);
                pictureBox2.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(resultImage);
                lblResult.Text = "Kết quả: Có sự khác biệt!";
            }
            else
            {
                lblResult.Text = "Kết quả: Ảnh giống nhau!";
            }
        }

        private Mat CompareAndHighlightDifferences(string imgPath1, string imgPath2)
        {
            using var img1 = Cv2.ImRead(imgPath1, ImreadModes.Color);
            using var img2 = Cv2.ImRead(imgPath2, ImreadModes.Color);

            if (img1.Empty() || img2.Empty())
                throw new Exception("Không đọc được ảnh.");

            // Resize nhỏ lại nếu ảnh quá lớn
            if (img1.Width > 1000)
                Cv2.Resize(img1, img1, new OpenCvSharp.Size(800, img1.Height * 800 / img1.Width));
            if (img2.Width > 1000)
                Cv2.Resize(img2, img2, new OpenCvSharp.Size(800, img2.Height * 800 / img2.Width));

            // Chuyển về grayscale
            using var gray1 = new Mat();
            using var gray2 = new Mat();
            Cv2.CvtColor(img1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(img2, gray2, ColorConversionCodes.BGR2GRAY);

            // Tạo đối tượng SIFT
            var sift = SIFT.Create();

            // Tìm keypoints và descriptors
            KeyPoint[] keypoints1, keypoints2;
            Mat descriptors1 = new Mat(), descriptors2 = new Mat();
            sift.DetectAndCompute(gray1, null, out keypoints1, descriptors1);
            sift.DetectAndCompute(gray2, null, out keypoints2, descriptors2);

            // Dùng BFMatcher với L2 norm
            var bf = new BFMatcher(NormTypes.L2, crossCheck: true);
            var matches = bf.Match(descriptors1, descriptors2);

            if (matches.Length < 4)
                return null; // Không đủ điểm để align

            // Lấy các match tốt nhất
            matches = matches.OrderBy(m => m.Distance).Take(50).ToArray();

            var srcPoints = matches.Select(m => keypoints1[m.QueryIdx].Pt).ToArray();
            var dstPoints = matches.Select(m => keypoints2[m.TrainIdx].Pt).ToArray();

            // Tạo Mat từ các điểm
            Mat srcMat = new Mat(srcPoints.Length, 1, MatType.CV_32FC2);
            Mat dstMat = new Mat(dstPoints.Length, 1, MatType.CV_32FC2);

            // Chuyển các điểm thành Point2f và gán vào Mat
            for (int i = 0; i < srcPoints.Length; i++)
            {
                srcMat.Set(i, 0, new Point2f((float)srcPoints[i].X, (float)srcPoints[i].Y));
                dstMat.Set(i, 0, new Point2f((float)dstPoints[i].X, (float)dstPoints[i].Y));
            }

            // Tính toán Homography
            var homography = Cv2.FindHomography(srcMat, dstMat, HomographyMethods.Ransac);

            if (homography.Empty())
                return null;

            // Warp img2 về cùng góc nhìn với img1
            using var alignedImg2 = new Mat();
            Cv2.WarpPerspective(img2, alignedImg2, homography, img1.Size());

            // Bây giờ mới so sánh pixel
            using var diff = new Mat();
            Cv2.Absdiff(img1, alignedImg2, diff);

            using var diffGray = new Mat();
            Cv2.CvtColor(diff, diffGray, ColorConversionCodes.BGR2GRAY);

            using var thresh = new Mat();
            Cv2.Threshold(diffGray, thresh, 30, 255, ThresholdTypes.Binary);

            Cv2.FindContours(thresh, out OpenCvSharp.Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
                return null; // Không thấy sự khác biệt

            var resultImg1 = img1.Clone();
            var resultImg2 = alignedImg2.Clone();

            // Vẽ khoanh tròn trên ảnh img1 và img2 mà không thay đổi ảnh gốc
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                var center = new OpenCvSharp.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                int radius = Math.Max(rect.Width, rect.Height) / 2 + 10;

                // Vẽ khoanh tròn đỏ trên ảnh 1 (để hiển thị sự khác biệt trên ảnh 1)
                Cv2.Circle(resultImg1, center, radius, new Scalar(0, 0, 255), 3);  // Red circle on image 1

                // Vẽ khoanh tròn xanh trên ảnh 2 (để phân biệt với ảnh 1)
                Cv2.Circle(resultImg2, center, radius, new Scalar(255, 0, 0), 3);  // Blue circle on image 2
            }

            // Kết hợp ảnh img1 và img2 để hiển thị kết quả
            var result = new Mat();
            Cv2.HConcat(new Mat[] { resultImg1, resultImg2 }, result);

            return result;
        }

    }
}
