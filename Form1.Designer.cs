namespace ConvertAST
{
    partial class ConvertAST
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
            bLoad = new Button();
            bSave = new Button();
            pbJob = new ProgressBar();
            tbLog = new TextBox();
            SuspendLayout();
            // 
            // bLoad
            // 
            bLoad.Location = new Point(12, 12);
            bLoad.Name = "bLoad";
            bLoad.Size = new Size(284, 58);
            bLoad.TabIndex = 0;
            bLoad.Text = "Choisir le fichier à charger";
            bLoad.UseVisualStyleBackColor = true;
            bLoad.Click += bLoad_Click;
            // 
            // bSave
            // 
            bSave.Enabled = false;
            bSave.Location = new Point(12, 139);
            bSave.Name = "bSave";
            bSave.Size = new Size(284, 58);
            bSave.TabIndex = 1;
            bSave.Text = "Choisir le nom du fichier modifié";
            bSave.UseVisualStyleBackColor = true;
            bSave.Click += bSave_Click;
            // 
            // pbJob
            // 
            pbJob.Location = new Point(12, 272);
            pbJob.Name = "pbJob";
            pbJob.Size = new Size(284, 34);
            pbJob.TabIndex = 2;
            // 
            // tbLog
            // 
            tbLog.Location = new Point(319, 12);
            tbLog.Multiline = true;
            tbLog.Name = "tbLog";
            tbLog.ReadOnly = true;
            tbLog.ScrollBars = ScrollBars.Vertical;
            tbLog.Size = new Size(763, 510);
            tbLog.TabIndex = 3;
            // 
            // ConvertAST
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1094, 534);
            Controls.Add(tbLog);
            Controls.Add(pbJob);
            Controls.Add(bSave);
            Controls.Add(bLoad);
            Name = "ConvertAST";
            Text = "Conversion de fichiers AST";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button bLoad;
        private Button bSave;
        public ProgressBar pbJob;
        public TextBox tbLog;
    }
}
