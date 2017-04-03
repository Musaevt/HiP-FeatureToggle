using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
	public class ValuesManager
	{
		private readonly ToggleDbContext _dbContext;

		public ValuesManager(ToggleDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		internal void Add(string val)
		{
			var v = new Values() { Value = val };
			_dbContext.Values.Add(v);
			_dbContext.SaveChanges();
		}

		internal IEnumerable<string> Get()
		{
			return _dbContext.Values.Select(v => v.Value).ToList();
		}
	}
}
