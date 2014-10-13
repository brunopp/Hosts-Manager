using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HostsManager.Models
{
	public class Mapping
	{
		public int Id
		{
			get { return Domain.GetHashCode() ^ IP.GetHashCode(); }
		}

		public bool Active { get; set; }

		public string Domain { get; set; }

		public string IP { get; set; }

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
}