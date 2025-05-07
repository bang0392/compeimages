namespace CompareImages
{
    partial class Form4
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            dateTimePicker1 = new DateTimePicker();
            pictureBox2 = new PictureBox();
            btnChooseImage1 = new Button();
            btnChooseImage2 = new Button();
            btnCompare = new Button();
            lblResult = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(24, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(428, 559);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(801, 352);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(200, 23);
            dateTimePicker1.TabIndex = 1;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(646, 12);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(428, 559);
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // btnChooseImage1
            // 
            btnChooseImage1.Location = new Point(495, 79);
            btnChooseImage1.Name = "btnChooseImage1";
            btnChooseImage1.Size = new Size(107, 23);
            btnChooseImage1.TabIndex = 3;
            btnChooseImage1.Text = "Chọn ảnh 1";
            btnChooseImage1.UseVisualStyleBackColor = true;
            btnChooseImage1.Click += btnChooseImage1_Click;
            // 
            // btnChooseImage2
            // 
            btnChooseImage2.Location = new Point(495, 135);
            btnChooseImage2.Name = "btnChooseImage2";
            btnChooseImage2.Size = new Size(107, 23);
            btnChooseImage2.TabIndex = 4;
            btnChooseImage2.Text = "Chọn ảnh 2";
            btnChooseImage2.UseVisualStyleBackColor = true;
            btnChooseImage2.Click += btnChooseImage2_Click;
            // 
            // btnCompare
            // 
            btnCompare.Location = new Point(495, 261);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(107, 23);
            btnCompare.TabIndex = 5;
            btnCompare.Text = "Compare";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // lblResult
            // 
            lblResult.Location = new Point(495, 321);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(107, 79);
            lblResult.TabIndex = 6;
            lblResult.Text = "...";
            lblResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form4
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1101, 596);
            Controls.Add(lblResult);
            Controls.Add(btnCompare);
            Controls.Add(btnChooseImage2);
            Controls.Add(btnChooseImage1);
            Controls.Add(pictureBox2);
            Controls.Add(dateTimePicker1);
            Controls.Add(pictureBox1);
            Name = "Form4";
            Text = "Form4";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private DateTimePicker dateTimePicker1;
        private PictureBox pictureBox2;
        private Button btnChooseImage1;
        private Button btnChooseImage2;
        private Button btnCompare;
        private Label lblResult;
    }
}