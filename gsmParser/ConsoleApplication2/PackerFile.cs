using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace ConsoleApplication2
{
	public class PackerFile
	{

		public override string ToString()
		{
			StringWriter strWr = null;

			try
			{
				var xSerializer = new XmlSerializer(GetType());
				var sb = new StringBuilder();
				strWr = new StringWriter(sb);

				xSerializer.Serialize(strWr, this);

				return sb.ToString();
			}
			finally
			{
				if (strWr != null)
				{
					strWr.Dispose();
				}
			}
		}


		private PackerFile()
		{ Type = FileType.Undefined; }

		private List<PackerFileItem> m_lst = null;

		/// <summary>
		/// Пользователь
		/// </summary>

		public string Customer { get; set; }

		/// <summary>
		/// Дата заказа в формате DD.MM.YYYY
		/// </summary>

		public DateTime OrderDate { get; set; }

		/// <summary>
		/// Количество произведенных сетевых ресурсов
		/// </summary>

		public int Quantity { get; set; }

		/// <summary>
		/// Тип сетевого ресурса
		/// </summary>

		public FileType Type { get; set; }

		/// <summary>
		/// Код заказа во внешней системе SAP
		/// </summary>

		public string SapRequestCode { get; set; }

		/// <summary>
		/// Срок годности ресурсов в формате DD.MM.YYYY
		/// </summary>

		public DateTime ValidityDate { get; set; }

		/// <summary>
		/// Содержимое ошибки при разборе файла
		/// </summary>

		public string Error { get; set; }

		/// <summary>
		/// Информация по сформированным записям в файле от поставщика
		/// </summary>

		public List<PackerFileItem> Items
		{
			get
			{
				if (m_lst == null)
				{
					m_lst = new List<PackerFileItem>();
				}

				return m_lst;
			}
		}

		public static PackerFile CreateEmpty()
		{
			return new PackerFile();
		}

	}

}
