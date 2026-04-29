using Microsoft.AspNetCore.Identity;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Services
{
    public class NumericTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
        where TUser : class
    {
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(false);
        }
    }
}
