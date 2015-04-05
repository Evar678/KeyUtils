using System;
using System.Linq;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using Zxcvbn;

namespace KeyUtils
{
	partial class MainForm : Form
	{
		//I like organization, so everything is in regions. That way, 300 lines of code looks more like 50 :)

		#region Properties

		string[] modetext1 = { "Key.dat made by\nBlockland on this machine",
							   "multiple Key.dats\nusing a known key",
							   "a Key.dat with\ncustom parameters" };

		//Strings of text for the 2nd "What's this?" button in the Decrypt tab
		string[] modetext2 = 
		{ "In order to properly decode your key.dat file, we need to know what blockland thinks your processor name is. To do this, we could either use a c++ dll file (which would be a bit shady) or we can get the processor name from a console.log file that blockland creates.\n\nPlease press the \"Select\" button and select a console.log file created by this computer.\n\nThis is only known to work if the log file was created on a Microsoft operating system like Windows.",
		  "In order to properly decode your key.dat files in this mode, we need a full key associated with one of the keydats.\n\nPlease press the Select button and select the keydat whose key is fully known, then put the known key in the bottom textbox.",
		  "Enter in a MAC address in the top textbox and a processor name in the bottom and the program will output the decryption result.",
		};

		string[] lbltext = 
		{
			"Please select a console.log",
			"Please select a key.dat + known key",
			"Please enter MAC and Processor"
		};

		string[] fileArray = {};
		string file2 = String.Empty;
		Zxcvbn.Zxcvbn strengthEstimator = new Zxcvbn.Zxcvbn();

		byte currentMode = 0;

		#endregion

		#region Constructors

		public MainForm()
		{
			InitializeComponent();
		}

		#endregion Constructors

		#region Textboxes

		private void TXT_FK1_TextChanged(object sender, EventArgs e)
		{
			int x;
			if (!Int32.TryParse(TXT_FK1.Text, out x))
			{
				TXT_FK2.Text = "Please enter a BLID!";
				return;
			}
			else
			{
				TXT_FK2.Text = KeyUtils.generateKeyfromBLID(x);
			}
		}

		private void TXT_BLID1_TextChanged(object sender, EventArgs e)
		{
			string txt = TXT_BLID1.Text;

			if (txt.Length != TXT_BLID1.MaxLength)
			{
				TXT_BLID2.Text = "Please enter Key ID!";
				return;
			}

			string chars = KeyUtils.keyChars;

			int tmp1 = -1,
				tmp2 = 1,
				blid = 0;

			for(int a = 4; a >= 0; a--, tmp2 *= 32)
			{
				tmp1 = chars.IndexOf(txt[a]);
				if(tmp1 < 0)
				{
					TXT_BLID2.Text = "Please enter Key ID!";
					return;
				}

				blid += tmp2 * tmp1;
			}

			TXT_BLID2.Text = blid.ToString();
		}

		#endregion

		#region Buttons

		#region Decrypt Tab

		private void BTN_Mode_Click(object sender, EventArgs e)
		{
			if (++currentMode >= modetext1.Length)
				currentMode = 0;

			LBL_Mode.Text = "Current Decryption Method:\nDecrypt " + modetext1[currentMode];
			LBL_DYN.Text = lbltext[currentMode];

			fileArray = new string[0];

			TXT_Extra1.Text = TXT_Extra2.Text = file2 = String.Empty;

			setKeyDatAmount(0);

			switch(currentMode)
			{
				case 0:
					BTN_Select2.Visible = true;

					TXT_Extra1.Width = 99;
					TXT_Extra1.ReadOnly = true;
					TXT_Extra2.Visible = false;
					break;

				case 1:
					TXT_Extra2.Visible = true;
					break;

				case 2:
					BTN_What2.Visible = true;
					BTN_Select2.Visible = false;

					LBL_DYN.Visible = true;

					TXT_Extra1.Visible = true;

					TXT_Extra1.Width = 152;
					TXT_Extra1.ReadOnly = false;
					break;

				default:
					MessageBox.Show("Something seriously screwed up. Thankfully this will probably never happen again. Restart the program.", "Oh noes!");
					break;
			}
		}

		private void BTN_Select1_Click(object sender, EventArgs e)
		{
			DLG_Open.Multiselect = true;
			DLG_Open.Filter = ".dat file|*.dat|All files|*.*";

			DLG_Open.ShowDialog();

			fileArray = DLG_Open.FileNames;

			if (!String.IsNullOrWhiteSpace(file2))
			{
				int ind = Array.IndexOf(fileArray, file2);
				if (ind >= 0)
				{
					List<string> blah = fileArray.ToList();
					blah.RemoveAt(ind);
					fileArray = blah.ToArray();

					MessageBox.Show("You already selected one of those files using the other select button! The file has not been added to the keydat list, and will remain in the textbox below.", "Warning");
				}
			}

			setKeyDatAmount(fileArray.Length + (!String.IsNullOrWhiteSpace(file2) && currentMode != 0 ? 1 : 0));
		}

		private void BTN_Select2_Click(object sender, EventArgs e)
		{
			DLG_Open.Multiselect = false;

			if (currentMode == 0)
				DLG_Open.Filter = ".log file|*.log|All files|*.*";
			else
				DLG_Open.Filter = ".dat file|*.dat|All files|*.*";

			DLG_Open.ShowDialog();

			file2 = DLG_Open.FileName;

			int ind = Array.IndexOf(fileArray, file2);
			if (ind >= 0)
			{
				List<string> bleh = fileArray.ToList();
				bleh.RemoveAt(ind);
				fileArray = bleh.ToArray();

				MessageBox.Show("You already selected this file using the other select button! The file has been moved from the keydat list into the text box below.", "Warning");
			}

			TXT_Extra1.Text = file2;

			setKeyDatAmount(fileArray.Length + (!String.IsNullOrWhiteSpace(file2) && currentMode != 0 ? 1 : 0));
		}

		private void BTN_What1_Click(object sender, EventArgs e)
		{
			MessageBox.Show("A bit can be either a 1 or a 0, which is 2 values.\n\n" +
					"If there's one bit of uncertainty, then that means you need to manually check 2 different combinations to see if they're valid.\n\n" +
					"If there's two bits of uncertainty, that number doubles to 4 combinations. With 3 bits, that doubles again to 8. With 4 bits it doubles again to 16, and so on.\n\n" +
					"0 bits = 1\n5 bits = 32\n10 bits = 1,024\n20 bits = ~1,050,000\n30 bits = ~1,070,000,000", "What's this?");
		}

		private void BTN_What2_Click(object sender, EventArgs e)
		{
			MessageBox.Show(modetext2[currentMode], "What's this?");
		}

		private void BTN_Decrypt_Click(object sender, EventArgs e)
		{
			DecryptionParameters par = null;

			//Set up decryption parameters in the specific way provided by the documentation
			switch(currentMode)
			{
				case 0:
					par = new DecryptionParameters(fileArray, file2);
					break;
				
				case 1:
					par = new DecryptionParameters(fileArray, file2, TXT_Extra2.Text);
					break;

				case 2:
					par = new DecryptionParameters(fileArray, TXT_Extra1.Text, TXT_Extra2.Text);
					break;
				
				default:
					MessageBox.Show("Something seriously screwed up. Thankfully this will probably never happen again, so please restart the program.", "Oh noes!");
					break;
			}

			//Show the results dialog
			ResultsForm FRM_Results = new ResultsForm(currentMode, par);
			FRM_Results.ShowDialog();
		}

		#endregion Decrypt Tab

		#region Others Tab

		private void BTN_Gen_Click(object sender, EventArgs e)
		{
			TXT_FK1_TextChanged(new object(), new EventArgs());
		}

		#endregion Others Tab

		#endregion Buttons

		#region Decryption Prep

		#endregion Decryption Prep

		#region Other Methods

		private void setKeyDatAmount(int num)
		{
			LBL_Num.Text = num.ToString();
		}

		#endregion Other Methods

		private void TXT_1_TextChanged(object sender, EventArgs e)
		{
			var result = strengthEstimator.EvaluatePassword(TXT_1.Text);
			LBL_Entropy.Text = result.Entropy.ToString("0.0");
		}
	}
}