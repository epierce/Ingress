using Nancy;
using System;
using System.Collections.Generic;
using Nancy.ModelBinding;
using Ingress.Models;
using Ingress.Util;

namespace Ingress.Modules
{
    public class NotificationModule : NancyModule
    {
        public NotificationModule() : base("/notifications")
        {
            // http://localhost:8088/notifications
            Get["/"] = parameter => { return GetAll(parameter.target); };

            // http://localhost:8088/notifications/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177
            Get["/{id}"] = parameter => { return GetById(parameter.id); };

            // http://localhost:8088/notifications       POST: Item form in body
            Post["/"] = parameter => { return this.AddNotification(); };

            // http://localhost:8088/notifications/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177    PUT: Item form in body
            Put["/{id}"] = parameter => { return this.UpdateNotification(parameter.id); };

            // http://localhost:8088/notifications/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177    DELETE:  
            Delete["/{id}"] = parameter => { return this.DeleteNotification(parameter.id); };

        }

        private object GetAll(string target)
        {
            try {
                // Create a connection to the PetaPoco ORM and try to fetch and object with the given Id
                NotificationMapper not_mpr = new NotificationMapper();
                IList<NotificationModel> res = null;

                // Get all objects or a filtered list by user
                if (target != null) {
                    res = not_mpr.GetByTarget(target);
                } else {
                    res = not_mpr.Get();
                }

                // Convert this into an array of JSON objects.
                return Response.AsJson(res);
            } catch (Exception e) {
                return HandleException(e, String.Format("NotificationModule.GetAll()"));
            }
        }

        private object GetById(string id)
        {
            try
            {
                // create a connection to the PetaPoco orm and try to fetch and object with the given Id
                NotificationMapper not_mpr = new NotificationMapper();
                NotificationModel notification = not_mpr.GetById(id);

                if (notification == null) {   // a null return means no object found
                    // return a reponse conforming to REST conventions: a 404 error
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("A notification with Id = {0} does not exist", id));
                } else {
                    // success. The Nancy server will automatically serialise this to JSON
                    return Response.AsJson(notification);
                }
            } catch (Exception e) {
                return HandleException(e, String.Format("NotificationModule.GetById({0})", id));
            }
        }

        Nancy.Response AddNotification()
        {
            // capture actual string posted in case the bind fails (as it will if the JSON is bad)
            // need to do it now as the bind operation will remove the data
            String rawBody = this.GetRawBody();

            NotificationModel notification = this.Bind<NotificationModel>();

            // Reject request with an ID param
            if (notification.Id != null)
            {
                return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "POST", HttpStatusCode.Conflict, String.Format("Use PUT to update an existing notification with Id = {0}", notification.Id));
            }
            
            // Save the item to the DB
            try {
                NotificationMapper not_mpr = new NotificationMapper();
                notification.CreateId();
                not_mpr.Add(notification);
                Nancy.Response response = Response.AsJson(notification);
                response.StatusCode = HttpStatusCode.Created;

                string uri = this.Request.Url.SiteBase + this.Request.Path + "/" + notification.Id;
                response.Headers["Location"] = uri;
                
                return response;
            } catch (Exception e) {
                Console.WriteLine(rawBody);
                String operation = String.Format("NotificationModule.AddItem({0})", (notification == null) ? "No Model Data" : notification.Message);
                return HandleException(e, operation);
            }
        }

        Nancy.Response UpdateNotification(string id)
        {

            NotificationModel notification = null;

            // capture actual string posted in case the bind fails (as it will if the JSON is bad)
            // need to do it now as the bind operation will remove the data
            String rawBody = this.GetRawBody();
            
            try {
                notification = this.Bind<NotificationModel>();

                NotificationMapper not_mpr = new NotificationMapper();
                notification.Id = id;

                NotificationModel res = not_mpr.GetById(id);

                if (res == null) {
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("A notification with Id = {0} does not exist", id));
                }
                not_mpr.update(notification);

                Nancy.Response response = Response.AsJson(notification);
                response.StatusCode = HttpStatusCode.OK;

                string uri = this.Request.Url.SiteBase + this.Request.Path + "/" + notification.Id;
                response.Headers["Location"] = uri;

                return response;

            } catch (Exception e) {
                String operation = String.Format("NotificationModule.UpdateBadge({0})", (notification == null) ? "No Model Data" : notification.Message);
                return HandleException(e, operation);
            }
        }

        Nancy.Response DeleteNotification(string id)
        {
            try {
                NotificationMapper not_mpr = new NotificationMapper();
                NotificationModel notification = not_mpr.GetById(id);

                if (notification == null) {
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("A notification with Id = {0} does not exist", id));
                }

                not_mpr.delete(notification);

                return HttpStatusCode.OK;
            } catch (Exception e) {
                return HandleException(e, String.Format("\nNotificationModule.Delete({0})", id));
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