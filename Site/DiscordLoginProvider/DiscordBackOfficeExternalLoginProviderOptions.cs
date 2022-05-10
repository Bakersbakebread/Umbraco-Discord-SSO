using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Site.DiscordLoginProvider
{
    public class
        DiscordBackOfficeExternalLoginProviderOptions : IConfigureNamedOptions<BackOfficeExternalLoginProviderOptions>
    {
        private readonly IDiscordRestClientFactory _discordRestClientFactory;
        private readonly IUserService _userService;

        public DiscordBackOfficeExternalLoginProviderOptions(IDiscordRestClientFactory discordRestClientFactory, IUserService userService)
        {
            _discordRestClientFactory = discordRestClientFactory;
            _userService = userService;
        }

        public void Configure(string name, BackOfficeExternalLoginProviderOptions options)
        {
            if (name != "Umbraco." + ApplicationConstants.DiscordLoginProviderName)
                return;

            Configure(options);
        }

        public void Configure(BackOfficeExternalLoginProviderOptions options)
        {
            options.ButtonStyle = "btn-primary";
            options.Icon = "fa fa-sign-in";
            options.AutoLinkOptions = new ExternalSignInAutoLinkOptions(
                // must be true for auto-linking to be enabled
                autoLinkExternalAccount: true,

                // Optionally specify default user group, else
                // assign in the OnAutoLinking callback
                // (default is editor)
                defaultUserGroups: new[] { Constants.Security.EditorGroupAlias },

                // Optionally specify the default culture to create
                // the user as. If null it will use the default
                // culture defined in the web.config, or it can
                // be dynamically assigned in the OnAutoLinking
                // callback.
                defaultCulture: null,
                // Optionally you can disable the ability to link/unlink
                // manually from within the back office. Set this to false
                // if you don't want the user to unlink from this external
                // provider.
                allowManualLinking: false
            )
            {
                // Optional callback
                OnAutoLinking = HandleAutoLinking,
                
                OnExternalLogin = (user, loginInfo) => true,
            };

            // Optionally you can disable the ability for users
            // to login with a username/password. If this is set
            // to true, it will disable username/password login
            // even if there are other external login providers installed.
            options.DenyLocalLogin = false;

            // Optionally choose to automatically redirect to the
            // external login provider so the user doesn't have
            // to click the login button. This is
            options.AutoRedirectLoginToExternalProvider = false;
        }

        private void HandleAutoLinking(BackOfficeIdentityUser autoLinkUser, ExternalLoginInfo loginInfo)
        {
            var userId = loginInfo.ProviderKey;
            var accessToken = loginInfo.AuthenticationTokens.FirstOrDefault(x => x.Name == "access_token")?.Value;
            var isInDiscord = Task.Run(async () => await  _discordRestClientFactory.IsUserInUmbracoDiscord(userId, accessToken));

            if (!isInDiscord.Result) 
                throw new UnauthorizedAccessException("You need to join Umbraco's Discord first!");
            
            // user is in the umbraco Discord, let's give them access right away! 
            autoLinkUser.IsApproved = true;
        }
    }
}