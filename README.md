# AgentFire.Sql.Tools

Contains some helpful Linq-to-Sql (`DataContext`-based) classes, which help you simplify your basic CRUD operations.

# Usage:

    // DbContext is your DataContext class.
    var data = DbEntry<DbContext>.PickDataToArray(db =>
    {
        return from g in db.Groups
               where g.Name.Contains(name)
               select new { Group = g, IsThisConvenient = (bool?)true };
    });
    
## Or:

    int id = 123;

    DbEntry<DbContext>.Crud.Modify<Group>(id, (db, group) =>
    {
        group.Name = "hey";
        group.State = 2;
    });
    
### And more.
