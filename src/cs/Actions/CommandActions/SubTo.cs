
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Actions.CommandActions
{
	public class SubTo : ACommandAction
	{
		public SubTo() : base()
		{
			Name = Prefix + "subto";
			Description = "Permet de s'abonner au manga précisé. (" + Name + " one-piece)";
		}

		public override async Task Invoke(IUserMessage message)
		{
			string msg = string.Empty;
			string message_lower = message.Content.ToLower();

			if (message_lower.Length > 6) {
				msg = Data.DataManager.database.subTo(message.Author.Id.ToString(), message_lower.Split(' ')[1]);
			}
			else {
				msg = "Il faut mettre un titre de manga. Ex : " + Name + " one-piece";
			}

			await message.Channel.SendMessagesAsync(msg);
		}
	}
}
