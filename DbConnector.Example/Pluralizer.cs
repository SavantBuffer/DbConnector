using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace TMN.DAL
{
    public class MyDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection services)
        {
            services.AddSingleton<IPluralizer, MyPluralizer>();
        }
    }

    public class MyPluralizer : IPluralizer
    {
        public string Pluralize(string name)
        {
            return name;
        }

        public string Singularize(string name)
        {
            if (name.ToUpper().EndsWith("IES"))
            {
               return name.Substring(0, name.Length - 3) + "y";
            }
            else if(name.ToUpper().EndsWith("S"))
            {
                return name.Substring(0, name.Length - 1);
            }
            else
            {
                return name;
            }
        }
    }
}
