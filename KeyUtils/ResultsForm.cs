using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KeyUtils
{
	partial class ResultsForm : Form
	{
		private byte decryptionMode;
		private DecryptionParameters Params;
		private DecryptionResult result;

		private delegate void DecryptDelegate(DecryptionParameters specs, DecryptionResult results);

		private DecryptDelegate[] decryptMethods;

		public ResultsForm(byte mode, DecryptionParameters Params)
		{
			InitializeComponent();

			this.Params = Params;
			decryptMethods = new DecryptDelegate[]
			{
				KeyUtils.decryptMode0, KeyUtils.decryptMode1, KeyUtils.decryptMode2
			};

			decryptionMode = mode;
		}

		private void ResultsForm_Load(object sender, EventArgs e)
		{
			result = doDecrypt(Params);
			TMR_ProgressPoll.Start();
		}

		private void TMR_ProgressPoll_Tick(object sender, EventArgs e)
		{
			if (result.completed)
				TMR_ProgressPoll.Stop();

			TXT_Results.Text = result.ToString();
		}

		private DecryptionResult doDecrypt(DecryptionParameters specs)
		{
			DecryptionResult results = new DecryptionResult();
			Task.Factory.StartNew(() => decryptMethods[decryptionMode](specs, results));

			TMR_ProgressPoll.Start();

			return results;
		}

		private void BTN_Save_Click(object sender, EventArgs e)
		{
			DLG_Save.ShowDialog();

			if (String.IsNullOrWhiteSpace(DLG_Save.FileName))
				MessageBox.Show("Please select a proper place to save the file!", "Error");

			StreamWriter file = null;

			try
			{
				file = File.CreateText(DLG_Save.FileName);
			}
			catch(IOException ex)
			{
				MessageBox.Show("Please sent this message to Ipquarx.\n\n" + ex.Message, "File Error!");
			}

			if (file == null)
				return;

			file.Write(TXT_Results.Text);

			file.Close();
			file.Dispose();

			MessageBox.Show("Save successful!", "Success!");
		}
	}
}
