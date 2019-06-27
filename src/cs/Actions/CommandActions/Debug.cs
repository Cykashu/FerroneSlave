
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Actions.CommandActions
{
	public class Debug : ACommandAction
	{
		public Debug() : base()
		{
			Name = Prefix + "d";
			Description = "Utilisé pour le debug.";
			Accessibility = AccessibilityType.Invisible;
		}

		public override async Task Invoke(IUserMessage message)
		{
			try {
				
			}
			catch (System.Exception e) {
				e.DisplayException(Name);
				await message.Channel.SendMessagesAsync(e.Message + "\n" + e.StackTrace);
			}
		}
	}
}
