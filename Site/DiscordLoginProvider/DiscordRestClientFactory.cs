using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;

namespace Site.DiscordLoginProvider
{
    public interface IDiscordRestClientFactory
    {
        /// <summary>
        /// Check if the user is in the Umbraco Discord Guild, otherwise they can't register! 
        /// </summary>
        /// <param name="userId">The user attempting to auto-link</param>
        /// <param name="accessToken">The user attempting to auto-link's access token</param>
        /// <returns></returns>
        Task<bool> IsUserInUmbracoDiscord(string userId, string accessToken);
    }

    public class DiscordRestClientFactory : IDiscordRestClientFactory
    {
        private const string _umbracoDiscordGuildId = "869656431308189746";

        private async Task<DiscordRestClient> CreateClient(string userId, string accessToken)
        {
            var discordRestClient = new DiscordRestClient();
            await discordRestClient.LoginAsync(TokenType.Bearer, accessToken);

            return discordRestClient;
        }
        
        /// <summary>
        /// Check if the user is in the Umbraco Discord Guild, otherwise they can't register! 
        /// </summary>
        /// <param name="userId">The user attempting to auto-link</param>
        /// <param name="accessToken">The user attempting to auto-link's access token</param>
        /// <returns></returns>
        public async Task<bool> IsUserInUmbracoDiscord(string userId, string accessToken)
        {
            await using var client = await CreateClient(userId, accessToken);
            
            var guildsFlattened = client.GetGuildSummariesAsync();
            var flattened = await guildsFlattened.FlattenAsync();
            
            var umbracoGuild = flattened.FirstOrDefault(x => x.Id == ulong.Parse(_umbracoDiscordGuildId));

            return umbracoGuild != null;
        }
    }
}