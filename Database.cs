using System;
using System.Collections.Generic;
using System.Linq;


namespace DiscordBot
{
	public class Database
	{
		public Database()
		{

		}

		public void init()
		{
			Utils.runPython("init_tables.py");
		}

		//� terme �a lancera un script python qui lira le fichier data.txt
		public void loadMangas()
		{
			foreach (KeyValuePair<string, string> kvp in Program.mangasData)
			{
				string query = "INSERT INTO mangas (titre, scan) VALUES (?,?)".Replace(' ', ':');
				string values = kvp.Key + ": "/* + kvp.Value*/;
				Utils.runPython("query_executor.py", query, values).Replace(':', '\n');
			}
		}

		public void loadUsers()
		{
			var users = Program.guild.Users;

			foreach (var user in users)
			{
				if (!user.IsBot)
				{
					short admin = 0;

					foreach (var role in user.Roles)
					{
						if (role.Id == 328899154887835678)
						{
							admin = 1;
							break;
						}
					}
					string username = user.Username.Replace(' ', '-');

					if (user.Id == 150338863234154496)
						username = "Fluttershy";

					try
					{
						addUser(user.Id.ToString(), username, "a", admin).aff();
					}
					catch (Exception e)
					{
						Utils.displayException(e, "Database adduser");
					}
				}
			}
		}

		public string addUser(string uid, string pseudo, string prenom = "", short admin = 0)
		{
			string query = "INSERT INTO users (uid, pseudo, prenom, admin) VALUES (?,?,?,?)".Replace(' ', ':');
			string values = uid + ":" + pseudo + ":" + prenom + ":" + admin;

			return Utils.runPython("query_executor.py", query, values).Replace(':', '\n');
		}

		public string addSub(string userId, string mangaId)
		{
			string query = "INSERT INTO subs (user, manga) VALUES (?,?)".Replace(' ', ':');
			string values = userId + ":" + mangaId;

			return Utils.runPython("query_executor.py", query, values).Replace(':', '\n');
		}

		public string get(string table)
		{
			string query = ("SELECT * FROM " + table).Replace(' ', ':');

			return "Table [" + table + "] : \n" + Utils.runPython("query_executor.py", query).Replace(':', '\n');
		}

		public string subTo(string uid, string manga)
		{
			string mangaId = makeQuery("SELECT id FROM mangas WHERE titre=?", manga);
			if (mangaId.Equals(String.Empty))
				return "Le manga '" + manga + "' n'existe pas :/";

			string userId = makeQuery("SELECT id FROM users WHERE uid=?", uid);
			if (userId.Equals(String.Empty))
				return "L'utilisateur n'est pas dans la base de donn�es :/";

			mangaId = Utils.onlyKeepDigits(mangaId);
			userId = Utils.onlyKeepDigits(userId);
			string alreadySub = makeQuery("SELECT id FROM subs WHERE user=? and manga=?", userId + ":" + mangaId);
			if (!alreadySub.Equals(String.Empty))
				return "Tu es d�j� abonn� � ce manga ! :)";

			addSub(userId, mangaId);

			return "Vous vous �tes bien abonn� au manga '" + manga + "'.";
		}

		public string unsubTo(string uid, string manga)
		{
			string mangaId = makeQuery("SELECT id FROM mangas WHERE titre=?", manga);
			if (mangaId.Equals(String.Empty))
				return "Le manga '" + manga + "' n'existe pas :/";

			string userId = makeQuery("SELECT id FROM users WHERE uid=?", uid);
			if (userId.Equals(String.Empty))
				return "L'utilisateur n'est pas dans la base de donn�es :/";

			mangaId = Utils.onlyKeepDigits(mangaId);
			userId = Utils.onlyKeepDigits(userId);
			string alreadySub = makeQuery("SELECT id FROM subs WHERE user=? and manga=?", userId + ":" + mangaId);
			if (alreadySub.Equals(String.Empty))
				return "Tu n'es pas abonn� � ce manga ! :)";

			makeQuery("DELETE FROM subs WHERE user=? and manga=?", userId + ":" + mangaId);

			return "Vous vous �tes bien d�sabonn� du manga '" + manga + "'.";
		}

		public string subList(string uid, string user = "")
		{
			string result = String.Empty;
			string userId = makeQuery("SELECT id FROM users WHERE uid=?", uid);
			if (userId.Equals(String.Empty))
				return "L'utilisateur n'est pas dans la base de donn�es :/";

			if (user.Equals(String.Empty))
			{
				List<string> users = new List<string>();
				string usersFlat = makeQuery("SELECT DISTINCT user FROM subs ORDER BY user");

				if (usersFlat.Contains('\n'))
					users = usersFlat.Split('\n').Select(elem => Utils.onlyKeepDigits(elem)).ToList();
				else
					users.Add(Utils.onlyKeepDigits(usersFlat));

				foreach (string usrId in users)
				{
					string usr = Utils.onlyKeepLetters(makeQuery("SELECT pseudo FROM users WHERE id=?", usrId));
					result += "Abonnements de **" + usr + "** : \n";
					var mangas = Utils.onlyKeepLetters(
						makeQuery("SELECT titre FROM subs JOIN mangas ON(subs.manga=mangas.id) JOIN users ON(subs.user=users.id) WHERE subs.user=?", usrId),
						new List<char>() { '-', '\n' }
						).Split('\n');
					foreach (var manga in mangas)
						result += "\t - " + manga + "\n";
				}

				return result;
			}
			else
			{
				return "pas encore impl�ment�";
			}
		}

		public bool idAdmin(string uid)
		{
			string userId = makeQuery("SELECT id FROM users WHERE uid=?", uid);
			if (userId.Equals(String.Empty))
				return false;

			userId = Utils.onlyKeepDigits(userId);
			string result = makeQuery("SELECT admin FROM users WHERE id=?", userId);
			result = Utils.onlyKeepDigits(result);
			if (result == "1")
				return true;

			return false;
		}

		public string display()
		{
			return Utils.moreThanTwoThousandsChars(Utils.runPython("display.py").Replace(':', '\n'));
		}

		public List<ulong> getSubs(string manga)
		{
			string mangaId = makeQuery("SELECT id FROM mangas WHERE titre=?", manga);
			if (mangaId.Equals(String.Empty))
				throw new Exception("getSub(string manga) : Le manga '" + manga + "' n'existe pas :/");

			List<ulong> users = new List<ulong>();
			mangaId = Utils.onlyKeepDigits(mangaId);

			string result = makeQuery("SELECT uid FROM subs JOIN users ON(subs.user=users.id) WHERE subs.manga=?", mangaId);
			if (result == String.Empty)
				return users;

			foreach (string uid in result.Split('\n'))
			{
				users.Add(Convert.ToUInt64(Utils.onlyKeepDigits(uid)));
			}

			return users;
		}



		private string makeQuery(string query, string values = "")
		{
			return Utils.runPython("query_executor.py", query.Replace(' ', ':'), values).Replace(':', '\n');
		}
	}
}
