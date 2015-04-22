using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

//KeyUtils Keydat Helper Library v1

namespace KeyUtils
{
	#region Helper Classes

	class DecryptionResult
	{
		private bool _completed;
		private int _statuscode;

		public string[] result;

		//To make everything thread-safe
		private object locker = new object();

		public bool completed
		{
			get { return _completed; }
		}

		public int statuscode
		{
			get { return _statuscode; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DecryptionResult"/> class.
		/// </summary>
		/// <param name="statuscode">Statuscode to use. Defaults to 3 (Decryption not started)</param>
		/// <param name="result">Optional text to use as the result text.</param>
		public DecryptionResult(int statuscode = 3, params string[] result)
		{
			if(result != null)
				this.result = result;
			else
				this.result = new string[] { "Decryption not started." };

			_completed = false;
			_statuscode = statuscode;
		}

		/// <summary>
		/// Set the result object to have finished the decryption successfully.
		/// </summary>
		/// <param name="detail">Success text to display to the user.</param>
		public void finishedWithSuccess(params string[] detail)
		{
			lock (locker)
			{
				_completed = true;
				_statuscode = 1;

				result = detail;
			}
		}

		/// <summary>
		/// Set the result object to have finished the decryption unsuccessfully.
		/// </summary>
		/// <param name="errorcode">The error code.</param>
		/// <param name="detail">Error text to display to the user.</param>
		public void finishedWithError(int errorcode, params string[] detail)
		{
			lock(locker)
			{
				_completed = true;
				_statuscode = -errorcode;

				result = detail;
			}
		}

		public override string ToString()
		{
			//Stringbuilders because why not
			StringBuilder result = new StringBuilder();

			if (_completed)
			{
				//Statuscode > 0 indicates that the decryption is done, <= 0 indicates an error.
				if (_statuscode > 0)
					result.AppendLine("Decryption Finished!\r\n");
				else
					result.AppendLine("Decryption Error: Code " + (-statuscode) + "\r\n\r\nError Reason:");
			}
			//Statuscode 1 indicates decryption is in progress.
			else if (_statuscode == 1)
				result.AppendLine("Decrypting...\r\n");
			else
				result.AppendLine("Decryption not started. Still seeing this message after a few seconds? Try restarting the program or contact Ipquarx@Gmail.com about the issue.");

			result.Append(String.Join("\r\n", this.result));
			return result.ToString();
		}
	}

	class DecryptionParameters
	{
		//Fairly self-explanitory
		public string[] keyDatPaths;
		public string[] otherInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="DecryptionParameters"/> class.
		/// </summary>
		/// <param name="keydats">An array of paths to key.dat files.</param>
		/// <param name="other">Other information.</param>
		public DecryptionParameters(string[] keydats, params string[] other)
		{
			keyDatPaths = keydats;
			otherInfo = other;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DecryptionParameters"/> class with blank fields
		/// </summary>
		public DecryptionParameters() : this(new string[0], new string[0])
		{ }
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

		//The 4 most common processor names, collected from lots and lots and lots of console logs posted on BLF.
		//Your processor name is probably in this list.
		static string[] commonProcessors = 
		{
			"Intel (unknown, Pentium Pro/II/III family)", "Intel Pentium III/II", "Intel Pentium M", "AMD (unknown)"
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


		/// <summary>
		/// Generates a FAKE key from an inputted BLID.
		/// </summary>
		/// <param name="blid">The BLID to use.</param>
		/// <returns>A completely fake, randomly generated "key."</returns>
		public static string generateKeyfromBLID(int blid)
		{
			if(blid > 33554431)
				return "BLID must be less than 33,554,432";

			if (blid < 0)
				return "BLID must be positive or 0";

			//Construct the key ID
			byte[] key = new byte[17];
			int a = 0;

			//Convert from base 2 to base 32
			for (; a < 5; a++ )
			{
				key[4 - a] = (byte)(blid & 0x1f);
				blid >>= 5;
			}

			//Generate random numbers
			byte[] randoms = new byte[11];
			RandomNumberGenerator rng = new RNGCryptoServiceProvider();
			Random rng2 = new Random();
			rng.GetBytes(randoms);

			key[5] = (byte)rng2.Next(1, 31);

			//Fill the key byte array
			for (a = 0; a < 11; a++ )
				key[a + 6] = (byte)(randoms[a] & 0x1f);

			StringBuilder x = new StringBuilder();

			//Convert the bytes to a full key
			for (a = 0; a < 17; a++)
				x.Append(keyChars[key[a]]);

			return formatKey(x.ToString());
		}

		/// <summary>
		/// Formats an inputted raw key.
		/// </summary>
		/// <param name="key">The key to format.</param>
		/// <returns>A key with dashes added in, matching the key field boxes that Blockland uses.</returns>
		/// <exception cref="System.ArgumentException">Param must be exactly 17 characters long.</exception>
		public static string formatKey(string key)
		{
			if (key.Length != 17)
				throw new ArgumentException("Param must be 17 characters long", "key");

			return key.Substring(0, 5) + "-" + key.Substring(5, 4) + "-" + key.Substring(9, 4) + "-" + key.Substring(13);
		}

		/// <summary>
		/// Decrypt keydats from param with a given console log and an auto-detected MAC address.
		/// </summary>
		/// <param name="param">The decryption parameters (Keydats, options) to use. Method takes 1 option: A path to a console.log or launcher.log file.</param>
		/// <param name="result">Result object to handle messages to the user.</param>
		public static void decryptMode0(DecryptionParameters param, DecryptionResult result)
		{
			//Read keydats
			List<byte[]> keydats = IO.readKeyDats(param, result);

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

			//Try to find a processor name in it (Should happen in the first few lines)
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

			//Give an error message if we couldn't find the processor name (Probably wasn't a valid log file)
			if (String.IsNullOrEmpty(foundProcessor))
			{
				result.finishedWithError(1, "Failed to read log file " + param.otherInfo[0] + "\n\nReason: Could not find processor name in file");
				return;
			}

			//Start to prepare the result message
			string resultmessage = "Detected MAC: " + GetMacAddress() + "\nDetected Processor Name: " + foundProcessor;

			//Repeat the processor name a few times and then trim it to 17 characters
			while (foundProcessor.Length < 17)
				foundProcessor += foundProcessor;
			foundProcessor = foundProcessor.Substring(0, 17);

			//Prepare byte arrays
			byte[] mac = Encoding.ASCII.GetBytes("XXXXX" + GetMacAddress()),
				processor = Encoding.ASCII.GetBytes(foundProcessor),
				xor = new byte[17];

			//Create XOR value
			int a = 0, b = 0;
			for (; a < 17; a++)
				xor[a] = (byte)(mac[a] + processor[a]);

			//XOR it with all the different keydats
			for(a = 0; a < param.keyDatPaths.Length; a++)
			{
				//Prepare a byte array to store the result in
				byte[] xored = new byte[17];

				//Add to the result message
				resultmessage += "\n\nKeydat file #" + (a + 1) + " (" + Path.GetFileName(param.keyDatPaths[a]) + "):\n";

				//Do the XORing
				for (b = 0; b < 17; b++)
				{
					xored[b] = (byte)(keydats[a][b] ^ xor[b]);

					//If it isn't a vaild character then break out of the loop
					if(validCharTable[xored[b]] == 0)
						break;
				}
				
				//If the decryption failed that means the MAC/Processor combo does not match up with the keydat, so give an error message and keep going
				if(b != 17)
				{
					resultmessage += "Failed to decrypt: Keydat does not match up with MAC/Processor Name\nPartial Key: " + Encoding.Default.GetString(xored).Substring(0, b);
					continue;
				}

				resultmessage += "Recovered Key: " + formatKey(Encoding.UTF8.GetString(xored));
			}

			//Finally send the finished result message
			result.finishedWithSuccess(resultmessage.Split('\n'));
		}

		/// <summary>
		/// Perform a "Known Key Decryption" where one key is used to find all the others inputted.
		/// </summary>
		/// <param name="param">The decryption parameters (Keydats, options) to use. Method takes 2 options: A path to a key.dat file and a plaintext unformatted 17-character key.</param>
		/// <param name="result">Result object to handle messages to the user.</param>
		public static void decryptMode1(DecryptionParameters param, DecryptionResult result)
		{
			//Read in the unknown keydats
			List<byte[]> UnknownKeydats = IO.readKeyDats(param, result);

			//This will catch any errors thrown by IO.readKeyDats
			if (result.completed)
				return;

			//Read in the known keydat (We have to kinda manipulate param here unfortunately)
			param.keyDatPaths = new string[] { param.otherInfo[0] };
			byte[] knownKeydat = IO.readKeyDats(param, result)[0];

			if (result.completed)
				return;

			//Calculate the xor value
			byte[] xor = new byte[17];

			//Xor known keydat with known key
			for (int a = 0; a < 17; a++)
				xor[a] = (byte)(knownKeydat[a] ^ param.otherInfo[1][a]);

			//Define variables for use later
			string message = String.Empty;
			bool hasFailed = false;

			//Loop through all the unknown keydats and try to decrypt them
			for (int a = 0; a < UnknownKeydats.Count; a++)
			{
				message += "Keydat file #" + (a + 1) + " (" + Path.GetFileName(param.keyDatPaths[a]) + "):\n";
				string key = String.Empty;

				for (int b = 0; b < 17; b++)
				{
					//Key1[i] ^ KeyDat1[i] ^ KeyDat2[i] = Key2[i]
					key += (char)(UnknownKeydats[a][b] ^ xor[b]);

					//Let the user know if the results probably aren't correct
					if (!hasFailed && validCharTable[key[b]] == 0)
					{
						hasFailed = true;
						message = "Keydat decryption failed: Known key did not match up with other keydats. Results probably aren't correct.\n\n" + message;
					}
				}

				//Can't format unless we have the full key
				if (!hasFailed)
					key = formatKey(key);

				message += "Recovered Key: " + key + "\n\n";
			}

			if (hasFailed)
				result.finishedWithError(2, ("Error code 2: Decryption Failure\n\n" + message).Split('\n'));
			else
				result.finishedWithSuccess(message.Split('\n'));
		}

		/// <summary>
		/// Performs a decryption on the given key.dats with a given MAC and Processor Name
		/// </summary>
		/// <param name="param">The decryption parameters (Keydats, options) to use.
		/// Method takes 2 options: A plaintext MAC address and a processor name, which is case sensitive.</param>
		/// <param name="result">The result.</param>
		public static void decryptMode2(DecryptionParameters param, DecryptionResult result)
		{
			//Read in the keydats
			List<byte[]> Keydats = IO.readKeyDats(param, result);

			//Catch any error thrown by readKeyDats
			if (result.completed)
				return;

			//Prepare the extra info
			param.otherInfo[0] = "XXXXX" + param.otherInfo[0].Replace(":", "").Replace(" ", "").ToLower();

			while (param.otherInfo[1].Length < 17)
				param.otherInfo[1] += param.otherInfo[1];
			param.otherInfo[1] = param.otherInfo[1].Substring(0, 17);

			//Construct the xor value
			byte[] xor = new byte[17];

			for (int a = 0; a < 17; a++)
				xor[a] = (byte)(0 + param.otherInfo[0][a] + param.otherInfo[1][a]);

			//Prepare variables
			bool hasFailed = false;
			string message = "Results are in hexadecimal.";
			byte[] key = null;

			//Try to decrypt all the keydats
			for (int a = 0; a < Keydats.Count; a++)
			{
				message += "\n\nKeydat file #" + (a + 1) + " (" + Path.GetFileName(param.keyDatPaths[a]) + "):\n";
				key = new byte[17];

				for (int b = 0; b < 17; b++)
				{
					key[b] = (byte)(Keydats[a][b] ^ xor[b]);

					if (!hasFailed && validCharTable[key[b]] == 0)
					{
						message = "MAC/Processor combo does not match up with one or more keydats. Results probably aren't what you wanted.\n\n" + message;
						hasFailed = true;
					}
				}

				message += ByteArrayToString(key);
			}

			result.finishedWithSuccess(message.Split('\n'));
		}

		#endregion Key-related Methods

		//These methods can be left out if you want to re-use the library somewhere else.
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

		//Courtesy of http://stackoverflow.com/a/15784105/4059721 with edits
		private static string GetMacAddress()
		{
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

			return nics[0].GetPhysicalAddress().ToString().ToLower();
		}

		//Courtesy of http://stackoverflow.com/a/311179
		private static string ByteArrayToString(byte[] ba)
		{
			string hex = BitConverter.ToString(ba);
			return hex.Replace("-", "");
		}

		#endregion Other Methods
	}
}