namespace Fishley;

public partial class Fishley
{
	public class EditFish : DiscordSlashCommand
	{
		public override SlashCommandBuilder Builder => new SlashCommandBuilder()
		.WithName("fish_database")
		.WithDescription("Access the fish database")
		.AddOption(new SlashCommandOptionBuilder()
			.WithName("edit")
			.WithDescription("Edit a fish (This clears the reported issues count too)")
			.WithType(ApplicationCommandOptionType.SubCommand)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("fish_id")
				.WithDescription("Which fish to edit")
				.WithRequired(true)
				.WithType(ApplicationCommandOptionType.Integer)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("common_name")
				.WithDescription("Change the common name")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.String)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("page_name")
				.WithDescription("Change the page name")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.String)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("wiki_page")
				.WithDescription("Change the wiki page")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.String)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("wiki_info_page")
				.WithDescription("Change the wiki info page")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.String)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("monthly_views")
				.WithDescription("Change the monthly views")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.Integer)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("image_link")
				.WithDescription("Change the image link")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.String)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("rarity")
				.WithDescription("Change the rarity")
				.WithRequired(false)
				.WithType(ApplicationCommandOptionType.String)
				.AddChoice("F-", "F-")
				.AddChoice("F", "F")
				.AddChoice("F+", "F+")
				.AddChoice("E-", "E-")
				.AddChoice("E", "E")
				.AddChoice("E+", "E+")
				.AddChoice("D-", "D-")
				.AddChoice("D", "D")
				.AddChoice("D+", "D+")
				.AddChoice("C-", "C-")
				.AddChoice("C", "C")
				.AddChoice("C+", "C+")
				.AddChoice("B-", "B-")
				.AddChoice("B", "B")
				.AddChoice("B+", "B+")
				.AddChoice("A-", "A-")
				.AddChoice("A", "A")
				.AddChoice("A+", "A+")
				.AddChoice("S-", "S-")
				.AddChoice("S", "S")
				.AddChoice("S+", "S+")
			)
		)
		.AddOption(new SlashCommandOptionBuilder()
			.WithName("remove")
			.WithDescription("Remove a fish")
			.WithType(ApplicationCommandOptionType.SubCommand)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("fish_id")
				.WithDescription("Which fish to remove")
				.WithRequired(true)
				.WithType(ApplicationCommandOptionType.Integer)
			)
		)
		.AddOption(new SlashCommandOptionBuilder()
			.WithName("view")
			.WithDescription("View a fish")
			.WithType(ApplicationCommandOptionType.SubCommand)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("fish_id")
				.WithDescription("Which fish to view")
				.WithRequired(true)
				.WithType(ApplicationCommandOptionType.Integer)
			)
		)
		.AddOption(new SlashCommandOptionBuilder()
			.WithName("add")
			.WithDescription("Add a fish")
			.WithType(ApplicationCommandOptionType.SubCommand)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("wiki_page")
				.WithDescription("Add a wiki page to the fish database (Will fetch it and fill out all the stats)")
				.WithRequired(true)
				.WithType(ApplicationCommandOptionType.String)
			)
			.AddOption(new SlashCommandOptionBuilder()
				.WithName("common_name")
				.WithDescription("The common name of the fish")
				.WithRequired(true)
				.WithType(ApplicationCommandOptionType.String)
			)
		)
		.AddOption(new SlashCommandOptionBuilder()
			.WithName("most_issues")
			.WithDescription("Return a list of fishes with the most issues reported")
			.WithType(ApplicationCommandOptionType.SubCommand)
		)
		.WithDefaultMemberPermissions(GuildPermission.Administrator);

		public override Func<SocketSlashCommand, Task> Function => ChangeFish;
		public override bool SpamOnly => false;

		public async Task ChangeFish(SocketSlashCommand command)
		{
			if (!IsAdmin((SocketGuildUser)command.User))
			{
				await command.RespondAsync($"NOT AN ADMIN BUG OFF", ephemeral: true);
				return;
			}

			var commandType = command.Data.Options.First();

			switch (commandType.Name)
			{
				case "edit":
					{
						var fishId = (int)(long)commandType.Options.First().Value;
						var commonName = (string)commandType.Options.FirstOrDefault(x => x.Name == "common_name")?.Value ?? null;
						var pageName = (string)commandType.Options.FirstOrDefault(x => x.Name == "page_name")?.Value ?? null;
						var wikiPage = (string)commandType.Options.FirstOrDefault(x => x.Name == "wiki_page")?.Value ?? null;
						var wikiInfoPage = (string)commandType.Options.FirstOrDefault(x => x.Name == "wiki_info_page")?.Value ?? null;
						var monthlyViews = (int)(long)(commandType.Options.FirstOrDefault(x => x.Name == "monthly_views") != null ? commandType.Options.FirstOrDefault(x => x.Name == "monthly_views").Value : -1L);
						var imageLink = (string)commandType.Options.FirstOrDefault(x => x.Name == "image_link")?.Value ?? null;
						var rarity = (string)commandType.Options.FirstOrDefault(x => x.Name == "rarity")?.Value ?? null;

						var fishData = await GetFish(fishId);

						if (fishData == null)
						{
							await command.RespondAsync($"Could not find any fish with the id {fishId}.", ephemeral: true);
							return;
						}

						if (commonName != null)
							fishData.CommonName = commonName;
						if (pageName != null)
							fishData.PageName = pageName;
						if (wikiPage != null)
							fishData.WikiPage = wikiPage;
						if (wikiInfoPage != null)
							fishData.WikiInfoPage = wikiInfoPage;
						if (monthlyViews >= 0)
							fishData.MonthlyViews = monthlyViews;
						if (imageLink != null)
							fishData.ImageLink = imageLink;
						if (rarity != null)
							fishData.Rarity = rarity;

						fishData.IssuesReported = 0; // Supposedly it's been fixed

						await UpdateOrCreateFish(fishData);

						var embed = new FishEmbedBuilder(fishData, "Fish has been updated").Build();
						await command.RespondAsync(embed: embed);
						DebugSay($"Updated fish: {fishData.WikiPage}");
					}
					break;
				case "remove":
					{
						var fishId = (int)(long)commandType.Options.First().Value;
						var fishData = await GetFish(fishId);
						var fishRemoved = await RemoveFish(fishId);

						if (fishData == null || !fishRemoved)
						{
							await command.RespondAsync($"Could not find any fish with the id {fishId}.", ephemeral: true);
							return;
						}

						var embed = new FishEmbedBuilder(fishData, "Fish has been removed").Build();
						await command.RespondAsync(embed: embed);
						DebugSay($"Removed from database fish: {fishData.WikiPage}");
					}
					break;
				case "view":
					{
						var fishId = (int)(long)commandType.Options.First().Value;
						var fishData = await GetFish(fishId);

						if (fishData == null)
						{
							await command.RespondAsync($"Could not find any fish with the id {fishId}.", ephemeral: true);
							return;
						}

						var embed = new FishEmbedBuilder(fishData).Build();
						await command.RespondAsync(embed: embed);
					}
					break;
				case "add":
					{
						var wikiPage = (string)commandType.Options.First().Value;
						var commonName = (string)commandType.Options.Last().Value;

						await command.RespondAsync($"Fetching page...", ephemeral: true);
						var channel = (SocketTextChannel)command.Channel;

						var fishResponse = await AddFish(wikiPage, commonName);

						if (!fishResponse.Item1)
						{
							await SendMessage(channel, $"Couldn't add: {fishResponse.Item2}", deleteAfterSeconds: 5);
							return;
						}

						var addedFish = await GetFish(fishResponse.Item3);
						var embed = new FishEmbedBuilder(addedFish, "Fish has been added").Build();
						await SendMessage(channel, $"<@{command.User.Id}>", embed: embed);
					}
					break;
				case "most_issues":
					{
						using (var db = new FishleyDbContext())
						{
							var fishes = db.Fishes;
							var foundFishes = fishes.AsAsyncEnumerable();
							var mostIssues = foundFishes.OrderByDescending(x => x.IssuesReported);
							var top5 = await mostIssues.Take(5).ToListAsync();

							var embedBuilder = new EmbedBuilder().WithTitle("Fishes with most issues reported")
								.WithCurrentTimestamp();

							var spot = 1;
							foreach (var fish in top5)
							{
								if (fish.IssuesReported <= 0) break;
								embedBuilder = embedBuilder.AddField($"#{spot}: {fish.CommonName}", $"Times Reported: {fish.IssuesReported} - Fish Id: {fish.PageId}");
								spot++;
							}

							if (spot == 1)
								await command.RespondAsync("No issues found, good job team :-)");
							else
								await command.RespondAsync(embed: embedBuilder.Build());
						}
					}
					break;
			}
		}
	}
}