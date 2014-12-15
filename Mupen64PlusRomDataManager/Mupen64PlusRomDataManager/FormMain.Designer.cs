namespace Mupen64PlusRomDataManager
{
    partial class FormMain
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
            this.buttonWiki = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.buttonValidate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonWiki
            // 
            this.buttonWiki.Location = new System.Drawing.Point(13, 12);
            this.buttonWiki.Name = "buttonWiki";
            this.buttonWiki.Size = new System.Drawing.Size(75, 23);
            this.buttonWiki.TabIndex = 0;
            this.buttonWiki.Text = "Wiki";
            this.buttonWiki.UseVisualStyleBackColor = true;
            this.buttonWiki.Click += new System.EventHandler(this.buttonWiki_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox.Location = new System.Drawing.Point(13, 41);
            this.richTextBox.Name = "richTextBox1";
            this.richTextBox.Size = new System.Drawing.Size(761, 518);
            this.richTextBox.TabIndex = 1;
            this.richTextBox.Text = "";
            // 
            // buttonValidate
            // 
            this.buttonValidate.Location = new System.Drawing.Point(94, 12);
            this.buttonValidate.Name = "buttonValidate";
            this.buttonValidate.Size = new System.Drawing.Size(75, 23);
            this.buttonValidate.TabIndex = 2;
            this.buttonValidate.Text = "Validate";
            this.buttonValidate.UseVisualStyleBackColor = true;
            this.buttonValidate.Click += new System.EventHandler(this.buttonValidate_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 571);
            this.Controls.Add(this.buttonValidate);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.buttonWiki);
            this.Name = "FormMain";
            this.Text = "Rom Database Manager";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonWiki;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button buttonValidate;
    }
}

