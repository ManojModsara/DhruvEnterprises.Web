using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IUserService
    {
        ICollection<Role> GetAdminRole(bool isActive = false);
        User GetUser(int id);
        Role GetUserRole(int roleId);
        KeyValuePair<int, List<User>> GetAdminUsers(DataTableServerSide searchModel, int userId = 0);
        KeyValuePair<int, List<User>> GetAdminCreditUsers(DataTableServerSide searchModel, int userId = 0);
        User Save(User adminUser);
        bool Active(int id);
        bool UserExists(string Emailid, string Mobileno);
        decimal GetUserWalletBalance(int userid);
        decimal GetUserWalletBalanceVW(int userid);
        User GetUserByApiToken(string TokenId);
        List<User> GetUserList(int RoleID = 0);
        List<FireBaseToken> GetFireBaseToken();
        ICollection<View_UserListWithBalance> GetUserListWithBalace(int id = 0);
        KeyValuePair<int, List<NotificationBar>> GetNotificationBars(DataTableServerSide searchModel, int roleid = 0);
        bool ActiveNoteBar(int id);
        NotificationBar Save(NotificationBar notificationBar);
        void Delete(int id);
        NotificationBar GetNotificationBar(int id);
        List<NotificationBar> GetNotificationBarList(byte RoleID = 0);
        List<StateList> GetStates();
        List<KYCTypedDocument> GetKYCDocList();
        KeyValuePair<int, List<UserKYC>> GetKYCsUser(DataTableServerSide searchModel, int userId = 0);

        bool SignUpUserExists(string Emailid, string Mobileno, string UserName);
        KeyValuePair<int, List<UserKYC>> GetKycDocs(DataTableServerSide searchModel, int userId = 0);
        List<User> GetUserListByPackageID(int PackageID = 0);
        
    }
}
