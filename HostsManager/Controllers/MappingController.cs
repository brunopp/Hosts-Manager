using HostsManager.Data;
using HostsManager.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace HostsManager.Controllers
{
	//[TokenAuth]
    public class MappingController : ApiController
    {
		HostsFile _hostsFile = HostsFile.Instance;

        public IEnumerable<Mapping> GetAll()
        {
			return _hostsFile.Get();
        }

		public Mapping Get(string domain, bool onlyActive = false)
		{
			return _hostsFile.Get(domain, onlyActive);
		}

        public void Post(Mapping mapping)
        {
			//_hostsFile.Add(mapping);
        }

		public void Put(int id, Mapping mapping)
        {
			//_hostsFile.Update(mapping, id);
        }

		[HttpPut]
		public HttpResponseMessage UpdateCoords(int id, Mapping mapping)
		{
			if (mapping.Id != id)
				return new HttpResponseMessage(HttpStatusCode.Ambiguous);

			_hostsFile.UpdateCoords(mapping);

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		public void Delete(int id)
        {
			//_hostsFile.Delete(id);
        }
    }

	public class HostsFile
	{
		private static Lazy<HostsFile> _instance = new Lazy<HostsFile>(() => new HostsFile());

		private const string _hostsFileName = "hosts";
		private readonly string _hostsFileDir;
		private readonly string _hostsPath;

		private HashSet<Mapping> _mappings;
		private FileSystemWatcher _watcher;

		public static HostsFile Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		private HostsFile()
		{
			_hostsFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\");
			_hostsPath = Path.Combine(_hostsFileDir, _hostsFileName);

			SyncMappings();

			_watcher = new FileSystemWatcher(_hostsFileDir, _hostsFileName);
			_watcher.NotifyFilter = NotifyFilters.LastWrite;
			_watcher.Changed += _watcher_Changed;
			_watcher.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Synchronize the database with the hosts file and update the mappings list.
		/// </summary>
		private void SyncMappings()
		{
			// get mappings in hosts file
			List<Mapping> fileMappings = ReadHostsFile();

			// sync db with hosts file
			using (HostsContext db = new HostsContext())
			{
				List<Mapping> dbMappings = db.Mappings.ToList();

				// delete mappings not in hosts file
				var deletedMappings = dbMappings.Except(fileMappings, new MappingComparerDomain());
				foreach (Mapping m in deletedMappings)
				{
					db.Mappings.Remove(m);
				}

				// add/update mappings
				foreach (Mapping m in fileMappings)
				{
					Mapping tmp = dbMappings.Where(x => x.Domain == m.Domain).FirstOrDefault();
					if (tmp == null)
					{
						db.Mappings.Add(m);
					}
					else if (m.IP != tmp.IP)
					{
						tmp.IP = m.IP;
					}
				}
				db.SaveChanges();

				_mappings = new HashSet<Mapping>(db.Mappings.ToList(), new MappingComparer());
			}
		}

		private List<Mapping> ReadHostsFile()
		{
			string s = "";
			for (int i = 0; i < 10; i++)
			{
				try
				{
					using (StringReader sr = new StringReader(File.ReadAllText(_hostsPath)))
					{
						s = sr.ReadToEnd();
					}

					break;
				}
				catch (Exception)
				{
					System.Threading.Thread.Sleep(1000);
				}
			}

			Regex rx = new Regex(@"^(#)?\s*\b((?:[0-9]{1,3}\.){3}[0-9]{1,3})\b\s*\b([a-zA-Z.-]+)\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			MatchCollection mc = rx.Matches(s);

			List<Mapping> mappings = new List<Mapping>();
			foreach (Match m in mc)
			{
				Mapping mapping = new Mapping
				{
					Domain = m.Groups[3].Value,
					IP = m.Groups[2].Value,
					Active = !m.Groups[1].Success
				};

				Mapping map = mappings.Find(x => x.Domain == mapping.Domain);
				if (map != null && map.Active == true)
				{
					mapping = map;
				}

				mappings.Add(mapping);
			}

			return mappings;
		}

		void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			SyncMappings();
		}

		public Mapping Get(string domain, bool onlyActive)
		{
			var q = _mappings.Where(x => x.Domain == domain);

			if (onlyActive)
				q = q.Where(x => x.Active == onlyActive);

			Mapping m = q.FirstOrDefault();
			return m;
		}

		public List<Mapping> Get()
		{
			return _mappings.ToList();
		}

		public void Add(Mapping mapping)
		{
			_mappings.Add(mapping);

			Save();
		}

		public void UpdateCoords(Mapping mapping)
		{
			using (HostsContext db = new HostsContext())
			{
				Mapping m = db.Mappings.Find(mapping.Id);

				m.XLeft = mapping.XLeft;
				m.XRight = mapping.XRight;
				m.YTop = mapping.YTop;
				m.YBottom = mapping.YBottom;

				db.SaveChanges();

				_mappings.Remove(m);
				_mappings.Add(m);
			}
		}

		public void Update(Mapping mapping)
		{
			Mapping tmp = _mappings.Where(x => x.Id == mapping.Id).FirstOrDefault();

			if (tmp != null)
			{
				if (mapping.Domain != null)
					tmp.Domain = mapping.Domain;
				if (mapping.IP != null)
					tmp.IP = mapping.IP;


				tmp.Active = mapping.Active;
				tmp.IP = mapping.IP;
			}

			Save();
		}

		public void Delete(int id)
		{
			_mappings.RemoveWhere(x => x.Id == id);

			Save();
		}

		public void Save()
		{
			_watcher.EnableRaisingEvents = false;

			using (StreamWriter w = File.CreateText(_hostsPath))
			{
				foreach (Mapping m in _mappings)
				{
					w.WriteLine("{0} {1}", m.IP, m.Domain);
				}
			}

			_watcher.EnableRaisingEvents = true;
		}
	}

	public class TokenAuth : AuthorizeAttribute
	{
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			return HttpContext.Current.Application["Token"].ToString() == HttpContext.Current.Request.Headers["Token"].ToString();
		}
	}
}
