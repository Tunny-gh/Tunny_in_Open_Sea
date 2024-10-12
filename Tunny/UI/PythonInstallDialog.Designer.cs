﻿
namespace Tunny.UI
{
    partial class PythonInstallDialog 
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PythonInstallDialog));
            this.installProgressBar = new System.Windows.Forms.ProgressBar();
            this.installerTitleLabel = new System.Windows.Forms.Label();
            this.installItemLabel = new System.Windows.Forms.Label();
            this.notificationLabel = new System.Windows.Forms.Label();
            this.installBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // installProgressBar
            // 
            this.installProgressBar.Location = new System.Drawing.Point(15, 96);
            this.installProgressBar.Name = "installProgressBar";
            this.installProgressBar.Size = new System.Drawing.Size(334, 23);
            this.installProgressBar.TabIndex = 0;
            // 
            // installerTitleLabel
            // 
            this.installerTitleLabel.AutoSize = true;
            this.installerTitleLabel.Location = new System.Drawing.Point(12, 9);
            this.installerTitleLabel.Name = "installerTitleLabel";
            this.installerTitleLabel.Size = new System.Drawing.Size(282, 15);
            this.installerTitleLabel.TabIndex = 1;
            this.installerTitleLabel.Text = "Installing Python packages depending on Tunny";
            // 
            // installItemLabel
            // 
            this.installItemLabel.Location = new System.Drawing.Point(35, 39);
            this.installItemLabel.Name = "installItemLabel";
            this.installItemLabel.Size = new System.Drawing.Size(288, 55);
            this.installItemLabel.TabIndex = 2;
            this.installItemLabel.Text = "Now Installing: ";
            // 
            // notificationLabel
            // 
            this.notificationLabel.Location = new System.Drawing.Point(12, 131);
            this.notificationLabel.Name = "notificationLabel";
            this.notificationLabel.Size = new System.Drawing.Size(337, 81);
            this.notificationLabel.TabIndex = 3;
            this.notificationLabel.Text = "**This process runs only when Tunny is launched for the first time.**";
            // 
            // PythonInstallDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(358, 219);
            this.Controls.Add(this.notificationLabel);
            this.Controls.Add(this.installItemLabel);
            this.Controls.Add(this.installerTitleLabel);
            this.Controls.Add(this.installProgressBar);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PythonInstallDialog";
            this.Text = "Python Installer";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClosingXButton);
            this.Load += new System.EventHandler(this.OptimizationWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar installProgressBar;
        private System.Windows.Forms.Label installerTitleLabel;
        private System.Windows.Forms.Label installItemLabel;
        private System.Windows.Forms.Label notificationLabel;
        private System.ComponentModel.BackgroundWorker installBackgroundWorker;
    }
}

