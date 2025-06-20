using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;


namespace SchoolAPI.Controllers
{
    public static class LunchGenerator
    {
        public static void InsertSampleData(SQLiteConnection conn)
        {
            bool IsEmpty(string tableName)
            {
                var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName}", conn);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }

            if (IsEmpty("Soup"))
            {
                var soups = new[] { "Húsleves", "Paradicsomleves", "Zöldségleves", "Gulyásleves", "Lencseleves" };
                foreach (var s in soups)
                {
                    new SQLiteCommand("INSERT INTO Soup (Name) VALUES (@name)", conn)
                    {
                        Parameters = { new SQLiteParameter("@name", s) }
                    }.ExecuteNonQuery();
                }
            }

            if (IsEmpty("MainDish"))
            {
                var mains = new[] { "Spagetti", "Grill Csirke", "Rántott Hús", "Töltött Csirke", "Halrudacska" };
                foreach (var m in mains)
                {
                    new SQLiteCommand("INSERT INTO MainDish (Name) VALUES (@name)", conn)
                    {
                        Parameters = { new SQLiteParameter("@name", m) }
                    }.ExecuteNonQuery();
                }
            }

            if (IsEmpty("Dessert"))
            {
                var desserts = new[] { "Fagyi", "Süti", "Krémes", "Palacsinta", "Túrós rétes", "Puding", "Gyümölcssaláta" };
                foreach (var d in desserts)
                {
                    new SQLiteCommand("INSERT INTO Dessert (Name) VALUES (@name)", conn)
                    {
                        Parameters = { new SQLiteParameter("@name", d) }
                    }.ExecuteNonQuery();
                }
            }
        }

        public static void GenerateLunchForWeek(SQLiteConnection conn, DateTime startDate)
        {
            var rng = new Random();

            for (int i = 0; i < 5; i++) // hétfőtől péntekig
            {
                var date = startDate.AddDays(i);

                int soupId = GetRandomId(conn, "Soup", rng);
                int mainDishId = GetRandomId(conn, "MainDish", rng);
                int dessertId = GetRandomId(conn, "Dessert", rng);

                var dayName = date.ToString("dddd", new CultureInfo("hu-HU"));

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
    INSERT INTO Lunch (Date, Day, SoupID, MainDishID, DessertID)
    VALUES (@date, @day, @soup, @main, @dessert)";
                cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@day", dayName);
                cmd.Parameters.AddWithValue("@soup", soupId);
                cmd.Parameters.AddWithValue("@main", mainDishId);
                cmd.Parameters.AddWithValue("@dessert", dessertId);
                cmd.ExecuteNonQuery();
            }
        }

        private static int GetRandomId(SQLiteConnection conn, string tableName, Random rng)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT {tableName}ID FROM {tableName}";
            var ids = new List<int>();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                ids.Add(reader.GetInt32(0));

            if (ids.Count == 0) throw new Exception($"Nincs adat a(z) {tableName} táblában.");

            return ids[rng.Next(ids.Count)];
        }

        public static void GenerateFullYearLunch(SQLiteConnection conn, int year)
        {
            string[] weekdays = { "Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek" };
            Random rand = new Random();

            List<int> allSoups = GetIDs(conn, "Soup");
            List<int> allMains = GetIDs(conn, "MainDish");
            List<int> allDesserts = GetIDs(conn, "Dessert");

            // Clear old entries if needed
            new SQLiteCommand("DELETE FROM Lunch", conn).ExecuteNonQuery();

            // Find the first Monday of the year
            DateTime date = new DateTime(year, 1, 1);
            while (date.DayOfWeek != DayOfWeek.Monday)
                date = date.AddDays(1);

            while (date.Year == year)
            {
                // Randomly select 5 for each category, for this week
                List<int> weeklySoups = allSoups.OrderBy(x => rand.Next()).Take(5).ToList();
                List<int> weeklyMains = allMains.OrderBy(x => rand.Next()).Take(5).ToList();
                List<int> weeklyDesserts = allDesserts.OrderBy(x => rand.Next()).Take(5).ToList();

                for (int i = 0; i < 5 && date.Year == year; i++)
                {
                    string dayName = date.ToString("dddd", new System.Globalization.CultureInfo("hu-HU"));

                    int soupId = weeklySoups[i % weeklySoups.Count];
                    int mainId = weeklyMains[i % weeklyMains.Count];
                    int dessertId = weeklyDesserts[i % weeklyDesserts.Count];

                    using var cmd = new SQLiteCommand(
                        "INSERT INTO Lunch (Day, Date, SoupID, MainDishID, DessertID) VALUES (@day, @date, @soup, @main, @dessert)", conn);

                    cmd.Parameters.AddWithValue("@day", dayName); // dinamikus napnév
                    cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@soup", soupId);
                    cmd.Parameters.AddWithValue("@main", mainId);
                    cmd.Parameters.AddWithValue("@dessert", dessertId);

                    cmd.ExecuteNonQuery();

                    date = date.AddDays(1); // go to next weekday
                }

                // Skip weekends
                while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    date = date.AddDays(1);
            }
        }

        private static List<int> ShuffleList(List<int> list, Random rand)
        {
            var shuffled = new List<int>(list);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            return shuffled;
        }

        private static List<int> GetIDs(SQLiteConnection conn, string tableName)
        {
            var cmd = new SQLiteCommand($"SELECT {tableName}ID FROM {tableName}", conn);
            var reader = cmd.ExecuteReader();
            List<int> ids = new();
            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
            reader.Close();
            return ids;
        }
    }
}