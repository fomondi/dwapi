﻿using System;
using Dwapi.SharedKernel.Interfaces;
using Dwapi.SharedKernel.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dwapi.SharedKernel.Enum;
using MySql.Data.MySqlClient;

namespace Dwapi.SharedKernel.Infrastructure.Repository
{
    public abstract class BaseRepository<T, TId> : IRepository<T, TId> where T : Entity<TId>
    {
        protected internal DbContext Context;
        protected internal DbSet<T> DbSet;
        private IDbConnection _connection;

        protected BaseRepository(DbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public virtual T Get(TId id)
        {
            return DbSet.Find(id);
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).AsNoTracking().FirstOrDefault();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return DbSet.AsNoTracking();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).AsNoTracking(); 
        }

        public virtual void Create(T entity)
        {
            if (null != entity)
            {
                Context.Add(entity);
            }
        }

        public virtual void Update(T entity)
        {
            if (null != entity)
            {
                Context.Update(entity);
            }
        }

        public virtual void CreateOrUpdate(T entity)
        {
            if (null == entity)
                return;

            var exists = DbSet.AsNoTracking().FirstOrDefault(x => Equals(x.Id, entity.Id));
            if (null != exists)
            {
                Update(entity);
                return;
            }

            Create(entity);
        }

        public IEnumerable<T> GetFromSql(string query)
        {
            IEnumerable<T> results = Enumerable.Empty<T>();

            var cn = GetConnectionString();

            if (Context.Database.ProviderName.ToLower().Contains("SqlServer".ToLower()))
            {
                using (var connection = new SqlConnection(cn))
                {
                    results = connection.Query<T>(query);

                }
            }

            if (Context.Database.ProviderName.ToLower().Contains("MySql".ToLower()))
            {
                using (var connection = new MySqlConnection(cn))
                {
                    results = connection.Query<T>(query);

                }
            }

            return results;
        }

        public DatabaseProvider GetConnectionProvider()
        {
            DatabaseProvider provider = DatabaseProvider.Other;
            if (Context.Database.ProviderName.ToLower().Contains("SqlServer".ToLower()))
            {
                provider = DatabaseProvider.MsSql;
            }

            if (Context.Database.ProviderName.ToLower().Contains("MySql".ToLower()))
            {
                provider = DatabaseProvider.MySql;
            }
            return provider;
        }

        public virtual void Delete(TId id)
        {
            var entity = DbSet.Find(id);
            Delete(entity);
        }

        public IDbConnection GetConnection(bool opened = true)
        {
            if (null == _connection)
            {
                _connection = Context.Database.GetDbConnection();
            }

            if (opened)
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
            }
            return _connection;
        }

        public string GetConnectionString()
        {
            return Context.Database.GetDbConnection().ConnectionString;
        }

        public void CloseConnection()
        {
            CloseConnection(_connection);
        }

        public void CloseConnection(IDbConnection connection)
        {
            if (null != connection)
            {
                try
                {
                    connection.Close();
                }
                catch { }
            }
        }

        public virtual void Delete(T entity)
        {
            if (null != entity)
                DbSet.Remove(entity);
        }

        public virtual void SaveChanges()
        {
            Context.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }
    }
}