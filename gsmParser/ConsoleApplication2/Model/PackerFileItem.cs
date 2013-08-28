using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication2
{
	[Serializable]
	public class PackerFileItem
	{
		/// <summary>
		/// Международный идентификатор абонента мобильной связи
		/// </summary>

		public string IMSI { get; set; }

		/// <summary>
		/// Заводской номер карты
		/// </summary>

		public string ICC_ID { get; set; }

		/// <summary>
		/// Телефонный номер абонента
		/// </summary>

		public string MSISDN { get; set; }

		/// <summary>
		/// Personal identification Number 1-персональный секретный PIN абонента
		/// </summary>

		public string PIN1 { get; set; }

		/// <summary>
		/// Personal Unblocking key
		/// </summary>

		public string PUK1 { get; set; }

		/// <summary>
		/// Personal identification Number 2- персональный секретный PIN абонента
		/// </summary>

		public string PIN2 { get; set; }

		/// <summary>
		/// Personal Unblocking key
		/// </summary>

		public string PUK2 { get; set; }

		/// <summary>
		/// KI
		/// </summary>

		public string KI { get; set; }

		/// <summary>
		/// ADM1
		/// </summary>

		public string ADM1 { get; set; }

		/// <summary>
		/// Access_Control
		/// </summary>

		public string Access_Control { get; set; }

		/// <summary>
		/// HRPD_PASSWD
		/// </summary>

		public string HrpdPasswd { get; set; }
	}

}
