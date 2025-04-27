using System.Collections.Generic;
using DhruvEnterprises.Repo.Search;

namespace DhruvEnterprises.Repo
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity FindById(object id);
        void InsertGraph(TEntity entity);
        void Update(TEntity entity);
        TEntity Update(TEntity dbEntity, TEntity entity);
        void Delete(object id);
        void Delete(TEntity entity);
        void Insert(TEntity entity);
        void ChangeEntityState<T>(T entity, ObjectState state) where T : class;
        void ChangeEntityCollectionState<T>(ICollection<T> entityCollection, ObjectState state) where T : class;
        RepositoryQuery<TEntity> Query();
        void SaveChanges();
        void Dispose();

        PagedListResult<TEntity> Search(SearchQuery<TEntity> searchQuery);
        PagedListResult<TEntity> Search(SearchQuery<TEntity> searchQuery, out int totalCount);
    }
}
