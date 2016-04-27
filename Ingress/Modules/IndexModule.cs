using Nancy;

namespace Ingress.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => View["index"];

            Get["/admin"] = _ => View["admin"];
        }
    }
}