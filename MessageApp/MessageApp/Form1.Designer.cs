namespace MessageApp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Send = new System.Windows.Forms.Button();
            this.MessageBox = new System.Windows.Forms.TextBox();
            this.Messages = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Send
            // 
            this.Send.Location = new System.Drawing.Point(709, 393);
            this.Send.Name = "Send";
            this.Send.Size = new System.Drawing.Size(79, 45);
            this.Send.TabIndex = 0;
            this.Send.Text = "Send";
            this.Send.UseVisualStyleBackColor = true;
            this.Send.Click += new System.EventHandler(this.button1_Click);
            // 
            // MessageBox
            // 
            this.MessageBox.Location = new System.Drawing.Point(12, 402);
            this.MessageBox.Name = "MessageBox";
            this.MessageBox.ShortcutsEnabled = false;
            this.MessageBox.Size = new System.Drawing.Size(691, 26);
            this.MessageBox.TabIndex = 1;
            // 
            // Messages
            // 
            this.Messages.Location = new System.Drawing.Point(12, 9);
            this.Messages.Name = "Messages";
            this.Messages.Size = new System.Drawing.Size(776, 381);
            this.Messages.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Messages);
            this.Controls.Add(this.MessageBox);
            this.Controls.Add(this.Send);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Message App";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Send;
        private System.Windows.Forms.TextBox MessageBox;
        private System.Windows.Forms.Label Messages;
    }
}

