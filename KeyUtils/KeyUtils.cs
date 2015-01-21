using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace KeyUtils
{
	#region Helper Classes

	class DecryptionResult
	{
		private bool _completed;
		private double _progress;
		private int _statuscode;
		private DateTime start, end;

		public string[] result;

		private object locker = new object();

		public bool completed
		{
			get { return _completed; }
		}

		public double progress
		{
			get { return _progress; }
		}

		public int statuscode
		{
			get { return _statuscode; }
		}

		public DecryptionResult(int statuscode = 3, params string[] result)
		{
			if(result != null)
				this.result = result;
			else
				this.result = new string[] { "Decryption not started." };

			_completed = false;
			_progress = 0.0;
			_statuscode = statuscode;
		}

		public void startTimer()
		{
			start = DateTime.Now;
		}

		private void stopTimer()
		{
			end = DateTime.Now;
		}

		private double getTimerSeconds()
		{
			if (start == null)
				throw new Exception("Timer not started.");

			if(end == null)
				return DateTime.Now.Subtract(start).TotalSeconds;

			return end.Subtract(start).TotalSeconds;
		}

		public void finishedWithError(int errorcode, params string[] detail)
		{
			lock(locker)
			{
				_progress = 0.0;
				_completed = true;

				_statuscode = -errorcode;
				result = detail;

				stopTimer();
			}
		}

		public void finishedWithSuccess(params string[] detail)
		{
			lock(locker)
			{
				_progress = 0.0;
				_completed = true;

				_statuscode = 1;

				result = detail;

				stopTimer();
			}
		}

		public void setProgress(double progress, string[] text = null)
		{
			lock(locker)
			{
				_completed = false;
				this._progress = progress;
				_statuscode = 2;

				if(text != null)
					result = text;
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			
			if (completed)
			{
				if (_statuscode > 0)
				{
					//For some reason String.Format makes it so that you need to use \r\n instead of just \n. Kinda annoying but whatever.
					result.AppendLine(String.Format("Decryption Finished!\r\n\r\nTime Taken: {0:0.00} Seconds\r\n", getTimerSeconds()));
				}
				else
					result.AppendLine("Decryption Error: Code " + (-statuscode) + "\r\n\r\nError Reason:");
			}
			else
				result.AppendLine(String.Format("Decrypting...\r\n\r\nProgress: {0:0.000}%\r\n", progress * 100));

			result.Append(String.Join("\r\n", this.result));
			return result.ToString();
		}
	}

	class DecryptionParameters
	{
		public string[] keyDatPaths;
		public string[] otherInfo;

		public DecryptionParameters(string[] keydats, params string[] other)
		{
			keyDatPaths = keydats;
			otherInfo = other;
		}

		public DecryptionParameters()
		{
			keyDatPaths = new string[0];
			otherInfo = new string[0];
		}
	}

	#endregion Helper Classes

	static class KeyUtils
	{
		//Courtesy of http://stackoverflow.com/a/661706/4059721
		private delegate void SetPropertyThreadSafeDelegate<TResult>(Control @this, Expression<Func<TResult>> property, TResult value);

		public static readonly string keyChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

		//All 32 processors that blockland on Windows can recognize
		static string[] processorsFull =
		{
			"AMD (unknown)", "AMD Athlon", "AMD K5", "AMD K6-2", "AMD K6-3", "AMD K6",
			"AuthenticAMD", "Cyrix (unknown)", "Cyrix 6x86mx/MII", "Cyrix 6x86", "Cyrix GXm",
			"CyrixInstead", "Cyrix Media GX", "GenuineIntel", "Intel (unknown)",
			"Intel (unknown, Pentium 4 family)", "Intel (unknown, Pentium family)",
			"Intel (unknown, Pentium Pro/II/III family)", "Intel 486 class",
			"Intel Core 2", "Intel Core", "Intel Itanium 2", "Intel Itanium", 
			"Intel Pentium Celeron", "Intel Pentium III", "Intel Pentium II", "Intel Pentium MMX",
			"Intel Pentium M", "Intel Pentium Pro", "Intel Pentium 4", "Intel Pentium",
			"Unknown x86 Compatible"
		};

		//All 32 processors repeated until 17 characters or more in length, and then trimmed to 17 characters exactly
		//(With duplicates removed, that leaves 29 processor types in total)
		static string[] processorsTrim1 =
		{
			"AMD (unknown)AMD ", "AMD AthlonAMD Ath", "AMD K5AMD K5AMD K",
			"AMD K6-2AMD K6-2A", "AMD K6-3AMD K6-3A", "AMD K6AMD K6AMD K",
			"AuthenticAMDAuthe", "Cyrix (unknown)Cy", "Cyrix 6x86Cyrix 6",
			"Cyrix 6x86mx/MIIC", "Cyrix GXmCyrix GX", "CyrixInsteadCyrix",
			"Cyrix Media GXCyr", "GenuineIntelGenui", "Intel (unknown)In",
			"Intel (unknown, P", "Intel 486 classIn", "Intel Core 2Intel",
			"Intel CoreIntel C", "Intel Itanium 2In", "Intel ItaniumInte",
			"Intel Pentium 4In", "Intel Pentium Cel", "Intel Pentium III",
			"Intel PentiumInte", "Intel Pentium MIn", "Intel Pentium MMX",
			"Intel Pentium Pro", "Unknown x86 Compa"
		};

		//Quick lookup tables for optimized performance
		static byte[] validCharTable =
		{
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0,
			0,1,1,1,1,1,1,1,1,0,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
		};

		#region Key-related Methods

		public static string generateKeyfromBLID(int blid)
		{
			if(blid > 33554431)
				return "BLID must be less than 33554432";

			if (blid < 0)
				return "BLID must be positive or 0";

			//Construct the key ID
			byte[] key = new byte[17];
			int a = 0;

			for (; a < 5; a++ )
			{
				key[4 - a] = (byte)(blid & 0x1f);
				blid >>= 5;
			}

			byte[] randoms = new byte[11];
			RandomNumberGenerator rng = new RNGCryptoServiceProvider();
			Random rng2 = new Random();
			rng.GetBytes(randoms);

			key[5] = (byte)rng2.Next(1, 31);

			for (a = 0; a < 11; a++ )
				key[a + 6] = (byte)(randoms[a] & 0x1f);

			StringBuilder x = new StringBuilder();

			for (a = 0; a < 17; a++)
				x.Append(keyChars[key[a]]);

			return formatKey(x.ToString());
		}

		public static string formatKey(string key)
		{
			if (key.Length != 17)
				throw new ArgumentException("Param must be 17 characters long", "key");

			return key.Substring(0, 5) + "-" + key.Substring(5, 4) + "-" + key.Substring(9, 4) + "-" + key.Substring(13);
		}

		public static void decryptMode0(DecryptionParameters param, DecryptionResult result)
		{
			//result.finishedWithSuccess(GetMACAddress(), "", x.ToString());
			result.startTimer();

			//Read keydats
			List<byte[]> keydats = readKeyDats(param, result);

			if(result.completed)
				return;
			
			if (String.IsNullOrWhiteSpace(param.otherInfo[0]))
			{
				result.finishedWithError(10, "Decryption Mode not yet implemented: Please select a console.log and retry the decryption.");
				return;
			}

			//try to read console log
			StreamReader stream = null;

			try
			{
				stream = File.OpenText(param.otherInfo[0]);
			}
			catch (IOException ex)
			{
				result.finishedWithError(1, ("Failed to read log file " + param.otherInfo[0] + "\nReason: IO Failure\n" + ex.Message).Split('\n'));
				return;
			}
			catch(Exception ex)
			{
				result.finishedWithError(1, ("Failed to read log file " + param.otherInfo[0] + "\nUnknown Failure:\n" + ex.Message).Split('\n'));
				return;
			}

			string foundProcessor = null;
			while(!stream.EndOfStream)
			{
				string line = stream.ReadLine();

				foreach(string process in processorsFull)
				{
					if(line.IndexOf(process) >= 0)
					{
						foundProcessor = process;
						break;
					}
				}

				if (!String.IsNullOrEmpty(foundProcessor))
					break;
			}

			if (String.IsNullOrEmpty(foundProcessor))
			{
				result.finishedWithError(1, "Failed to read log file " + param.otherInfo[0] + "\n\nReason: Could not find Processor Name in File");
				return;
			}

			string resultmessage = "Detected MAC: " + GetMacAddress() + "\nDetected Processor Name: " + foundProcessor;

			while (foundProcessor.Length < 17)
				foundProcessor += foundProcessor;
			foundProcessor = foundProcessor.Substring(0, 17);

			//prepare xor
			byte[] mac = Encoding.ASCII.GetBytes("XXXXX" + GetMacAddress()),
				processor = Encoding.ASCII.GetBytes(foundProcessor),
				xor = new byte[17];

			int a = 0, b = 0;
			for (; a < 17; a++)
				xor[a] = (byte)(mac[a] + processor[a]);

			//xor it with all the different keydats
			for(a = 0; a < param.keyDatPaths.Length; a++)
			{
				byte[] xored = new byte[17];

				resultmessage += "\n\nKeydat file #" + (a + 1) + ": " + Path.GetFileName(param.keyDatPaths[a]) + "\n";

				for (b = 0; b < 17; b++)
				{
					xored[b] = (byte)(keydats[a][b] ^ xor[b]);

					//the Containts method in a hashset is super friggin quick. Like seriously.
					if(validCharTable[xored[b]] == 0)
						break;
				}
				
				if(b != 17)
				{
					resultmessage += "Failed to decrypt: Keydat does not match up with MAC/Processor Name\nPartial Key: " + Encoding.Default.GetString(xored).Substring(0, b);
					continue;
				}

				resultmessage += "Recovered Key: " + formatKey(Encoding.UTF8.GetString(xored));
			}

			result.finishedWithSuccess(resultmessage.Split('\n'));
		}

		public static void decryptMode1(DecryptionParameters param, DecryptionResult result)
		{
			result.finishedWithError(10, "Decryption mode not yet implemented.");
		}

		public static void decryptMode2(DecryptionParameters param, DecryptionResult result)
		{
			result.finishedWithError(10, "Decryption mode not yet implemented.");
		}

		private static List<byte[]> readKeyDats(DecryptionParameters param, DecryptionResult result)
		{
			List<byte[]> keydats = new List<byte[]>();
			string failmessage = String.Empty;
			bool didfail = false;

			result.startTimer();

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
				catch(IOException ex)
				{
					failmessage += "Failed to read file " + path + "\n\nReason:\nError Code 1: IO Failure\n" + ex.Message + "\n\n";
					didfail = true;
					continue;
				}
				catch(Exception ex)
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

			if(didfail)
				result.finishedWithError(1, failmessage.Substring(0, failmessage.Length - 2).Split('\n'));

			return keydats;
		}

		private static char[][][] preparePossibleChars(int keydatcount)
		{
			char[][][] possibleChars = new char[keydatcount][][];

			for (int a = 0; a < keydatcount; a++)
			{
				possibleChars[a] = new char[17][];
				possibleChars[a][0] = new char[] { 'A' };
				possibleChars[a][1] = new char[] { 'A', 'B', 'C', 'D', 'E' };

				for (int b = 2; b < 17; b++)
					possibleChars[a][b] = keyChars.ToCharArray();
			}

			return possibleChars;
		}

		//This function to be refined later
		private static char[][][] narrowPossibleChars(char[][][] possibleChars, List<byte[]> keyDats, int keydatCount)
		{
			int a = 0, b = 0, c = 0, d = 0;

			//4 nested for loops?! Blasphemy!
			for (a = 0;     a < keydatCount - 1; a++)
			for (b = a + 1; b < keydatCount;     b++)
			for (c = 1; c < 17; c++)
			{
				List<char> pc1 = new List<char>(), pc2 = new List<char>();
				byte xored = (byte)(keyDats[a][c] ^ keyDats[b][c]);

				for (d = 0; d < possibleChars[a][c].Length; d++)
				{
					byte xored2 = (byte)(xored ^ possibleChars[a][c][d]);

					//Somehow, these two are functionally equivalent even though in theory they shouldn't be.
					//if(Array.IndexOf(possibleChars[b][c], (char)xored2) >= 0)
					if (validCharTable[xored2] != 0)
					{
						pc1.Add(possibleChars[a][c][d]);
						pc2.Add((char)xored2);
					}
				}

				possibleChars[a][c] = pc1.ToArray();
				possibleChars[b][c] = pc2.ToArray();
			}

			return possibleChars;
		}

		#endregion Key-related Methods

		#region Other Methods

		//Courtesy of http://stackoverflow.com/a/661706/4059721
		public static void SetPropertyThreadSafe<TResult>(this Control @this, Expression<Func<TResult>> property, TResult value)
		{
			var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

			if (propertyInfo == null ||
				!@this.GetType().IsSubclassOf(propertyInfo.ReflectedType) ||
				@this.GetType().GetProperty(propertyInfo.Name, propertyInfo.PropertyType) == null)
			{
				throw new ArgumentException("The lambda expression 'property' must reference a valid property on this Control.");
			}

			if (@this.InvokeRequired)
			{
				@this.Invoke(new SetPropertyThreadSafeDelegate<TResult>(SetPropertyThreadSafe), new object[] { @this, property, value });
			}
			else
			{
				@this.GetType().InvokeMember(propertyInfo.Name, BindingFlags.SetProperty, null, @this, new object[] { value });
			}
		}

		//Courtesy of http://stackoverflow.com/a/15784105/4059721 with minor edits
		private static string GetMacAddress()
		{
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			String sMacAddress = string.Empty;
			foreach (NetworkInterface adapter in nics)
			{
				if (sMacAddress == String.Empty)
				{
					sMacAddress = adapter.GetPhysicalAddress().ToString();
					break;
				}
			}

			return sMacAddress.ToLower();
		}

		#endregion Other Methods
	}

	static class Extensions<T>
	{
		public static T[] mergeArrays(T[] array1, T[] array2)
		{
			List<T> newarray = new List<T>();
			HashSet<T> h2 = new HashSet<T>(array2);

			for(int a = 0; a < array2.Length; a++)
				if (h2.Contains(array2[a]))
					newarray.Add(array2[a]);

			return newarray.ToArray();
		}
	}
}