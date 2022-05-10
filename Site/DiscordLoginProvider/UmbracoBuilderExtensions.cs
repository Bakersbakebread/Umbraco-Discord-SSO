using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace Site.DiscordLoginProvider
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddDiscordAuthentication(this IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IDiscordRestClientFactory, DiscordRestClientFactory>();
            
            // Register DiscordBackOfficeExternalLoginProviderOptions here rather than require it in startup
            builder.Services.ConfigureOptions<DiscordBackOfficeExternalLoginProviderOptions>();

            builder.AddBackOfficeExternalLogins(logins => { logins.AddBackOfficeLogin(BuildDiscord); });

            return builder;
        }

        private static void BuildDiscord(BackOfficeAuthenticationBuilder backOfficeAuthenticationBuilder)
        {
            backOfficeAuthenticationBuilder.AddDiscord(
                backOfficeAuthenticationBuilder.SchemeForBackOffice(ApplicationConstants.DiscordLoginProviderName),
                options =>
                {
                    options.ClientId = "YourDiscordClientId";
                    options.ClientSecret = "YourDiscordSecret";
                
                    // we need to save these tokens so we can access them to make requests against discord api
                    options.SaveTokens = true;

                    options.Scope.Add("email");
                    options.Scope.Add("guilds");
                });
        }
    }
}