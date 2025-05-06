namespace CompareImages
{
    partial class Form2
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
            pictureBoxOriginal = new PictureBox();
            pictureBoxScanned = new PictureBox();
            btnSelectOriginal = new Button();
            btnSelectScanned = new Button();
            btnCompare = new Button();
            lblResult = new Label();
            lblAngle = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOriginal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxScanned).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxOriginal
            // 
            pictureBoxOriginal.Location = new Point(21, 12);
            pictureBoxOriginal.Name = "pictureBoxOriginal";
            pictureBoxOriginal.Size = new Size(422, 590);
            pictureBoxOriginal.TabIndex = 0;
            pictureBoxOriginal.TabStop = false;
            // 
            // pictureBoxScanned
            // 
            pictureBoxScanned.Location = new Point(643, 12);
            pictureBoxScanned.Name = "pictureBoxScanned";
            pictureBoxScanned.Size = new Size(422, 590);
            pictureBoxScanned.TabIndex = 1;
            pictureBoxScanned.TabStop = false;
            // 
            // btnSelectOriginal
            // 
            btnSelectOriginal.Location = new Point(476, 43);
            btnSelectOriginal.Name = "btnSelectOriginal";
            btnSelectOriginal.Size = new Size(138, 23);
            btnSelectOriginal.TabIndex = 2;
            btnSelectOriginal.Text = "Ảnh gốc";
            btnSelectOriginal.UseVisualStyleBackColor = true;
            btnSelectOriginal.Click += btnSelectOriginal_Click;
            // 
            // btnSelectScanned
            // 
            btnSelectScanned.Location = new Point(476, 86);
            btnSelectScanned.Name = "btnSelectScanned";
            btnSelectScanned.Size = new Size(138, 23);
            btnSelectScanned.TabIndex = 3;
            btnSelectScanned.Text = "Ảnh so sánh";
            btnSelectScanned.UseVisualStyleBackColor = true;
            btnSelectScanned.Click += btnSelectScanned_Click;
            // 
            // btnCompare
            // 
            btnCompare.Location = new Point(476, 227);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(138, 23);
            btnCompare.TabIndex = 4;
            btnCompare.Text = "So sánh";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // lblResult
            // 
            lblResult.Location = new Point(476, 322);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(138, 118);
            lblResult.TabIndex = 5;
            lblResult.Text = "....";
            lblResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAngle
            // 
            lblAngle.Location = new Point(476, 469);
            lblAngle.Name = "lblAngle";
            lblAngle.Size = new Size(138, 23);
            lblAngle.TabIndex = 6;
            lblAngle.Text = "độ nghiêng";
            lblAngle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1086, 629);
            Controls.Add(lblAngle);
            Controls.Add(lblResult);
            Controls.Add(btnCompare);
            Controls.Add(btnSelectScanned);
            Controls.Add(btnSelectOriginal);
            Controls.Add(pictureBoxScanned);
            Controls.Add(pictureBoxOriginal);
            Name = "Form2";
            Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)pictureBoxOriginal).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxScanned).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxOriginal;
        private PictureBox pictureBoxScanned;
        private Button btnSelectOriginal;
        private Button btnSelectScanned;
        private Button btnCompare;
        private Label lblResult;
        private Label lblAngle;
    }
}