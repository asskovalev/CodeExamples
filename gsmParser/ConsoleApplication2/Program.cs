using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConsoleApplication2
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var stream = File.OpenRead("TestData\\CDMAFileExample.txt"))
			{
				var f = ParseFile(stream);
				Console.WriteLine(f.ToString());
			}

			Console.ReadKey();
		}

		public static PackerFile ParseFile(string fname)
		{
			if (!File.Exists(fname))
			{
				throw new FileNotFoundException("FileNotFound");
			}

			using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
			{
				return ParseFile(fs);
			}
		}

		public static PackerFile ParseFile(Stream sr)
		{
			PackerFile file = PackerFile.CreateEmpty();

			using (StreamReader str_rd = new StreamReader(sr, Encoding.Default))
			{
				//текущая читаемая строка файла
				//в заголовке 31 строка, потом идут всякие дополнительности
				int cur_line = 0;
				string s = string.Empty;
				while ((s = str_rd.ReadLine()) != null)
				{
					cur_line++;
					string cur_signature = string.Empty;

					if (cur_line == 6)
					{
						cur_signature = "Type:";
						if (!s.StartsWith(cur_signature))
						{
							throw new IOException("InvalidFileFormat");
						}
						string ftype_tmp = s.Substring(cur_signature.Length, s.Length - cur_signature.Length).Trim();
						if (ftype_tmp.IndexOf("CDMA") != -1)
						{
							file.Type = FileType.Cdma;
						}
						else
						{
							file.Type = FileType.Gsm;
						}

						if (file.Type != FileType.Undefined)
						{
							sr.Seek(0, SeekOrigin.Begin);
							switch (file.Type)
							{
								case FileType.Gsm:
									ParseGsmFile(file, sr);
									break;

								case FileType.Cdma:
									ParseCdmaFile(file, sr);
									break;
							}
							break;
						}
					}
				}
			}

			return file;
		}

		private static void ParseGsmFile(PackerFile file, Stream sr)
		{
			var rules = new Dictionary<int, Func<string, int, Exception>>
                {
                    {  1, Parser.Starts("*") },
                    {  2, Parser.When(Parser.And(Parser.IsStarts("*"), Parser.IsContains("HEADER DESCRIPTION")), Parser.ResultOk, Parser.ResultFail) },
                    {  3, Parser.Starts("*") },
                    {  4, Parser.When(Parser.IsStarts("Customer:"), 
                                (s, ln) => 
                                    { 
                                        file.Customer = s.Trim();
                                        return Parser.ResultOk(s, ln); 
                                    }, 
                                Parser.ResultFail) },
                    {  5, Parser.When(Parser.IsStarts("Quantity:"),
                                (s, ln) =>
                                    {
                                        try
                                        {
                                            file.Quantity = Convert.ToInt32(s.Trim());
                                        }
                                        catch (ArithmeticException ex)
                                        {
                                            return new IOException("StringParsingError", ex);
                                        }
                                        catch (FormatException ex)
                                        {
                                            return new IOException("StringFormatError", ex);
                                        }
                                        return Parser.ResultOk(s, ln);
                                    },
                                Parser.ResultFail) },
                    {  6, Parser.ResultOk },
                    {  7, Parser.Starts("Profile:") },
                    {  8, Parser.Starts("Batch:") },
                    {  9, Parser.Starts("*") },
                    { 10, Parser.Starts("Transport_key:") },
                    { 11, Parser.Starts("*") },
                    { 12, Parser.Starts("Address1:") },
                    { 13, Parser.Starts("Address2:") },
                    { 14, Parser.Starts("Address5:") },
                    { 15, Parser.Starts("*") },
                    { 16, Parser.Starts("Graph_ref:") },
                    { 17, Parser.Starts("*") },
                    { 18, Parser.When(Parser.IsStarts("PO_ref_number:"), 
                                (s, ln) => 
                                    { 
                                        file.SapRequestCode = s.Trim();
                                        return Parser.ResultOk(s, ln); 
                                    }, 
                                Parser.ResultFail) },
                    { 19, Parser.Starts("SIM_reference:") },
                    { 20, Parser.When(Parser.IsStarts("Expiry_date:"), 
                                (s, ln) => 
                                    { 
                                        IFormatProvider prov = System.Threading.Thread.CurrentThread.CurrentCulture;
                                        file.ValidityDate = DateTime.ParseExact(s.Trim(), "dd.MM.yyyy", prov);
                                        file.SapRequestCode = s.Trim();
                                        return Parser.ResultOk(s, ln); 
                                    }, 
                                Parser.ResultFail) },
                    { 21, Parser.Starts("*") },
                    { 22, Parser.When(Parser.And(Parser.IsStarts("*"), Parser.IsContains("INPUT VARIABLES")), Parser.ResultOk, Parser.ResultFail) },
                    { 23, Parser.Starts("*") },
                    { 24, Parser.Starts("var_in_list:") },
                    { 25, Parser.Starts("IMSI:") },
                    { 26, Parser.Starts("Ser_nb:") },
                    { 27, Parser.Starts("MSISDN:") },
                    { 28, Parser.Starts("*") },
                    { 29, Parser.When(Parser.And(Parser.IsStarts("*"), Parser.IsContains("OUTPUT VARIABLES")), Parser.ResultOk, Parser.ResultFail) },
                    { 30, Parser.Starts("*") },
                    { 31, Parser.Starts("var_out:") }
                };

			using (StreamReader str_rd = new StreamReader(sr, Encoding.Default))
			{
				//текущая читаемая строка файла
				//в заголовке 31 строка, потом идут всякие дополнительности
				int cur_line = 0;
				string s = string.Empty;
				while ((s = str_rd.ReadLine()) != null)
				{
					cur_line++;

					if (rules.ContainsKey(cur_line))
					{
						var rule = rules[cur_line];
						var check_result = rule(s, cur_line);
						if (check_result != null)
							throw new IOException("InvalidFileFormat", check_result);
					}
					else
					{
						string[] arr = s.Split(' ');
						if (arr.Length < 10)
						{
							throw new IOException("InvalidFileFormat");
						}

						file.Items.Add(new PackerFileItem()
						{
							IMSI = arr[0],
							ICC_ID = arr[1],
							MSISDN = arr[2],
							PIN1 = arr[3],
							PUK1 = arr[4],
							PIN2 = arr[5],
							PUK2 = arr[6],
							KI = arr[7],
							ADM1 = arr[8],
							Access_Control = arr[9]
						});
					}
				}
			}
		}

		private static void ParseCdmaFile(PackerFile file, Stream sr)
		{
			var rules = new Dictionary<int, Func<string, int, Exception>>
                {
                    {  1, Parser.Starts("*") },
                    {  2, Parser.When(Parser.And(Parser.IsStarts("*"), Parser.IsContains("HEADER DESCRIPTION")), Parser.ResultOk, Parser.ResultFail) },
                    {  3, Parser.Starts("*") },
                    {  4, Parser.When(Parser.IsStarts("Customer:"), 
                                (s, ln) => 
                                    { 
                                        file.Customer = s.Trim();
                                        return Parser.ResultOk(s, ln); 
                                    }, 
                                Parser.ResultFail) },
                    {  5, Parser.When(Parser.IsStarts("Quantity:"),
                                (s, ln) =>
                                    {
                                        try
                                        {
                                            file.Quantity = Convert.ToInt32(s.Trim());
                                        }
                                        catch (ArithmeticException ex)
                                        {
											return new IOException("StringParsingError", ex);
                                        }
                                        catch (FormatException ex)
                                        {
											return new IOException("StringFormatError", ex);
                                        }
                                        return Parser.ResultOk(s, ln);
                                    },
                                Parser.ResultFail) },
                    {  6, Parser.ResultOk },
                    {  7, Parser.Starts("Profile:") },
                    {  8, Parser.Starts("Batch:") },
                    {  9, Parser.Starts("*") },
                    { 10, Parser.Starts("Transport_key:") },
                    { 11, Parser.Starts("*") },
                    { 12, Parser.Starts("Address1:") },
                    { 13, Parser.Starts("Address2:") },
                    { 14, Parser.Starts("*") },
                    { 15, Parser.Starts("Graph_ref:") },
                    { 16, Parser.Starts("Artwork:") },
                    { 17, Parser.Starts("*") },
                    { 18, Parser.Starts("Card_Manuf:") },
                    { 19, Parser.When(Parser.IsStarts("PO_ref_number:"), 
                                (s, ln) => 
                                    { 
                                        file.SapRequestCode = s.Trim();
                                        return Parser.ResultOk(s, ln); 
                                    }, 
                                Parser.ResultFail) },
                    { 20, Parser.Starts("SIM_Reference:") },
                    { 21, Parser.Starts("Mailer_Items:") },
                    { 22, Parser.Starts("*") },
                    { 23, Parser.Starts("*") },
                    { 24, Parser.When(Parser.And(Parser.IsStarts("*"), Parser.IsContains("INPUT VARIABLES")), Parser.ResultOk, Parser.ResultFail) },
                    { 25, Parser.Starts("*") },
                    { 26, Parser.Starts("Var_In_List:") },
                    { 27, Parser.Starts("IMSI:") },
                    { 28, Parser.Starts("Ser_nb:") },
                    { 29, Parser.Starts("MSISDN:") },
                    { 30, Parser.Starts("*") },
                    { 31, Parser.Starts("*") },
                    { 32, Parser.When(Parser.And(Parser.IsStarts("*"), Parser.IsContains("OUTPUT VARIABLES")), Parser.ResultOk, Parser.ResultFail) },
                    { 33, Parser.Starts("*") },
                    { 34, Parser.Starts("Var_Out:") },
                };


			using (StreamReader str_rd = new StreamReader(sr, Encoding.Default))
			{
				//текущая читаемая строка файла
				//в заголовке 31 строка, потом идут всякие дополнительности
				int cur_line = 0;
				string s = string.Empty;
				while ((s = str_rd.ReadLine()) != null)
				{
					cur_line++;

					if (rules.ContainsKey(cur_line))
					{
						var rule = rules[cur_line];
						var check_result = rule(s, cur_line);
						if (check_result != null)
							throw new IOException("InvalidFileFormat", check_result);
					}
					else
					{
						string[] arr = s.Split(' ');
						if (arr.Length < 9)
						{
							throw new IOException("InvalidFileFormat");
						}

						file.Items.Add(new PackerFileItem()
						{
							IMSI = arr[0],
							ICC_ID = arr[1],
							MSISDN = arr[2],
							PIN1 = arr[3],
							PUK1 = arr[4],
							PIN2 = arr[5],
							PUK2 = arr[6],
							//это не ошибка что ки в HrpdPasswd
							KI = arr[7],
							HrpdPasswd = arr[7],
							ADM1 = arr[8],
							Access_Control = arr[9]
						});
					}
				}
			}
		}
	}
}