using System;
using System.Web;
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;

namespace Ingress.Models
{
    [PetaPoco.TableName("Notifications")]
    public class NotificationModel
    {
        public string Id { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }

        public NotificationModel() : base() { }

        public void CreateId()
        {
            if (Id == null) { Id = Guid.NewGuid().ToString(); }
        }
    }

    // Class to handle persisting item details to the database.
    public class NotificationMapper
    {
        internal IList<NotificationModel> Get()
        {
            return NotificationMapper.GetDatabase().Query<NotificationModel>("Select * from Notifications").ToList();
        }

        internal IList<NotificationModel> GetByTarget(string Target)
        {
            return NotificationMapper.GetDatabase().Query<NotificationModel>("Select * from Items Where Target=@0", Target).ToList();
        }

        public NotificationModel GetById(string id)
        {
            return NotificationMapper.GetDatabase().SingleOrDefault<NotificationModel>("WHERE Id=@0", id);
        }

        public void Add(NotificationModel item)
        {
            NotificationMapper.GetDatabase().Insert(item);
        }

        internal void update(NotificationModel item)
        {
            NotificationMapper.GetDatabase().Update(item);
        }

        internal void delete(NotificationModel item)
        {
            NotificationMapper.GetDatabase().Delete(item);
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