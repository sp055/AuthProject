using System.Linq;
using AuthProject.Data;
using AuthProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace AuthProject.Filters
{
    public class UserFilterAttribute : IActionFilter
    {
        private readonly ApplicationDbContext _context;


        public UserFilterAttribute(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string data = "";

            var routeData = context.RouteData;
            var controller = routeData.Values["controller"];
            var action = routeData.Values["action"];

            var url = $"{controller}/{action}";

            var method = context.HttpContext.Request.Method;
            var methodType = context.ActionDescriptor.DisplayName;

            if (!string.IsNullOrEmpty(context.HttpContext.Request.QueryString.Value))
            {
                data = context.HttpContext.Request.QueryString.Value;
            }
            else
            {
                var arguments = context.ActionArguments;

                var value = arguments.FirstOrDefault().Value;



                var convertedValue = JsonConvert.SerializeObject(value);
                data = convertedValue;
            }
            
            var user = context.HttpContext.User.Identity.Name;

            if (user != null)
            {
                var ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

                SaveUserActivity(data, url, user, method, ipAddress);
            }
            
        }

        public void SaveUserActivity(string data, string url, string user, string method, string ipAddress)
        {
            var userActivity = new UserActivity
            {
                Data = data,
                Url = url,
                UserName = user,
                IpAddress = ipAddress,
                MethodType = method,
            };

            _context.UserActivities.Add(userActivity);
            _context.SaveChanges();
        }
    }
}