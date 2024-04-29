using LiteDB;
using Microsoft.Extensions.Logging;

public class LiteDbContext
{
    private readonly LiteDatabase _db;

    public LiteDbContext()
    {
        _db = new LiteDatabase("apps.db");
    }

    public ILiteCollection<AppInfo> Apps => _db.GetCollection<AppInfo>("apps");

    public List<AppInfo> GetAllApps()
    {
        return Apps.FindAll().ToList();
    }

    public void InsertApp(AppInfo app)
    {
        Apps.Insert(app);
    }
    public void RemoveApp(string name)
    {
        Apps.DeleteMany(a => a.Name == name);
    }
    public void Dispose()
    {
        _db.Dispose();
    }
}
