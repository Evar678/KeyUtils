using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

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
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("You don't have permissions to save there! Try running the program with admin priviledges or saving in a different location.", "File Error!");
			}
			catch (PathTooLongException)
			{
				MessageBox.Show("The path to save the file at was too long! Try going up a few (Or dozens. I have no idea why you're this far nested in) folder levels.", "File Error!");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Please send a screencap of this to Ipquarx@Gmail.com:\n\n" + ex.Message, "File error!");
			}

			//Will be triggered if an exception was thrown
			if (file == null)
				return;

			file.Write(TXT_Results.Text);

			file.Close();
			file.Dispose();

			MessageBox.Show("Save successful!", "Success!");
		}
	}
}
