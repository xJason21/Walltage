﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Walltage.Domain.Entities;

namespace Walltage.Domain
{
    public class Repository<T> : IRepository<T> where T : class 
    {
        protected DbContext _dbContext;
        protected DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            //if (context == null)
            //    throw new ArgumentNullException("dbContext can not be null");

            _dbContext = context;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual IQueryable<T> Table()
        {
            return _dbSet.AsQueryable();
        }

        public virtual T FindById(int id)
        {
            return _dbSet.Find(id);
        }
        
        public virtual void Insert(T entity)
        {
            SetBaseEntityForInsert(entity);
            _dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            SetBaseEntityForUpdate(entity);
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(T entity)
        {
            DbEntityEntry dbEntityEntry = _dbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Deleted)
                dbEntityEntry.State = EntityState.Deleted;
            else
            {
                _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            }
        }

        public virtual void Delete(int id)
        {
            var entityToDelete = _dbSet.Find(id);
            if (entityToDelete == null)
                return;
            Delete(entityToDelete);
        }

        public virtual void BulkInsert(IEnumerable<T> entities)
        {
            SetBaseEntityForInsert(entities);
            _dbSet.AddRange(entities);
        }

        public virtual void BulkUpdate(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                SetBaseEntityForUpdate(entity);
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        public virtual void BulkDelete(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                if (_dbContext.Entry(entity).State == EntityState.Detached)
                    _dbSet.Attach(entity);
                _dbSet.Remove(entity);
            }
        }

        public virtual void BulkDelete(IEnumerable<object> ids)
        {
            foreach (var id in ids)
            {
                var entity = _dbSet.Find(id);
                if (entity != null)
                    Delete(entity);
            }
        }

        public virtual int Count(System.Linq.Expressions.Expression<Func<T, bool>> match)
        {
            return _dbSet.Count(match);
        }
        
        public virtual void Save(bool async = false)
        {
            if (async)
            {
                _dbContext.SaveChangesAsync();
            }
            else
            {
                _dbContext.SaveChanges();
            }
        }

        #region Utilities

        private void SetBaseEntityForInsert(T entity)
        {
            var baseEntity = entity as AuditableEntity;
            if (baseEntity == null) return;
            baseEntity.AddedDate = DateTime.Now;
            baseEntity.ModifiedDate = DateTime.Now;
        }

        private void SetBaseEntityForInsert(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
                SetBaseEntityForInsert(entity);
        }

        private void SetBaseEntityForUpdate(T entity)
        {
            var baseEntity = entity as AuditableEntity;
            if (baseEntity == null) return;
            baseEntity.ModifiedDate = DateTime.Now;
        }

        private void SetBaseEntityForUpdate(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
                SetBaseEntityForUpdate(entity);
        }
        #endregion
    }
}
