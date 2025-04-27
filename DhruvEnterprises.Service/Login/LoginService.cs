using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Linq;

namespace DhruvEnterprises.Service
{
    public class LoginService : ILoginService
    {
        #region "Fields"
        private IRepository<User> repoUser;
        private IRepository<Role> repoRole;
        private IRepository<Menu> repoMenu;
        #endregion

        #region "Cosntructor"
        public LoginService(IRepository<User> _repoUserMaster, IRepository<Role> _repoAdminRole, IRepository<Menu> _repoMenu)
        {
            this.repoUser = _repoUserMaster;
            this.repoRole = _repoAdminRole;
            this.repoMenu = _repoMenu;

        }
        #endregion

        public User GetUserDeatils(string email, string password)
        {
            return repoUser.Query().Filter(x => x.Username.ToLower() == email.ToLower() && String.Compare(x.Password, password, false) == 0 && x.IsActive == true).Get().FirstOrDefault();
         }

        public User GetUserDeatilByEmail(string email)
        {
            return repoUser.Query().Filter(x => x.Username.ToLower() == email.ToLower() && x.IsActive == true).Get().FirstOrDefault();
        }

        public User GetUserDeatilByGuid(Guid resetCode)
        {
            return repoUser.Query().Filter(x => x.ResetCode.ToLower() == resetCode.ToString()).Get().FirstOrDefault();
        }

        public User GetUserDeatilById(int userId)
        {
            return repoUser.FindById(userId);
        }

        public User Update(User entity)
        {
            repoUser.Update(entity);
            return repoUser.Query().Filter(x => x.Username.ToLower() == entity.Username.ToLower()).Get().FirstOrDefault();
        }

        #region "Dispose"
        public void Dispose()
        {
            if (repoUser != null)
            {
                repoUser.Dispose();
                repoUser = null;
            }

            if (repoRole != null)
            {
                repoRole.Dispose();
                repoRole = null;
            }

            if (repoMenu != null)
            {
                repoMenu.Dispose();
                repoMenu = null;
            }
        }
        #endregion

    }
}
