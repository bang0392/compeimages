namespace CompareImages
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            btnChooseImages = new Button();
            btnCompare = new Button();
            lblResult = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(984, 188);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(12, 332);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(984, 261);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // btnChooseImages
            // 
            btnChooseImages.Location = new Point(233, 234);
            btnChooseImages.Name = "btnChooseImages";
            btnChooseImages.Size = new Size(111, 49);
            btnChooseImages.TabIndex = 2;
            btnChooseImages.Text = "Chọn ảnh";
            btnChooseImages.UseVisualStyleBackColor = true;
            btnChooseImages.Click += btnChooseImages_Click;
            // 
            // btnCompare
            // 
            btnCompare.Location = new Point(440, 234);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(111, 49);
            btnCompare.TabIndex = 3;
            btnCompare.Text = "So Sánh";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // lblResult
            // 
            lblResult.Location = new Point(621, 251);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(167, 23);
            lblResult.TabIndex = 4;
            lblResult.Text = ".....";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1056, 637);
            Controls.Add(lblResult);
            Controls.Add(btnCompare);
            Controls.Add(btnChooseImages);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button btnChooseImages;
        private Button btnCompare;
        private Label lblResult;
    }
}
