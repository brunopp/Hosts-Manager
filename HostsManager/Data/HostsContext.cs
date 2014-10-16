using HostsManager.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace HostsManager.Data
{
	public class HostsContext : DbContext
	{
		public HostsContext() : base(ConfigurationManager.ConnectionStrings["sqldb"].ConnectionString)
		{
			Debug.Write(Database.Connection.ConnectionString);
		}

		public DbSet<Mapping> Mappings { get; set; }
	}
}