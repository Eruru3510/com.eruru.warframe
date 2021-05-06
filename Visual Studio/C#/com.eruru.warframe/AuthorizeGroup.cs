using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.eruru.warframe {

	public class AuthorizeGroup {

		public long Id;
		public DateTime Expiry;
		public bool IsExpiry {

			get => DateTime.Now >= Expiry;

		}

	}

}