namespace Fishley;

public partial class Fishley
{
	public static Dictionary<string, DiscordSlashCommand> Commands = new()
	{
		{ "fish", new RandomFishCommand() },
		{ "speak", new SpeakCommand() },
		{ "balance", new BalanceCommand() },
		{ "fish_database", new EditFish() }
	};

	private static async Task SlashCommandHandler(SocketSlashCommand command)
	{
		var name = command.Data.Name;
		var channel = command.Channel;

		if (Commands.ContainsKey(name))
		{
			var commandClass = Commands[name];

			if (commandClass.SpamOnly && channel != SpamChannel)
			{
				await command.RespondAsync($"You can only use this command in the <#{SpamChannel}> channel.", ephemeral: true);
				return;
			}

			await Commands[name].Function.Invoke(command);
		}
		else
			await command.RespondAsync("That command is unavailable. Bug off now!", ephemeral: true);
	}

	private static async Task ButtonHandler(SocketMessageComponent component)
	{
		var componentId = component.Data.CustomId;

		foreach (var command in Commands)
		{
			if (command.Value.Components.ContainsKey(componentId))
			{
				await command.Value.Components[componentId].Invoke(component);
				return;
			}
		}
	}

	public abstract class DiscordSlashCommand
	{
		public virtual SlashCommandBuilder Builder { get; private set; }
		public virtual Func<SocketSlashCommand, Task> Function { get; private set; }
		public virtual bool SpamOnly { get; private set; } = true;
		public virtual Dictionary<string, Func<SocketMessageComponent, Task>> Components { get; private set; }
	}
}