using Nancy;

namespace Ingress.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => View["index"];

            Get["/admin/items"] = _ => View["admin-items"];

            Get["/admin/notifications"] = _ => View["admin-notifications"];
        }
    }
}