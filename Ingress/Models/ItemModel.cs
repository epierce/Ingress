using System;
using System.Web;
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;

namespace Ingress.Models
{
    [PetaPoco.TableName("Items")]
    public class ItemModel
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public ItemModel() : base() { }

        public void CreateId()
        {
            if (Id == null) { Id = Guid.NewGuid().ToString(); }
        }
    }

    // Class to handle persisting item details to the database.
    public class ItemMapper
    {
        internal IList<ItemModel> Get()
        {
            return ItemMapper.GetDatabase().Query<ItemModel>("select * from Items order by RANDOM()").ToList();
        }

        internal IList<ItemModel> GetByOwner(string Owner)
        {
            return ItemMapper.GetDatabase().Query<ItemModel>("Select * from Items Where Owner=@0 order by RANDOM()", Owner).ToList();
        }

        public ItemModel GetById(string id)
        {
            return ItemMapper.GetDatabase().SingleOrDefault<ItemModel>("WHERE Id=@0", id);
        }

        public void Add(ItemModel item)
        {
            ItemMapper.GetDatabase().Insert(item);
        }

        internal void update(ItemModel item)
        {
            ItemMapper.GetDatabase().Update(item);
        }

        internal void delete(ItemModel item)
        {
            ItemMapper.GetDatabase().Delete(item);
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