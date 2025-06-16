using System;
using System.Collections.Generic;
using System.Data.SQLite;


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

        public static void GenerateWeeklyLunch(SQLiteConnection conn)
        {
            string[] weekdays = { "Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek" };
            Random rand = new Random();

            List<int> soups = GetIDs(conn, "Soup");
            List<int> mains = GetIDs(conn, "MainDish");
            List<int> desserts = GetIDs(conn, "Dessert");

            new SQLiteCommand("DELETE FROM Lunch", conn).ExecuteNonQuery();

            // Shuffle lists
            soups = ShuffleList(soups, rand);
            mains = ShuffleList(mains, rand);
            desserts = ShuffleList(desserts, rand);

            for (int i = 0; i < weekdays.Length; i++)
            {
                int soupId = soups[i % soups.Count];       // If fewer than 5 items, repeat but only if needed
                int mainId = mains[i % mains.Count];
                int dessertId = desserts[i % desserts.Count];

                var cmd = new SQLiteCommand("INSERT INTO Lunch (Day, SoupID, MainDishID, DessertID) VALUES (@day, @soup, @main, @dessert)", conn);
                cmd.Parameters.AddWithValue("@day", weekdays[i]);
                cmd.Parameters.AddWithValue("@soup", soupId);
                cmd.Parameters.AddWithValue("@main", mainId);
                cmd.Parameters.AddWithValue("@dessert", dessertId);
                cmd.ExecuteNonQuery();
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