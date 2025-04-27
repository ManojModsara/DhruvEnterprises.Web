using System;
using System.Collections;
using DhruvEnterprises.Data;

namespace DhruvEnterprises.Repo
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DhruvEnterprisesDBEntities _context;

        private bool _disposed;
        private Hashtable _repositories;

        public UnitOfWork(DhruvEnterprisesDBEntities context)
        {
            _context = context;
        }

        public UnitOfWork()
        {
            _context = new DhruvEnterprisesDBEntities();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _context.Dispose();

            _disposed = true;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);

                var repositoryInstance =
                    Activator.CreateInstance(repositoryType
                            .MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }
    }
}
