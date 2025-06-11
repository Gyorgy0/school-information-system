
using System.Data.SQLite;

public static class DatabaseConnector
{
    private static SQLiteConnection? conn;

    public static SQLiteConnection Db()
    {
        if (conn == null)
        {
            string? projectRoot = GetProjectRoot();

            if (projectRoot == null)
            {
                throw new InvalidOperationException("Nem siker�lt meghat�rozni a projekt gy�k�rk�nyvt�r�t!");
            }

            string dbPath = Path.Combine(projectRoot, "database", "edupage.sqlite3");

            if (!File.Exists(dbPath))
            {
                Console.WriteLine("HIBA: Az adatb�zis f�jl nem tal�lhat�!");
                throw new FileNotFoundException("Az adatb�zis nem tal�lhat� a megadott helyen.", dbPath);
            }

            conn = new SQLiteConnection($"Data Source={dbPath}");
            conn.Open();
        }
        else if (conn.State == System.Data.ConnectionState.Closed)
        {
            conn.Open();
        }

        return conn;
    }

    public static SQLiteConnection CreateNewConnection()
    {
        string? projectRoot = GetProjectRoot();

        if (projectRoot == null)
        {
            throw new InvalidOperationException("Nem siker�lt meghat�rozni a projekt gy�k�rk�nyvt�r�t!");
        }

        string dbPath = Path.Combine(projectRoot, "database", "edupage.sqlite3");

        var conn = new SQLiteConnection($"Data Source={dbPath}");
        conn.Open();
        return conn;
    }

    private static string? GetProjectRoot()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        DirectoryInfo? dir = new DirectoryInfo(baseDirectory);

        for (int i = 0; i < 3; i++)
        {
            if (dir?.Parent == null) return null;
            dir = dir.Parent;
        }

        return dir?.FullName;
    }
}