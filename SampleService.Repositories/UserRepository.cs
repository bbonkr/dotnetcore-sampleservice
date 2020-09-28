using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using SampleService.Authorization.Data;
using SampleService.Data;
using SampleService.Entities;

namespace SampleService.Repositories
{
    public interface IRepository : IDisposable
    {
        Task BeginTranAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken));

        //Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public interface IUserRepository: IRepository
    {
        Task<User> FindByIdAsync(string Id, bool includesReferenceEntities = false);

        Task<User> FindByUsernameAsync(string username, bool includesReferenceEntities = false);

        Task<User> FindByRefreshTokenAsync(string refreshToken);

        Task<IList<User>> GetUsersAsync(
            Expression<Func<User, bool>> predicate = null,
            int page = 1,
            int count = 10,
            bool includesReferenceEntities = false);

        Task CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateAsync(User user, CancellationToken cancellationToken = default(CancellationToken));

        Task CloseAccountAsync(User user, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateFailCountAsync(User user, int count = -1, CancellationToken cancellationToken = default(CancellationToken));

        Task ResetFailCountAsync(User user, CancellationToken cancellationToken = default(CancellationToken));

        Task AddRefreshTokenAsync(User user, RefreshToken refreshToken, CancellationToken cancellationToken = default(CancellationToken));

        Task RevokeRefreshTokenAsync(User user, RefreshToken refreshToken,  CancellationToken cancellationToken = default(CancellationToken));
    }

    public abstract class RepositoryBase: IRepository
    {
        public RepositoryBase(
            DataContext dataContext,
            ILoggerFactory loggerFactory)
        {
            this.dataContext = dataContext;
            logger = loggerFactory.CreateLogger<RepositoryBase>();
        }

        public async Task BeginTranAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            //if (transaction != null)
            //{
            //    await transaction.RollbackAsync(cancellationToken);
            //}

            transaction = await this.dataContext.Database.BeginTransactionAsync(cancellationToken);

            //return transaction;
        }

       public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if(transaction != null) {
                await transaction.CommitAsync(cancellationToken);
                await transaction.DisposeAsync();

                transaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                await transaction.DisposeAsync();

                transaction = null;
            }
        }

        //public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    return dataContext.SaveChangesAsync(cancellationToken);
        //}

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Dispose();
            }

            
        }

        protected readonly DataContext dataContext;
        protected IDbContextTransaction transaction = null;

        private readonly ILogger logger;
    }

    public class UserRepository : RepositoryBase, IUserRepository
    {
        public const int FAIL_COUNT_TO_LOCK = 5;

        public UserRepository(DataContext dataContext, ILoggerFactory loggerFactory):base(dataContext, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<UserRepository>();
        }

        public Task<User> FindByIdAsync(string id, bool includesReferenceEntities = false)
        {
            var query = dataContext.Users
                .Where(x => x.Id == id && x.IsEnabled);

            if (includesReferenceEntities)
            {
                query = query.Include(x => x.RefreshTokens);
            }

            var user = query
                .FirstOrDefault();

            return Task.FromResult(user);
        }

        public Task<User> FindByUsernameAsync(string username, bool includesReferenceEntities = false)
        {
            var query = dataContext.Set<User>()
              .Where(x => x.UserName == username && x.IsEnabled);
            if (includesReferenceEntities)
            {
                query = query.Include(x => x.RefreshTokens);
            }

            var user = query
                .FirstOrDefault();

            return Task.FromResult(user);
        }

        public Task<User> FindByRefreshTokenAsync(string refreshToken)
        {
            var user = dataContext.Users
                .Include(x => x.RefreshTokens)
                .Where(x => x.RefreshTokens.Any(t => t.Token == refreshToken))
                .FirstOrDefault();

            return Task.FromResult(user);
        }

        public Task<IList<User>> GetUsersAsync(
            Expression<Func<User, bool>> predicate = null, 
            int page = 1, 
            int count = 10, 
            bool includesReferenceEntities = false)
        {
            var query = dataContext.Users
                .Where(x => true);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (includesReferenceEntities)
            {
                query = query.Include(x => x.RefreshTokens);
            }

            var skip = (page - 1) * count;

            var users = query.Skip(skip).Take(count).ToList();

            return Task.FromResult<IList<User>>(users);
        }

        public async Task CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            dataContext.Add(user);

            await dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entry = await FindByIdAsync(user.Id);

            entry.FirstName = user.FirstName;
            entry.LastName = user.LastName;

            dataContext.Update(entry);

            await dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task CloseAccountAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entry = await FindByIdAsync(user.Id);

            entry.IsEnabled = false;
            entry.RefreshTokens.Clear();            

            dataContext.Update(entry);

            await dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateFailCountAsync(User user, int count = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (count == 0 || user.FailCount <= FAIL_COUNT_TO_LOCK)
            {
                var entry = await FindByIdAsync(user.Id);

                if (count > 0)
                {
                    entry.FailCount = count;
                }
                else
                {
                    entry.FailCount++;
                }

                if(count == 0)
                {
                    entry.IsLocked = false;
                }
                else if(user.FailCount >= FAIL_COUNT_TO_LOCK)
                {
                    entry.IsLocked = true;
                }

                dataContext.Update(entry);

                await dataContext.SaveChangesAsync(cancellationToken);
            }
        }

        public Task ResetFailCountAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateFailCountAsync(user, 0, cancellationToken);
        }

        public async Task AddRefreshTokenAsync(User user, RefreshToken refreshToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entry = await FindByIdAsync(user.Id, true);

            entry.RefreshTokens.Add(refreshToken);

            await dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeRefreshTokenAsync(User user, RefreshToken refreshToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entry = await FindByIdAsync(user.Id, true);

            var revokedRefreshToken = entry.RefreshTokens
                .FirstOrDefault(x => x.Token == refreshToken.Token);

            revokedRefreshToken.Revoked = DateTimeOffset.UtcNow;
            revokedRefreshToken.RevokedByIp = refreshToken.CreatedByIp;
            revokedRefreshToken.ReplacedByToken = refreshToken.Token;

            dataContext.Update(user);

            await dataContext.SaveChangesAsync(cancellationToken);
        }

        protected readonly ILogger logger;
    }
}
