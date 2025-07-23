using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DBPediaNetwork.Helpers.Communication
{
    public static class Extensions
    {
        public static string buildQueryParams(this List<HttpParams> lstParams)
        {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (lstParams != null && (lstParams?.Count() ?? 0) > 0)
            {
                foreach (var item in lstParams)
                {
                    if (!String.IsNullOrEmpty(item.key))
                    {
                        queryString.Add(item.key, item.value);
                    }
                }
            }

            return  !String.IsNullOrEmpty(queryString.ToString())?  String.Concat("?", queryString.ToString()) : string.Empty;
        }
    }
}