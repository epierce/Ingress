using Nancy;
using System;
using System.Collections.Generic;
using Nancy.ModelBinding;
using Ingress.Models;
using Ingress.Util;

namespace Ingress.Modules
{
    public class UserModule : NancyModule
    {
        public UserModule() : base("/users")
        {
            // http://localhost:8088/users
            Get["/"] = _ => { return GetAll(); };

            // http://localhost:8088/users/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177
            Get["/{id}"] = parameter => { return GetById(parameter.id); };

            // http://localhost:8088/users       POST: User form in body
            Post["/"] = parameter => { return this.AddUser(); };

            // http://localhost:8088/users/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177    PUT: User form in body
            Put["/{id}"] = parameter => { return this.UpdateItem(parameter.id); };

            // http://localhost:8088/users/1a43e3b8-85b7-411a-b9b8-5ed27ccd9177    DELETE:  
            Delete["/{id}"] = parameter => { return this.DeleteUser(parameter.id); };
        }

        private object GetAll()
        {
            try {
                // Create a connection to the PetaPoco ORM and try to fetch and object with the given Id
                UserMapper usr_mpr = new UserMapper();

                // Get all objects
                IList<UserModel> res = usr_mpr.Get();

                // Convert this into an array of JSON objects.
                return Response.AsJson(res);
            } catch (Exception e) {
                return HandleException(e, String.Format("UserModule.GetAll()"));
            }
        }

        private object GetById(string id)
        {
            try
            {
                // create a connection to the PetaPoco orm and try to fetch and object with the given Id
                UserMapper usr_mpr = new UserMapper();
                UserModel item = usr_mpr.GetById(id);

                if (item == null) {   // a null return means no object found
                    // return a reponse conforming to REST conventions: a 404 error
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("An Item with Id = {0} does not exist", id));
                } else {
                    // success. The Nancy server will automatically serialise this to JSON
                    return Response.AsJson(item);
                }
            } catch (Exception e) {
                return HandleException(e, String.Format("UserModule.GetById({0})", id));
            }
        }

        Nancy.Response AddUser()
        {
            // capture actual string posted in case the bind fails (as it will if the JSON is bad)
            // need to do it now as the bind operation will remove the data
            String rawBody = this.GetRawBody();

            UserModel user = this.Bind<UserModel>();

            // Reject request with an ID param
            if (user.Id != null)
            {
                return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "POST", HttpStatusCode.Conflict, String.Format("Use PUT to update an existing user with Id = {0}", user.Id));
            }
            
            // Save the item to the DB
            try {
                UserMapper usr_mpr = new UserMapper();
                user.CreateId();
                usr_mpr.Add(user);

                //Get new user data from DB
                UserModel new_user = usr_mpr.GetById(user.Id);
                Nancy.Response response = Response.AsJson(new_user);
                response.StatusCode = HttpStatusCode.Created;

                string uri = this.Request.Url.SiteBase + this.Request.Path + "/" + user.Id;
                response.Headers["Location"] = uri;
                
                return response;
            } catch (Exception e) {
                Console.WriteLine(rawBody);
                String operation = String.Format("UserModule.AddItem({0})", (user == null) ? "No Model Data" : user.Username);
                return HandleException(e, operation);
            }
        }

        Nancy.Response UpdateItem(string id)
        {

            UserModel user = null;

            // capture actual string posted in case the bind fails (as it will if the JSON is bad)
            // need to do it now as the bind operation will remove the data
            String rawBody = this.GetRawBody();
            
            try {
                user = this.Bind<UserModel>();

                UserMapper usr_mpr = new UserMapper();
                user.Id = id;

                UserModel res = usr_mpr.GetById(id);

                if (res == null) {
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("A User with Id = {0} does not exist", id));
                }
                usr_mpr.update(user);

                //Get new user data from DB
                UserModel updated_user = usr_mpr.GetById(user.Id);

                Nancy.Response response = Response.AsJson(updated_user);
                response.StatusCode = HttpStatusCode.OK;

                string uri = this.Request.Url.SiteBase + this.Request.Path + "/" + user.Id;
                response.Headers["Location"] = uri;

                return response;

            } catch (Exception e) {
                String operation = String.Format("UserModule.UpdateBadge({0})", (user == null) ? "No Model Data" : user.Username);
                return HandleException(e, operation);
            }
        }

        Nancy.Response DeleteUser(string id)
        {
            try
            {
                UserMapper usr_mpr = new UserMapper();
                UserModel user = usr_mpr.GetById(id);

                if (user == null)
                {
                    return ErrorBuilder.ErrorResponse(this.Request.Url.ToString(), "GET", HttpStatusCode.NotFound, String.Format("An User with Id = {0} does not exist", id));
                }

                usr_mpr.delete(user);

                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                return HandleException(e, String.Format("\nUserModule.Delete({0})", id));
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