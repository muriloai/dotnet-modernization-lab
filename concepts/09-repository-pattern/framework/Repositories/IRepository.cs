using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryPatternDemo.Repositories
{
    // Interface clássica de repositório genérico amplamente utilizada no .NET Framework
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes);
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
