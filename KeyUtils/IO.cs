using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace KeyUtils
{
	static class IO
	{
		public static string savedProcessor, savedBlocklandLoc;

		private static readonly string configFileLoc = "./config.ini";

		/// <summary>
		/// Writes the saved processor name and blockland folder location to a config file
		/// </summary>
		public static void writeConfigFile()
		{
			if(File.Exists(configFileLoc))
				File.Delete(configFileLoc);

			StreamWriter configFile = File.CreateText(configFileLoc);

			configFile.WriteLine(savedProcessor);
		}

		/// <summary>
		/// Reads the saved config file and puts the results in their respective variables
		/// </summary>
		public static void readConfigFile()
		{
			//TBD
		}

		/// <summary>
		/// Reads a list of keydats into byte arrays.
		/// </summary>
		/// <param name="param">DecryptionParameters which contains a list of keydats to read in.</param>
		/// <param name="result">Result object to handle messages to the user in case it fails to read the files.</param>
		/// <returns>Raw keydat bytes, returnValue[x][y] returns the y-th byte of the x-th keydat</returns>
		public static List<byte[]> readKeyDats(DecryptionParameters param, DecryptionResult result)
		{
			List<byte[]> keydats = new List<byte[]>();
			string failmessage = String.Empty;
			bool didfail = false;
			
			for (int x = 0; x < param.keyDatPaths.Length; x++)
			{
				string path = param.keyDatPaths[x];
				FileStream stream = null;

				try
				{
					//Try to open the file
					stream = File.Open(path, FileMode.Open);
				}
				catch (IOException ex)
				{
					failmessage += "Failed to read file " + path + "\n\nReason:\nError Code 1: IO Failure\n" + ex.Message + "\n\n";
					didfail = true;
					continue;
				}
				catch (Exception ex)
				{
					failmessage += "Failed to read file " + path + "\n\nReason:\nError Code 404: Unknown Failure\n" + ex.Message + "\n\n";
					didfail = true;
					continue;
				}

				if (stream.Length < 17 || stream.Length > 1000)
				{
					failmessage += "Failed to read file " + path + "\n\nReason:\nError Code 101: Not A Keydat File\n\n";
					didfail = true;
					continue;
				}

				//Create an array for the 
				byte[] arr = new byte[17];

				//We assume that blockland will never make a keydat that doesn't have the key part as the last 17 bytes. Should hopefully be update-proof.
				stream.Position = stream.Length - 17;
				stream.Read(arr, 0, 17);

				//Close and dispose the stream
				stream.Close();
				stream.Dispose();

				//Add the array to the keydats list
				keydats.Add(arr);
			}

			//If it failed to read one or more keydats, exit with an error
			if (didfail)
				result.finishedWithError(1, failmessage.Substring(0, failmessage.Length - 2).Split('\n'));

			return keydats;
		}
	}
}
