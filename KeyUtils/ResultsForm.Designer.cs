namespace KeyUtils
{
	partial class ResultsForm
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
			this.components = new System.ComponentModel.Container();
			this.TXT_Results = new System.Windows.Forms.TextBox();
			this.BTN_Save = new System.Windows.Forms.Button();
			this.TMR_ProgressPoll = new System.Windows.Forms.Timer(this.components);
			this.BTN_How = new System.Windows.Forms.Button();
			this.DLG_Save = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// TXT_Results
			// 
			this.TXT_Results.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TXT_Results.BackColor = System.Drawing.Color.White;
			this.TXT_Results.Location = new System.Drawing.Point(2, 3);
			this.TXT_Results.Multiline = true;
			this.TXT_Results.Name = "TXT_Results";
			this.TXT_Results.ReadOnly = true;
			this.TXT_Results.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TXT_Results.Size = new System.Drawing.Size(379, 230);
			this.TXT_Results.TabIndex = 0;
			// 
			// BTN_Save
			// 
			this.BTN_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BTN_Save.Location = new System.Drawing.Point(303, 236);
			this.BTN_Save.Name = "BTN_Save";
			this.BTN_Save.Size = new System.Drawing.Size(78, 23);
			this.BTN_Save.TabIndex = 1;
			this.BTN_Save.Text = "Save Results";
			this.BTN_Save.UseVisualStyleBackColor = true;
			this.BTN_Save.Click += new System.EventHandler(this.BTN_Save_Click);
			// 
			// TMR_ProgressPoll
			// 
			this.TMR_ProgressPoll.Interval = 200;
			this.TMR_ProgressPoll.Tick += new System.EventHandler(this.TMR_ProgressPoll_Tick);
			// 
			// BTN_How
			// 
			this.BTN_How.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BTN_How.Location = new System.Drawing.Point(2, 236);
			this.BTN_How.Name = "BTN_How";
			this.BTN_How.Size = new System.Drawing.Size(137, 23);
			this.BTN_How.TabIndex = 2;
			this.BTN_How.Text = "How to interpret this result";
			this.BTN_How.UseVisualStyleBackColor = true;
			// 
			// DLG_Save
			// 
			this.DLG_Save.DefaultExt = "txt";
			this.DLG_Save.Filter = "Text file|*.txt|All files|*.*";
			// 
			// ResultsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 262);
			this.Controls.Add(this.BTN_How);
			this.Controls.Add(this.BTN_Save);
			this.Controls.Add(this.TXT_Results);
			this.MinimumSize = new System.Drawing.Size(200, 150);
			this.Name = "ResultsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Results";
			this.Load += new System.EventHandler(this.ResultsForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox TXT_Results;
		private System.Windows.Forms.Button BTN_Save;
		private System.Windows.Forms.Timer TMR_ProgressPoll;
		private System.Windows.Forms.Button BTN_How;
		private System.Windows.Forms.SaveFileDialog DLG_Save;
	}
}