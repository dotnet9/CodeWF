using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWF.Data.SQLite.Infrastructure;

public class SQLiteDbContextRepository<T>(SQLiteBlogDbContext dbContext)
    : CodeWFRepository<T>(dbContext) where T : class;