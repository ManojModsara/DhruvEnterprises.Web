using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class UserService : IUserService
    {

        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Role> repoAdminRole;
        private IRepository<Menu> repoMenu;
        private IRepository<View_UserListWithBalance> repoUserBalance;
        private IRepository<NotificationBar> repoNotificationBar;
        //private IRepository<UserKYC> repoUserKyc;
        private IRepository<StateList> repoStateList;
        private IRepository<FireBaseToken> repoFireBaseToken;
        private IRepository<KYCTypedDocument> repoDocumentTypedList;
        private IRepository<UserKYC> repoKYCsUses;
        #endregion

        #region "Cosntructor"
        public UserService(IRepository<UserKYC> _repoKYCsUses, IRepository<StateList> _repoStateMaster, IRepository<UserKYC> _repoUserKyc, IRepository<User> _repoUserMaster, IRepository<Role> _repoAdminRole, IRepository<Menu> _repoMenu, IRepository<View_UserListWithBalance> _repoUserBalance, IRepository<NotificationBar> _repoNotificationBar, IRepository<KYCTypedDocument> _repoDocumentTypedList, IRepository<FireBaseToken> _repoFireBaseToken)
        {
            this.repoKYCsUses = _repoKYCsUses;
            this.repoStateList = _repoStateMaster;
            this.repoFireBaseToken = _repoFireBaseToken;
            //this.repoUserKyc = _repoUserKyc;
            this.repoAdminUser = _repoUserMaster;
            this.repoAdminRole = _repoAdminRole;
            this.repoMenu = _repoMenu;
            this.repoUserBalance = _repoUserBalance;
            this.repoNotificationBar = _repoNotificationBar;
            this.repoDocumentTypedList = _repoDocumentTypedList;
        }
        #endregion

        #region "Methods"
        public ICollection<Role> GetAdminRole(bool isActive = false)
        {
            return repoAdminRole.Query().AsTracking()
                //.Filter(c => c.IsActive == isActive || c.IsActive == true)
                .Get().ToList();
        }

        public User GetUser(int id)
        {
            return repoAdminUser.FindById(id);
        }

        public Role GetUserRole(int roleId)
        {
            return repoAdminRole.FindById(roleId);
        }

        public KeyValuePair<int, List<User>> GetAdminUsers(DataTableServerSide searchModel, int userId = 0)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<User>(searchModel, new Type[] { typeof(User), typeof(UserProfile), typeof(Role) });
            if (fdata != null)
            {
                if (fdata.StatusId > 0)
                {
                    predicate = fdata.StatusId == 1 ? predicate.And(x => x.IsActive) : predicate;
                    predicate = fdata.StatusId == 2 ? predicate.And(x => !x.IsActive) : predicate;
                    predicate = fdata.StatusId == 3 ? predicate.And(x => x.IsLocked) : predicate;
                }
                predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.Id == fdata.UserId);
                predicate = fdata.RoleId == 0 ? predicate : predicate.And(x => x.RoleId == fdata.RoleId);
                predicate = fdata.PackId == 0 ? predicate : predicate.And(x => x.PackageId == fdata.PackId);
                predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.UserProfile.MobileNumber == fdata.CustomerNo);
                predicate = string.IsNullOrEmpty(fdata.Email) ? predicate : predicate.And(x => x.UserProfile.Email == fdata.Email);
                predicate = string.IsNullOrEmpty(fdata.ApiKey) ? predicate : predicate.And(x => x.TokenAPI == fdata.ApiKey);
                predicate = string.IsNullOrEmpty(fdata.IPAddress) ? predicate : predicate.And(x => x.LoginIP == fdata.IPAddress);

            }

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<User> results = repoAdminUser
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(User), typeof(UserProfile), typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<User>> resultResponse = new KeyValuePair<int, List<User>>(totalCount, results);

            return resultResponse;
        }
        public KeyValuePair<int, List<User>> GetAdminCreditUsers(DataTableServerSide searchModel, int userId = 0)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<User>(searchModel, new Type[] { typeof(User), typeof(UserProfile), typeof(Role) });
            if (fdata != null)
            {
                if (fdata.StatusId > 0)
                {
                    predicate = fdata.StatusId == 1 ? predicate.And(x => x.IsActive) : predicate;
                    predicate = fdata.StatusId == 2 ? predicate.And(x => !x.IsActive) : predicate;
                    predicate = fdata.StatusId == 3 ? predicate.And(x => x.IsLocked) : predicate;
                }
                predicate = predicate.And(x => x.CreditBal != 0 && x.CreditBal!=null);
                predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.Id == fdata.UserId);
                predicate = fdata.RoleId == 0 ? predicate : predicate.And(x => x.RoleId == fdata.RoleId);
                predicate = fdata.PackId == 0 ? predicate : predicate.And(x => x.PackageId == fdata.PackId);
                predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.UserProfile.MobileNumber == fdata.CustomerNo);
                predicate = string.IsNullOrEmpty(fdata.Email) ? predicate : predicate.And(x => x.UserProfile.Email == fdata.Email);
                predicate = string.IsNullOrEmpty(fdata.ApiKey) ? predicate : predicate.And(x => x.TokenAPI == fdata.ApiKey);
                predicate = string.IsNullOrEmpty(fdata.IPAddress) ? predicate : predicate.And(x => x.LoginIP == fdata.IPAddress);

            }

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<User> results = repoAdminUser
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(User), typeof(UserProfile), typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<User>> resultResponse = new KeyValuePair<int, List<User>>(totalCount, results);

            return resultResponse;
        }
        public User Save(User adminUser)
        {
            adminUser.UpdatedDate = DateTime.Now;
            if (adminUser.Id == 0)
            {
                adminUser.AddedDate = DateTime.Now;
                repoAdminUser.Insert(adminUser);
            }
            else
            {
                
                repoAdminUser.Update(adminUser);
            }
            return adminUser;
        }

        public bool UserExists(string Emailid, string Mobileno)
        {
            ICollection<User> users = repoAdminUser.Query().AsTracking()
              .Filter(c => c.Username == Emailid || c.UserProfile.MobileNumber == Mobileno)
              .Get().ToList();

            return users.Count > 0 ? true : false;

        }

        public bool Active(int id)
        {
            var AdminUser = repoAdminUser.FindById(id);
            if (AdminUser != null)
            {
                AdminUser.IsActive = !(AdminUser.IsActive);
                repoAdminUser.SaveChanges();
            }
            return true;
        }

        public decimal GetUserWalletBalance(int userid)
        {
            User user = repoAdminUser.FindById(userid);

            var txnledger = user.TxnLedgers1.OrderByDescending(t => t.Id).FirstOrDefault();
            return txnledger != null ? txnledger.CL_Bal ?? 0 : 0;
        }

        public decimal GetUserWalletBalanceVW(int userid)
        {
            var txnledger = repoUserBalance.Query().Filter(x => x.Id == userid).Get().FirstOrDefault();
            return txnledger != null ? txnledger.cl_bal : 0;
        }

        public User GetUserByApiToken(string TokenId)
        {
            return repoAdminUser.Query().Filter(x => x.TokenAPI == TokenId).Get().FirstOrDefault();
        }

        public ICollection<View_UserListWithBalance> GetUserListWithBalace(int id = 0)
        {
            return repoUserBalance.Query().Filter(x => x.Id == id || id == 0).Get().ToList();
        }

        public List<User> GetUserList(int RoleID = 0)
        {
            return repoAdminUser.Query().Filter(x => (x.RoleId == RoleID || RoleID == 0) && x.IsActive).Get().ToList();
        }

        public KeyValuePair<int, List<NotificationBar>> GetNotificationBars(DataTableServerSide searchModel, int roleid = 0)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<NotificationBar>(searchModel, new Type[] { typeof(NotificationBar), typeof(Role) });


            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<NotificationBar> results = repoNotificationBar
                .Query()
                // .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(NotificationBar), typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .OrderByDescending(x => x.IsActive)
                .ToList();

            KeyValuePair<int, List<NotificationBar>> resultResponse = new KeyValuePair<int, List<NotificationBar>>(totalCount, results);

            return resultResponse;
        }


        public List<NotificationBar> GetNotificationBarList(byte RoleID = 0)
        {
            return repoNotificationBar.Query().Filter(x => (RoleID == 0 ? true : x.RoleId == RoleID) && x.IsActive == true).Get().ToList();
        }


        public bool ActiveNoteBar(int id)
        {
            var notebar = repoNotificationBar.FindById(id);
            if (notebar != null)
            {
                notebar.IsActive = !(notebar.IsActive);
                repoAdminUser.SaveChanges();
            }
            return true;
        }

        public NotificationBar Save(NotificationBar notificationBar)
        {
            notificationBar.UpdatedDate = DateTime.Now;
            if (notificationBar.Id == 0)
            {
                notificationBar.AddedDate = DateTime.Now;
                repoNotificationBar.Insert(notificationBar);
            }
            else
            {
                repoNotificationBar.Update(notificationBar);
            }
            return notificationBar;
        }


        public void Delete(int id)

        {

            var notebar = repoNotificationBar.FindById(id);
            repoNotificationBar.Delete(notebar);


        }


        public NotificationBar GetNotificationBar(int id)
        {
            return repoNotificationBar.FindById(id);
        }

       
        public List<StateList> GetStates()
        {
            return repoStateList.Query().Get().ToList();
        }

        public List<KYCTypedDocument> GetKYCDocList()
        {
            return repoDocumentTypedList.Query().Get().ToList();
        }
        public KeyValuePair<int, List<UserKYC>> GetKYCsUser(DataTableServerSide searchModel, int userId = 0)
        {

            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<UserKYC>(searchModel, new Type[] { typeof(UserKYC), typeof(User) });

            if (fdata != null)
            {
                //if (fdata.StatusId > 0)
                //{
                //    predicate = fdata. == 1 ? predicate.And(x => x.IsActive) : predicate;
                //    predicate = fdata.StatusId == 2 ? predicate.And(x => !x.IsActive) : predicate;
                //    predicate = fdata.StatusId == 3 ? predicate.And(x => x.IsLocked) : predicate;
                //}
                if (userId != 1)
                {
                    predicate = predicate.And(x => x.UserId == (fdata.UserId == 0 ? userId : fdata.UserId));
                }


                //predicate = fdata.RoleId == 0 ? predicate : predicate.And(x => x.RoleId == fdata.RoleId);
                //predicate = fdata.PackId == 0 ? predicate : predicate.And(x => x.PackageId == fdata.PackId);
                // predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x=>x.User.MobileNumber==fdata.CustomerNo);
                //  predicate = string.IsNullOrEmpty(fdata.EmailId) ? predicate : predicate.And(x => x.User.Email == fdata.EmailId);
                //predicate = string.IsNullOrEmpty(fdata.ApiKey) ? predicate : predicate.And(x => x.TokenAPI == fdata.ApiKey);
                //predicate = string.IsNullOrEmpty(fdata.IPAddress) ? predicate : predicate.And(x => x.LoginIP == fdata.IPAddress);

            }
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
            List<UserKYC> results = repoKYCsUses
             .Query().Filter(predicate)
             .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(UserKYC), typeof(User) }))
             .GetPage(page, searchModel.length, out totalCount).Distinct()
             .ToList();

            KeyValuePair<int, List<UserKYC>> resultResponse = new KeyValuePair<int, List<UserKYC>>(totalCount, results);

            return resultResponse;

        }

        public bool SignUpUserExists(string Emailid, string Mobileno, string UserName)
        {
            ICollection<User> users = repoAdminUser.Query().AsTracking()
              .Filter(c => c.Username == UserName || c.UserProfile.MobileNumber == Mobileno || c.UserProfile.Email == Emailid)
              .Get().ToList();

            return users.Count > 0 ? true : false;

        }

        public KeyValuePair<int, List<UserKYC>> GetKycDocs(DataTableServerSide searchModel, int userId = 0)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<UserKYC>(searchModel, new Type[] { typeof(UserKYC), typeof(Role) });
            if (fdata != null)
            {
                //if (fdata.StatusId > 0)
                //{
                //    predicate = fdata.StatusId == 1 ? predicate.And(x => x.IsActive) : predicate;
                //    predicate = fdata.StatusId == 2 ? predicate.And(x => !x.IsActive) : predicate;
                //    predicate = fdata.StatusId == 3 ? predicate.And(x => x.IsLocked) : predicate;
                //}

                // predicate = string.IsNullOrEmpty(fdata.adha) ? predicate : predicate.And(x => x.MobileNumber == fdata.CustomerNo);
                //predicate = string.IsNullOrEmpty(fdata.Email) ? predicate : predicate.And(x => x.Email == fdata.Email);
                //predicate = string.IsNullOrEmpty(fdata.ApiKey) ? predicate : predicate.And(x => x.TokenAPI == fdata.ApiKey);
                //predicate = string.IsNullOrEmpty(fdata.IPAddress) ? predicate : predicate.And(x => x.LoginIP == fdata.IPAddress);

            }

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<UserKYC> results = repoKYCsUses
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(User), typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<UserKYC>> resultResponse = new KeyValuePair<int, List<UserKYC>>(totalCount, results);

            return resultResponse;
        }


        public List<User> GetUserListByPackageID(int PackageID = 0)
        {
            return repoAdminUser.Query().Filter(x => (x.PackageId == PackageID) && x.IsActive).Get().ToList();
        }

       

        public List<FireBaseToken> GetFireBaseToken()
        {
            return repoFireBaseToken.Query().Get().ToList();
        }
        
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }
            if (repoAdminRole != null)
            {
                repoAdminRole.Dispose();
                repoAdminRole = null;
            }
            if (repoMenu != null)
            {
                repoMenu.Dispose();
                repoMenu = null;
            }
            if (repoUserBalance != null)
            {
                repoUserBalance.Dispose();
                repoUserBalance = null;
            }
            if (repoStateList != null)
            {
                repoStateList.Dispose();
                repoStateList = null;
            }
            if (repoDocumentTypedList != null)
            {
                repoDocumentTypedList.Dispose();
                repoDocumentTypedList = null;
            }

        }
        #endregion


    }
}
