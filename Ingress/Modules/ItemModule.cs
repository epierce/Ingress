using Nancy;
using System;
using System.Collections.Generic;
using Nancy.ModelBinding;
using Ingress.Models;
using Ingress.Util;

namespace Ingress.Modules
{
    public class ItemModule : NancyModule
    {
        public ItemModule() : base("/items")
        {
            // http://localhost:8088/items
            Get["/"] = parameter => { return GetAll(parameter.owner); };

            // http://localhost:8088/items/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177
            Get["/{id}"] = parameter => { return GetById(parameter.id); };

            // http://localhost:8088/items       POST: Item form in body
            Post["/"] = parameter => { return this.AddItem(); };

            // http://localhost:8088/items/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177    PUT: Item form in body
            Put["/{id}"] = parameter => { return this.UpdateItem(parameter.id); };

            // http://localhost:8088/items/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177    DELETE:  
            Delete["/{id}"] = parameter => { return this.DeleteItem(parameter.id); };

        }

        private object GetAll(string owner)
        {
            try {
                // Create a connection to the PetaPoco ORM and try to fetch and object with the given Id
                ItemMapper itm_mpr = new ItemMapper();
                IList<ItemModel> res = null;

                // Get all objects or a filtered list by user
                if (owner != null) {
                    res = itm_mpr.GetByOwner(owner);
                } else {
                    res = itm_mpr.Get();
                }

                // Convert this into an array of JSON objects.
                // string json = JsonConvert.SerializeObject(products, Formatting.Indented);
                return Response.AsJson(res);
            } catch (Exception e) {
                return HandleException(e, String.Format("ItemModule.GetAll()"));
            }
        }

        private object GetById(string id)
        {
            try
            {
                // create a connection to the PetaPoco orm and try to fetch and object with the given Id
                ItemMapper itm_mpr = new ItemMapper();
                ItemModel item = itm_mpr.GetById(id);

                if (item == null) {   // a null return means no object found
                    // return a reponse conforming to REST conventions: a 404 error
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("An Item with Id = {0} does not exist", id));
                } else {
                    // success. The Nancy server will automatically serialise this to JSON
                    return Response.AsJson(item);
                }
            } catch (Exception e) {
                return HandleException(e, String.Format("ItemModule.GetById({0})", id));
            }
        }

        Nancy.Response AddItem()
        {
            // capture actual string posted in case the bind fails (as it will if the JSON is bad)
            // need to do it now as the bind operation will remove the data
            String rawBody = this.GetRawBody();

            ItemModel item = this.Bind<ItemModel>();

            // Reject request with an ID param
            if (item.Id != null)
            {
                return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "POST", HttpStatusCode.Conflict, String.Format("Use PUT to update an existing Item with Id = {0}", item.Id));
            }
            
            // Save the item to the DB
            try {
                ItemMapper itm_mpr = new ItemMapper();
                item.CreateId();
                itm_mpr.Add(item);
                Nancy.Response response = Response.AsJson(item);
                response.StatusCode = HttpStatusCode.Created;

                string uri = this.Request.Url.SiteBase + this.Request.Path + "/" + item.Id;
                response.Headers["Location"] = uri;
                
                return response;
            } catch (Exception e) {
                Console.WriteLine(rawBody);
                String operation = String.Format("ItemModule.AddItem({0})", (item == null) ? "No Model Data" : item.Url);
                return HandleException(e, operation);
            }
        }

        Nancy.Response UpdateItem(string id)
        {

            ItemModel item = null;

            // capture actual string posted in case the bind fails (as it will if the JSON is bad)
            // need to do it now as the bind operation will remove the data
            String rawBody = this.GetRawBody();
            
            try {
                item = this.Bind<ItemModel>();

                ItemMapper itm_mpr = new ItemMapper();
                item.Id = id;

                ItemModel res = itm_mpr.GetById(id);

                if (res == null) {
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("An Item with Id = {0} does not exist", id));
                }
                itm_mpr.update(item);

                Nancy.Response response = Response.AsJson(item);
                response.StatusCode = HttpStatusCode.OK;

                string uri = this.Request.Url.SiteBase + this.Request.Path + "/" + item.Id;
                response.Headers["Location"] = uri;

                return response;

            } catch (Exception e) {
                String operation = String.Format("ItemModule.UpdateBadge({0})", (item == null) ? "No Model Data" : item.Url);
                return HandleException(e, operation);
            }
        }

        Nancy.Response DeleteItem(string id)
        {
            try {
                ItemMapper itm_mpr = new ItemMapper();
                ItemModel item = itm_mpr.GetById(id);

                if (item == null) {
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("An Item with Id = {0} does not exist", id));
                }

                itm_mpr.delete(item);

                return HttpStatusCode.OK;
            } catch (Exception e) {
                return HandleException(e, String.Format("\nItemModule.Delete({0})", id));
            }
        }

        Nancy.Response HandleException(Exception e, String operation)
        {
            String errorContext = String.Format("{1}:{2}: {3} Exception caught in: {0}", operation, DateTime.UtcNow.ToShortDateString(), DateTime.UtcNow.ToShortTimeString(), e.GetType());

            // write info to the server log. 
            Console.WriteLine("----------------------\n{0}\n{1}\n--------------------", errorContext, e.Message);
            if (e.InnerException != null) Console.WriteLine("{0}\n--------------------", e.InnerException.Message);

            // Return generic message to user
            return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.InternalServerError, "Operational difficulties");
        }

        private String GetRawBody()
        {
            // Read the body as a raw string
            byte[] b = new byte[this.Request.Body.Length];
            this.Request.Body.Read(b, 0, Convert.ToInt32(this.Request.Body.Length));
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            String bodyData = encoding.GetString(b);

            return bodyData;
        }
    }
}