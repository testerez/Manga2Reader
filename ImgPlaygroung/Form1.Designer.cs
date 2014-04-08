namespace ImgPlaygroung
{
    partial class Form1
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pbOriginal = new System.Windows.Forms.PictureBox();
            this.pfResult = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btBinarize = new System.Windows.Forms.Button();
            this.btReset = new System.Windows.Forms.Button();
            this.tbBinarize = new System.Windows.Forms.TextBox();
            this.btGrayscale = new System.Windows.Forms.Button();
            this.btCurrentTest = new System.Windows.Forms.Button();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pfResult)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.pbOriginal, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pfResult, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(617, 413);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pbOriginal
            // 
            this.pbOriginal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbOriginal.Location = new System.Drawing.Point(3, 3);
            this.pbOriginal.Name = "pbOriginal";
            this.pbOriginal.Size = new System.Drawing.Size(302, 407);
            this.pbOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbOriginal.TabIndex = 0;
            this.pbOriginal.TabStop = false;
            // 
            // pfResult
            // 
            this.pfResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pfResult.Location = new System.Drawing.Point(311, 3);
            this.pfResult.Name = "pfResult";
            this.pfResult.Size = new System.Drawing.Size(303, 407);
            this.pfResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pfResult.TabIndex = 1;
            this.pfResult.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.tbLog);
            this.panel1.Controls.Add(this.btCurrentTest);
            this.panel1.Controls.Add(this.btGrayscale);
            this.panel1.Controls.Add(this.tbBinarize);
            this.panel1.Controls.Add(this.btReset);
            this.panel1.Controls.Add(this.btBinarize);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(617, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(254, 413);
            this.panel1.TabIndex = 1;
            // 
            // btBinarize
            // 
            this.btBinarize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btBinarize.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btBinarize.Location = new System.Drawing.Point(48, 68);
            this.btBinarize.Name = "btBinarize";
            this.btBinarize.Size = new System.Drawing.Size(75, 23);
            this.btBinarize.TabIndex = 0;
            this.btBinarize.Text = "Binarize";
            this.btBinarize.UseVisualStyleBackColor = true;
            this.btBinarize.Click += new System.EventHandler(this.btBinarize_Click);
            // 
            // btReset
            // 
            this.btReset.Location = new System.Drawing.Point(4, 4);
            this.btReset.Name = "btReset";
            this.btReset.Size = new System.Drawing.Size(75, 23);
            this.btReset.TabIndex = 1;
            this.btReset.Text = "Reset";
            this.btReset.UseVisualStyleBackColor = true;
            this.btReset.Click += new System.EventHandler(this.btReset_Click);
            // 
            // tbBinarize
            // 
            this.tbBinarize.Location = new System.Drawing.Point(4, 68);
            this.tbBinarize.Name = "tbBinarize";
            this.tbBinarize.Size = new System.Drawing.Size(38, 20);
            this.tbBinarize.TabIndex = 2;
            this.tbBinarize.Text = "100";
            // 
            // btGrayscale
            // 
            this.btGrayscale.Location = new System.Drawing.Point(4, 34);
            this.btGrayscale.Name = "btGrayscale";
            this.btGrayscale.Size = new System.Drawing.Size(75, 23);
            this.btGrayscale.TabIndex = 3;
            this.btGrayscale.Text = "Grayscale";
            this.btGrayscale.UseVisualStyleBackColor = true;
            this.btGrayscale.Click += new System.EventHandler(this.btGrayscale_Click);
            // 
            // btCurrentTest
            // 
            this.btCurrentTest.Location = new System.Drawing.Point(4, 123);
            this.btCurrentTest.Name = "btCurrentTest";
            this.btCurrentTest.Size = new System.Drawing.Size(75, 23);
            this.btCurrentTest.TabIndex = 4;
            this.btCurrentTest.Text = "Current test";
            this.btCurrentTest.UseVisualStyleBackColor = true;
            this.btCurrentTest.Click += new System.EventHandler(this.btCurrentTest_Click);
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLog.Location = new System.Drawing.Point(4, 312);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLog.Size = new System.Drawing.Size(250, 97);
            this.tbLog.TabIndex = 5;
            this.tbLog.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 413);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pfResult)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pbOriginal;
        private System.Windows.Forms.PictureBox pfResult;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btBinarize;
        private System.Windows.Forms.Button btReset;
        private System.Windows.Forms.TextBox tbBinarize;
        private System.Windows.Forms.Button btGrayscale;
        private System.Windows.Forms.Button btCurrentTest;
        private System.Windows.Forms.TextBox tbLog;
    }
}

