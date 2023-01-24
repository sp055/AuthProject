using System.Linq;
using AuthProject.Data;
using AuthProject.Models;
using AuthProject.ViewModels;
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
            LogInViewModel? userVM;
            string? userName;
            var userContext = context.HttpContext.User.Identity.Name;
            if (method != "GET")
            {
                if (!string.IsNullOrEmpty(context.HttpContext.Request.QueryString.Value))
                {
                    data = context.HttpContext.Request.QueryString.Value;
                }
                else
                {
                    var arguments = context.ActionArguments;

                    var value = arguments.FirstOrDefault().Value;

                    if (value != null)
                    {
                        var convertedValue = JsonConvert.SerializeObject(value);
                        data = convertedValue;
                        if (value is LogInViewModel)
                        {
                            userVM = (LogInViewModel)value;
                            userName = userVM.UserName;
                            data = $@"[{{'UserName': {userName} }}]";
                            var userDB = _context.AppUser.FirstOrDefault(x => x.UserName == userName);
                            if (userDB != null) userContext = userDB.UserName;
                            userContext = userName;
                        }
                    }
                }



                if (userContext != null)
                {
                    var ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

                    SaveUserActivity(data, url, userContext, method, ipAddress);
                }
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