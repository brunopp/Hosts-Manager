using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HostsManager.Models
{
	public class Mapping
	{
		public int Id { get; set; }

		[MaxLength(254)]
		public string Domain { get; set; }

		[MaxLength(15)]
		public string IP { get; set; }

		[DefaultValue(true)]
		public bool Active { get; set; }

		[DefaultValue(true)]
		public bool ShowInfoBox { get; set; }

		public int? XLeft { get; set; }

		public int? XRight { get; set; }

		public int? YTop { get; set; }

		public int? YBottom { get; set; }

		public override int GetHashCode()
		{
			return Id;
		}
	}

	public class MappingComparer : IEqualityComparer<Mapping>
	{
		public bool Equals(Mapping x, Mapping y)
		{
			return x.Id == y.Id;
		}

		public int GetHashCode(Mapping obj)
		{
			return obj.Id;
		}
	}

	public class MappingComparerDomain : IEqualityComparer<Mapping>
	{
		public bool Equals(Mapping x, Mapping y)
		{
			return x.Domain.Equals(y.Domain, StringComparison.InvariantCultureIgnoreCase);
		}

		public int GetHashCode(Mapping obj)
		{
			return obj.Domain.GetHashCode();
		}
	}
}