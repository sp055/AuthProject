using Microsoft.AspNetCore.Identity;

namespace AuthProject.Models.Validaotrs
{
    public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : IdentityUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            bool result = true;
            char[] charPass = password.ToCharArray();
            var group = charPass.OrderBy(o => o).GroupBy(x => x).Select(g => g.Count());

            if (group.Any(v => v > 1))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "Not unique characers",
                    Description = "Password must contain only unique characters"
                }));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}