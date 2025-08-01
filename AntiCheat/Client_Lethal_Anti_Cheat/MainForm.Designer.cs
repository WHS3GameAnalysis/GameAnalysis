namespace LethalAntiCheatLauncher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RichTextBox logRichTextBox;
        private System.Windows.Forms.Button launchButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Panel filterPanel;
        private System.Windows.Forms.Button btnFilterAll;
        private System.Windows.Forms.Button btnFilterAntiCheat;
        private System.Windows.Forms.Button btnFilterHeartbeat;
        private System.Windows.Forms.Button btnFilterDll;
        private System.Windows.Forms.Button btnFilterSimpleAC;
        private System.Windows.Forms.Button btnFilterIntegrity;
        private System.Windows.Forms.Button btnFilterBehavior;
        private System.Windows.Forms.Button btnFilterDebug;
        private System.Windows.Forms.Button btnFilterHarmony;
        private System.Windows.Forms.Button btnFilterProcess;
        private System.Windows.Forms.Button btnFilterReflection;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            logRichTextBox = new RichTextBox();
            launchButton = new Button();
            progressBar = new ProgressBar();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            filterPanel = new Panel();
            btnFilterAll = new Button();
            btnFilterAntiCheat = new Button();
            btnFilterHeartbeat = new Button();
            btnFilterDll = new Button();
            btnFilterSimpleAC = new Button();
            btnFilterIntegrity = new Button();
            btnFilterBehavior = new Button();
            btnFilterDebug = new Button();
            btnFilterHarmony = new Button();
            btnFilterProcess = new Button();
            btnFilterReflection = new Button();
            statusStrip.SuspendLayout();
            filterPanel.SuspendLayout();
            SuspendLayout();
            // 
            // logRichTextBox
            // 
            logRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logRichTextBox.BackColor = Color.FromArgb(30, 30, 30);
            logRichTextBox.BorderStyle = BorderStyle.None;
            logRichTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            logRichTextBox.ForeColor = Color.White;
            logRichTextBox.Location = new Point(12, 60);
            logRichTextBox.Margin = new Padding(3, 4, 3, 4);
            logRichTextBox.Name = "logRichTextBox";
            logRichTextBox.ReadOnly = true;
            logRichTextBox.Size = new Size(860, 392);
            logRichTextBox.TabIndex = 0;
            logRichTextBox.Text = "";
            // 
            // launchButton
            // 
            launchButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            launchButton.BackColor = Color.Gray;
            launchButton.FlatStyle = FlatStyle.Flat;
            launchButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            launchButton.ForeColor = Color.White;
            launchButton.Location = new Point(12, 496);
            launchButton.Margin = new Padding(3, 4, 3, 4);
            launchButton.Name = "launchButton";
            launchButton.Size = new Size(860, 51);
            launchButton.TabIndex = 1;
            launchButton.Text = "Launch Game";
            launchButton.UseVisualStyleBackColor = false;
            launchButton.Click += launchButton_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(12, 460);
            progressBar.Margin = new Padding(3, 4, 3, 4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(860, 29);
            progressBar.TabIndex = 2;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 557);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(884, 22);
            statusStrip.TabIndex = 3;
            statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(39, 17);
            statusLabel.Text = "Ready";
            // 
            // filterPanel
            // 
            filterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filterPanel.BackColor = Color.White;
            filterPanel.Controls.Add(btnFilterAll);
            filterPanel.Controls.Add(btnFilterAntiCheat);
            filterPanel.Controls.Add(btnFilterHeartbeat);
            filterPanel.Controls.Add(btnFilterDll);
            filterPanel.Controls.Add(btnFilterSimpleAC);
            filterPanel.Controls.Add(btnFilterIntegrity);
            filterPanel.Controls.Add(btnFilterBehavior);
            filterPanel.Controls.Add(btnFilterDebug);
            filterPanel.Controls.Add(btnFilterHarmony);
            filterPanel.Controls.Add(btnFilterProcess);
            filterPanel.Controls.Add(btnFilterReflection);
            filterPanel.Location = new Point(12, 15);
            filterPanel.Margin = new Padding(3, 4, 3, 4);
            filterPanel.Name = "filterPanel";
            filterPanel.Size = new Size(860, 38);
            filterPanel.TabIndex = 4;
            // 
            // btnFilterAll
            // 
            btnFilterAll.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterAll.Location = new Point(0, 0);
            btnFilterAll.Margin = new Padding(3, 4, 3, 4);
            btnFilterAll.Name = "btnFilterAll";
            btnFilterAll.Size = new Size(75, 29);
            btnFilterAll.TabIndex = 0;
            btnFilterAll.Text = "All";
            btnFilterAll.UseVisualStyleBackColor = false;
            // 
            // btnFilterAntiCheat
            // 
            btnFilterAntiCheat.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterAntiCheat.Location = new Point(80, 0);
            btnFilterAntiCheat.Margin = new Padding(3, 4, 3, 4);
            btnFilterAntiCheat.Name = "btnFilterAntiCheat";
            btnFilterAntiCheat.Size = new Size(75, 29);
            btnFilterAntiCheat.TabIndex = 1;
            btnFilterAntiCheat.Text = "AntiCheat";
            btnFilterAntiCheat.UseVisualStyleBackColor = false;
            // 
            // btnFilterHeartbeat
            // 
            btnFilterHeartbeat.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterHeartbeat.Location = new Point(160, 0);
            btnFilterHeartbeat.Margin = new Padding(3, 4, 3, 4);
            btnFilterHeartbeat.Name = "btnFilterHeartbeat";
            btnFilterHeartbeat.Size = new Size(75, 29);
            btnFilterHeartbeat.TabIndex = 2;
            btnFilterHeartbeat.Text = "Heartbeat";
            btnFilterHeartbeat.UseVisualStyleBackColor = false;
            // 
            // btnFilterDll
            // 
            btnFilterDll.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterDll.Location = new Point(240, 0);
            btnFilterDll.Margin = new Padding(3, 4, 3, 4);
            btnFilterDll.Name = "btnFilterDll";
            btnFilterDll.Size = new Size(75, 29);
            btnFilterDll.TabIndex = 3;
            btnFilterDll.Text = "DLL";
            btnFilterDll.UseVisualStyleBackColor = false;
            // 
            // btnFilterSimpleAC
            // 
            btnFilterSimpleAC.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterSimpleAC.Location = new Point(320, 0);
            btnFilterSimpleAC.Margin = new Padding(3, 4, 3, 4);
            btnFilterSimpleAC.Name = "btnFilterSimpleAC";
            btnFilterSimpleAC.Size = new Size(75, 29);
            btnFilterSimpleAC.TabIndex = 4;
            btnFilterSimpleAC.Text = "SimpleAC";
            btnFilterSimpleAC.UseVisualStyleBackColor = false;
            // 
            // btnFilterIntegrity
            // 
            btnFilterIntegrity.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterIntegrity.Location = new Point(400, 0);
            btnFilterIntegrity.Margin = new Padding(3, 4, 3, 4);
            btnFilterIntegrity.Name = "btnFilterIntegrity";
            btnFilterIntegrity.Size = new Size(75, 29);
            btnFilterIntegrity.TabIndex = 5;
            btnFilterIntegrity.Text = "Integrity";
            btnFilterIntegrity.UseVisualStyleBackColor = false;
            // 
            // btnFilterBehavior
            // 
            btnFilterBehavior.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterBehavior.Location = new Point(480, 0);
            btnFilterBehavior.Margin = new Padding(3, 4, 3, 4);
            btnFilterBehavior.Name = "btnFilterBehavior";
            btnFilterBehavior.Size = new Size(75, 29);
            btnFilterBehavior.TabIndex = 6;
            btnFilterBehavior.Text = "Behavior";
            btnFilterBehavior.UseVisualStyleBackColor = false;
            // 
            // btnFilterDebug
            // 
            btnFilterDebug.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterDebug.Location = new Point(560, 0);
            btnFilterDebug.Margin = new Padding(3, 4, 3, 4);
            btnFilterDebug.Name = "btnFilterDebug";
            btnFilterDebug.Size = new Size(75, 29);
            btnFilterDebug.TabIndex = 7;
            btnFilterDebug.Text = "Debug";
            btnFilterDebug.UseVisualStyleBackColor = false;
            // 
            // btnFilterHarmony
            // 
            btnFilterHarmony.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterHarmony.Location = new Point(640, 0);
            btnFilterHarmony.Margin = new Padding(3, 4, 3, 4);
            btnFilterHarmony.Name = "btnFilterHarmony";
            btnFilterHarmony.Size = new Size(75, 29);
            btnFilterHarmony.TabIndex = 8;
            btnFilterHarmony.Text = "Harmony";
            btnFilterHarmony.UseVisualStyleBackColor = false;
            // 
            // btnFilterProcess
            // 
            btnFilterProcess.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterProcess.Location = new Point(720, 0);
            btnFilterProcess.Margin = new Padding(3, 4, 3, 4);
            btnFilterProcess.Name = "btnFilterProcess";
            btnFilterProcess.Size = new Size(75, 29);
            btnFilterProcess.TabIndex = 9;
            btnFilterProcess.Text = "Process";
            btnFilterProcess.UseVisualStyleBackColor = false;
            // 
            // btnFilterReflection
            // 
            btnFilterReflection.BackColor = Color.FromArgb(224, 224, 224);
            btnFilterReflection.Location = new Point(800, 0);
            btnFilterReflection.Margin = new Padding(3, 4, 3, 4);
            btnFilterReflection.Name = "btnFilterReflection";
            btnFilterReflection.Size = new Size(75, 29);
            btnFilterReflection.TabIndex = 10;
            btnFilterReflection.Text = "Reflection";
            btnFilterReflection.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(884, 579);
            Controls.Add(filterPanel);
            Controls.Add(statusStrip);
            Controls.Add(progressBar);
            Controls.Add(launchButton);
            Controls.Add(logRichTextBox);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            Text = "Lethal Anti-Cheat Client";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            filterPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}