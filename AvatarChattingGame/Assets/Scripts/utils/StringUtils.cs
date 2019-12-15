
using System;
using System.Text;
using System.Text.RegularExpressions;


	public class StringUtils
	{
		
		public StringUtils()
		{}
		
		// If we want just N bytes from the array
		static public string bytesToString(byte[] data,int length){
			string ret = "";
			for (int i = 0; i <length; i++)
				ret += data[i].ToString("x2")+" ";
			return ret;
		}
		
		// If we want all bytes from the array
		static public string bytesToString(byte[] data){
			string ret = "";
			for (int i = 0; i < data.Length; i++)
				ret += data[i].ToString("x2")+" ";
			return ret;
		}
		
		// If we want all bytes from the array but no spaces
		static public string bytesToString_NS(byte[] data){
			string ret = "";
			for (int i = 0; i < data.Length; i++)
				ret += data[i].ToString("x2");
			return ret;
		}
		
		
		// If we want an array of characters (from bytes) to be a string
		static public string charBytesToString(byte[] data){
			string ret = "";
			for (int i = 0; i < data.Length; i++)
				ret += (char)data[i];
			return ret;
		}
		
		// If we want an array of characters (from bytes) to be a string
		static public string charBytesToString_NZ(byte[] data){
			string ret = "";
			for (int i = 0; i < data.Length; i++){
				if (data[i]==0){
					break;
				}
				ret += (char)data[i];
			}
			return ret;
		}
		
		static public byte[] stringToBytes(string data){
			return Encoding.ASCII.GetBytes(data);
		}

        public static byte[] bitStringToBytes(string bitString)
        {
            byte[] output = new byte[bitString.Length / 8];

            for (int i = 0; i < output.Length; i++)
            {
                for (int b = 0; b <= 7; b++)
                {
                    output[i] |= (byte)((bitString[i * 8 + b] == '1' ? 1 : 0) << (7 - b));
                }
            }

            return output;
        }


		
		static public byte[] hexStringToBytes(string hexString){
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
            	bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);

            }
            return bytes;
		}

		
		
		
		static public string removeWhitespaceChars(string data){
			return Regex.Replace( data, @"\s", "" );
		}
		
	}
