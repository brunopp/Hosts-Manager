using HostsManager.Models;
using System;
using System.Collections.Generic;
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
			_hostsFile.Add(mapping);
        }

		public void Put(int id, Mapping mapping)
        {
			_hostsFile.Update(mapping, id);
        }

		public void Delete(int id)
        {
			_hostsFile.Delete(id);
        }
    }

	public class HostsFile
	{
		private static Lazy<HostsFile> _instance = new Lazy<HostsFile>(() => new HostsFile());

		private const string _hostsFileName = "hosts";
		private readonly string _hostsFileDir;
		private readonly string _hostsPath;

		private HashSet<Mapping> _mappings = new HashSet<Mapping>(new MappingComparer());
		private FileSystemWatcher _watcher;

		private HostsFile()
		{
			_hostsFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\");
			_hostsPath = Path.Combine(_hostsFileDir, _hostsFileName);

			_watcher = new FileSystemWatcher(_hostsFileDir, _hostsFileName);
			_watcher.NotifyFilter = NotifyFilters.LastWrite;
			_watcher.Changed += _watcher_Changed;
			_watcher.EnableRaisingEvents = true;

			Read();
		}

		void _watcher_Changed(object sender, FileSystemEventArgs e)
		{
			Read();
		}

		public static HostsFile Instance
		{
			get
			{
				return _instance.Value;
			}
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

		public void Update(Mapping newMapping, int id)
		{
			Mapping tmp = _mappings.Where(x => x.Id == id).FirstOrDefault();

			if (tmp != null)
			{
				tmp.Active = newMapping.Active;
				tmp.IP = newMapping.IP;
			}

			Save();
		}

		public void Delete(int id)
		{
			_mappings.RemoveWhere(x => x.Id == id);

			Save();
		}

		public void Read()
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

			_mappings.Clear();
			foreach (Match m in mc)
			{
				_mappings.Add(new Mapping
				{
					Active = !m.Groups[1].Success,
					Domain = m.Groups[3].Value,
					IP = m.Groups[2].Value
				});
			}
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

			Read();
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
