using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using HtmlAgilityPack;
using Supremes;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;


namespace DiscordBot
{
	class Database
	{
		public Database()
		{

		}

		public void init()
		{
			Utils.runPython("init_tables.py");
		}

		//� terme �a lancera un script python qui lira le fichier data.txt
		public void loadMangas(SortedDictionary<string, string> mangasData)
		{
			foreach (KeyValuePair<string, string> kvp in mangasData)
			{
				string query = "INSERT INTO mangas (titre, scan) VALUES (?,?)".Replace(' ', ':');
				string values = kvp.Key + ": "/* + kvp.Value*/;
				Utils.runPython("query_executor.py", query, values).Replace(':', '\n');
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
			//SELECT titre FROM subs JOIN mangas ON(subs.manga=mangas.id) JOIN users ON(subs.user=users.id) WHERE pseudo='ferrone'
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
					/*result += Utils.onlyKeepLetters(
						makeQuery("SELECT titre FROM subs JOIN mangas ON(subs.manga=mangas.id) JOIN users ON(subs.user=users.id) WHERE subs.user=?", usrId), 
						new List<char>() { '-', '\n' }
						) + "\n";*/
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

			}
			return String.Empty;
		}

		public bool idAdmin(string uid)
		{
			return false;
		}

		public List<string> display()
		{
			return Utils.moreThanTwoThousandsChars(Utils.runPython("aa.py").Replace(':', '\n'));
		}



		private string makeQuery(string query, string values = "")
		{
			return Utils.runPython("query_executor.py", query.Replace(' ', ':'), values).Replace(':', '\n');
		}
	}
}
