using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace KeyUtils
{
	static class IO
	{
		public static List<byte[]> readKeyDats(DecryptionParameters param, DecryptionResult result)
		{
			List<byte[]> keydats = new List<byte[]>();
			string failmessage = String.Empty;
			bool didfail = false;

			int x = 0;
			for (; x < param.keyDatPaths.Length; x++)
			{
				string path = param.keyDatPaths[x];

				//read file
				FileStream stream = null;

				try
				{
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

				byte[] arr = new byte[17];

				//We assume that blockland will never make a keydat that doesn't have the key part as the last 17 bytes.
				stream.Position = stream.Length - 17;
				stream.Read(arr, 0, 17);

				stream.Close();
				stream.Dispose();

				keydats.Add(arr);
			}

			if (didfail)
				result.finishedWithError(1, failmessage.Substring(0, failmessage.Length - 2).Split('\n'));

			return keydats;
		}
	}
}
