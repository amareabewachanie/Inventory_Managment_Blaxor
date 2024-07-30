using Inventory.Application.DTO.Request.Identity;
using Inventory.Application.DTO.Response;
using Inventory.Application.DTO.Response.Identity;
using Inventory.Application.Extensions.Identity;
using Inventory.Application.Interface.Identity;
using Inventory.Infrastructure.DataAcsess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Inventory.Application.DTO.Request.ActivityTracker;
using Inventory.Application.DTO.Response.ActivityTracker;
using Inventory.Domain.Entities;
using Mapster;

namespace Inventory.Infrastructure.Repository
{
    public class Account : IAccount
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public Account(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public async Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model)
        {
           var user = await FindUserByEmail(model.Email);
            if(user is not null)
            {
                return new ServiceResponse(false, "User already exists");
            }
            var newUser = new ApplicationUser()
            {
                UserName = model.Email,
                PasswordHash = model.Password,
                Email = model.Email,
                Name = model.Name
            };
            var result = CheckResult(await _userManager.CreateAsync(newUser, model.Password));
            if (!result.Flag) return result;
            return await CreateUserClaims(model);
        }

        public async Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync()
        {
            var userList = new List<GetUserWithClaimResponseDTO>();
            var allUsers = await _userManager.Users.ToListAsync();
            if (allUsers.Count == 0) return userList;

            foreach (var user in allUsers)
            {
                var currentUser = await _userManager.FindByIdAsync(user.Id);
                var getCurrentUserClaims = await _userManager.GetClaimsAsync(currentUser!);
                if (getCurrentUserClaims.Any())
                    userList.Add(new GetUserWithClaimResponseDTO
                    {
                        UserId = user.Id,
                        Email = getCurrentUserClaims.FirstOrDefault(_ => _.Type == ClaimTypes.Email)!.Value,
                        RoleName = getCurrentUserClaims.FirstOrDefault(_=>_.Type == ClaimTypes.Role)!.Value,
                        Name = getCurrentUserClaims.FirstOrDefault(_=>_.Type == "Name")!.Value,
                        ManageUser = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "ManageUser")!.Value),
                        Create = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Create")!.Value),
                        Update = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Update")!.Value),
                        Read = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Read")!.Value),
                        Delete = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Delete")!.Value),
                    });                 
            }
            return userList;
        }

        public async Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model)
        {
            var user = await FindUserByEmail(model.Email);
            if (user is null) return new ServiceResponse(false, "User not found");

            var verifyPassword = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!verifyPassword.Succeeded) return new ServiceResponse(false, "Incorrect credentials provided");
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded) return new ServiceResponse(false, "Unknown error occurred while logging you in");
            return new ServiceResponse(true, null);
        }

        public async Task SetUpAsync() => await CreateUserAsync(new CreateUserRequestDTO
            {
                Name = "Administrator",
                Email = "admin@admin.com",
                Password = "Admin@123",
                Policy = Policy.AdminPolicy
            });

        public async Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null) return new ServiceResponse(false, "User not found");

            var oldUserClaims = await _userManager.GetClaimsAsync(user);
            Claim[] newUserClaims= new [] {
                     new Claim(ClaimTypes.Email, user.Email!),
                     new Claim(ClaimTypes.Role, model.RoleName),
                     new Claim("Name", model.Name),
                     new Claim("Create", model.Create.ToString()),
                     new Claim("Update", model.Update.ToString()),
                     new Claim("Delete", model.Delete.ToString()),
                     new Claim("Read", model.Read.ToString()),
                     new Claim("ManageUser", model.ManageUser.ToString())
                };
            var result = await _userManager.RemoveClaimsAsync(user, oldUserClaims);
            var response = CheckResult(result);
            if (!response.Flag)
                return new ServiceResponse(false, response.Message!);
            var addNewClaims = await _userManager.AddClaimsAsync(user, newUserClaims);
            var outcome = CheckResult(addNewClaims);
            if (outcome.Flag)
                return new ServiceResponse(true, "User Updated");
            return outcome;
        }

        public async Task SaveActivityAsync(ActivityTrackerRequestDTO model)
        {
            _context.ActivityTrackers.Add(model.Adapt(new ActivityTracker()));
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync()
        {
            var list = new List<ActivityTrackerResponseDTO>();
            var data = (await _context.ActivityTrackers.ToListAsync()).Adapt<List<ActivityTrackerResponseDTO>>();
            foreach (var activity in data)
            {
                activity.UserName = (await _userManager.FindByIdAsync(activity.CreatedBy)).Name;
                list.Add(activity);
            }

            return list;
        }

        private async Task<ServiceResponse> CreateUserClaims(CreateUserRequestDTO model)
        {
            if (string.IsNullOrEmpty(model.Policy)) return new ServiceResponse(false, "No policy specified");
            Claim[] userClaims = new Claim[]{};
            if (model.Policy.Equals(Policy.AdminPolicy, StringComparison.OrdinalIgnoreCase))
            {
                userClaims = new[]
                {
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("Name", model.Name),
                    new Claim("Create", "true"),
                    new Claim("Update", "true"),
                    new Claim("Delete", "true"),
                    new Claim("Read", "true"),
                    new Claim("ManageUser", "true")
                };
            }
            else if (model.Policy.Equals(Policy.ManagerPolicy, StringComparison.OrdinalIgnoreCase))
            {
                userClaims = new []{
                     new Claim(ClaimTypes.Email, model.Email),
                     new Claim(ClaimTypes.Role, "Manager"),
                     new Claim("Name", model.Name),
                     new Claim("Create", "true"),
                     new Claim("Update", "true"),
                     new Claim("Delete", "false"),
                     new Claim("Read", "true"),
                     new Claim("ManageUser", "false")
                    };
            }
            else if (model.Policy.Equals(Policy.UserPolicy, StringComparison.OrdinalIgnoreCase))
            {
                userClaims = new []{
                     new Claim(ClaimTypes.Email, model.Email),
                     new Claim(ClaimTypes.Role, "Manager"),
                     new Claim("Name", model.Name),
                     new Claim("Create", "false"),
                     new Claim("Update", "fales"),
                     new Claim("Delete", "false"),
                     new Claim("Read", "false"),
                     new Claim("ManageUser", "false")
                    };
            }
            var result = CheckResult(await _userManager.AddClaimsAsync(await FindUserByEmail(model.Email), userClaims));
            if (result.Flag)
                return new ServiceResponse(true, "User Created");
            return result;
        }
        private async Task<ApplicationUser> FindUserByEmail(string email) => (await _userManager.FindByEmailAsync(email))!;
        private static ServiceResponse CheckResult(IdentityResult result)
        {
            if (result.Succeeded) return new ServiceResponse(true, null);
            var errors = result.Errors.Select(_ => _.Description);
            return new ServiceResponse(false, string.Join(Environment.NewLine, errors));
        }
    }
}
