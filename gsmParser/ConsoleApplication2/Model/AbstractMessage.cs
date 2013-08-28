using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Security.Permissions;
using System.IO;

namespace ConsoleApplication2
{
	[Serializable]
	public class AbstractMessage
	{
		/// <summary>
		/// Система-инициатор сообщения
		/// </summary>
		[Serializable]
		public enum InitiationSystem
		{
			/// <summary>
			/// Не указано (по-умолчанию)
			/// </summary>
			Unspecified = 0,

			/// <summary>
			/// Сап
			/// </summary>
			Sap = 1,

			/// <summary>
			/// Пума
			/// </summary>
			Puma = 2,

			/// <summary>
			/// Форис
			/// </summary>
			Foris = 3
		}

		/// <summary>
		/// Номер файла
		/// </summary>
		[XmlElement]
		public virtual long FileNumber { get; set; }

		/// <summary>Дата создания файла заказа (когда файл создала внешняя система)</summary>
		[XmlElement]
		public virtual DateTime CreationDate { get; set; }

		/// <summary>
		/// Номер логического HLR
		/// </summary>
		public virtual int LogicalHLR { get; set; }

		/// <summary>
		/// Система, инициировавшая запрос (SAP - 1, Puma - 2 или Foris - 3)
		/// </summary>
		public virtual InitiationSystem Initiator { get; set; }

		/// <summary>
		/// Общее кол-во элементов в сообщении
		/// </summary>
		public virtual int TotalQuantity { get; set; }


		[ReflectionPermission(SecurityAction.Assert)]
		public override string ToString()
		{
			XmlSerializer xSerializer = null;
			StringBuilder sb = null;
			StringWriter str_wr = null;

			try
			{
				xSerializer = new XmlSerializer(this.GetType());
				sb = new StringBuilder();
				str_wr = new StringWriter(sb);

				xSerializer.Serialize(str_wr, this);

				return sb.ToString();
			}
			finally
			{
				if (str_wr != null)
				{
					str_wr.Dispose();
					str_wr = null;
				}

				sb = null;
				xSerializer = null;
			}
		}

		public static AbstractMessage Deserialize(string serializedMsg)
		{
			AbstractMessage tmp = null;
			XmlSerializer xSerializer = null;
			System.IO.TextReader tr = null;

			try
			{
				xSerializer = new XmlSerializer(typeof(AbstractMessage));

				tr = new System.IO.StringReader(serializedMsg);

				tmp = (AbstractMessage)xSerializer.Deserialize(tr);
				return tmp;
			}
			finally
			{
				if (tr != null)
				{
					tr.Dispose();
					tr = null;
				}
				xSerializer = null;
				tmp = null;
			}
		}


		/// <summary>
		/// Получить имя файла соответствующее конкретному типу сообщения
		/// </summary>
		/// <returns>имя файла</returns>
		public virtual string GetFileName()
		{
			string res = string.Format("SP.{0:00}.{1}{2}.{3}{4}.txt",
									   LogicalHLR,
									   CreationDate.ToString("yyyyMMdd"),
									   CreationDate.ToString("HHmmss"),
									   (int)Initiator,
									   FileNumber.ToString().PadLeft(13, '0'));
			return res;
		}
	}

}
