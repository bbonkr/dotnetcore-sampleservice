//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//using SampleService.Authorization.Data;
//using SampleService.Entities;

//namespace SampleService.Services
//{
//    public interface IUserDataService
//    {
//        Task<User> FindByIdAsync(string id);

//        Task<User> FindByUsernameAsync(string username);

    

//        Task<IList<User>> GetAllAsync(Func<User, bool> predicate, bool includeTokens);
//    }

//    public class UserDataService : IUserDataService
//    {
//        public UserDataService(DataContext dataContext, ILoggerFactory loggerFactory)
//        {
//            this.dataContext = dataContext;
//            this.logger = loggerFactory.CreateLogger<UserDataService>();
//        }

//        public Task<User> FindByIdAsync(string id)
//        {
//            return dataContext.Users
//                .Where(x => x.Id == id)
//                .FirstOrDefaultAsync();
//        }

//        public Task<User> FindByUsernameAsync(string username)
//        {
//            return dataContext.Users
//             .Where(x => x.UserName == username)
//             .FirstOrDefaultAsync();
//        }

//        public Task<IList<User>> GetAllAsync(
//            Func<User, bool> predicate,
//            bool includeRefreshTokens = false)
//        {
//            var query = dataContext.Users.Where(_ => true);

//            if (includeRefreshTokens)
//            {
//                query = query.Include(x => x.RefreshTokens);
//            }

//            var result = query.Where(predicate);

//            return Task.FromResult<IList<User>>(result.ToList());
//        }

        

        

//        private readonly DataContext dataContext;
//        private readonly ILogger logger;
//    }
//}
