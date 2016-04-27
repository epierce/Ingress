using System;
using System.Web;
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;

namespace Ingress.Models
{
    [PetaPoco.TableName("Users")]
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [PetaPoco.Ignore]
        public IList<ItemModel> Items { get; set; }

        public UserModel() : base() { }

        public void CreateId()
        {
            if (Id == null) { Id = Guid.NewGuid().ToString(); }
        }
    }

    // Class to handle persisting item details to the database.
    public class UserMapper
    {
        internal IList<UserModel> Get()
        {
            String sql = "select * from Users order by Id";
            return UserMapper.GetDatabase().Query<UserModel>(sql).ToList();
        }

        internal IList<string> GetIds()
        {
            return UserMapper.GetDatabase().Query<string>("select Id from Users order by Id").ToList();
        }

        public UserModel GetById(string id)
        {
            UserModel user = UserMapper.GetDatabase().SingleOrDefault<UserModel>("WHERE Id=@0", id);
            user.Items = GetItems(id);

            return user;
        }

        public IList<ItemModel> GetItems(string id)
        {
            return UserMapper.GetDatabase().Query<ItemModel>("SELECT * from Items WHERE Owner=@0 order by Id", id).ToList();
        }

        public void Add(UserModel user)
        {
            UserMapper.GetDatabase().Insert(user);
        }

        internal void update(UserModel user)
        {
            UserMapper.GetDatabase().Update(user);
        }

        internal void delete(UserModel user)
        {
            UserMapper.GetDatabase().Delete(user);
        }

        private static PetaPoco.Database GetDatabase()
        {
            // A sqlite database is just a file.
            string connectionString = "Data Source=" + HttpContext.Current.Server.MapPath("~/ingress.db") + ";Version=3;";
            DbProviderFactory sqlFactory = new System.Data.SQLite.SQLiteFactory();

            PetaPoco.Database db = new PetaPoco.Database(connectionString, sqlFactory);
            return db;
        }
    }
}