using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Advokatforeningen.Archive.Api.Repositories.HelperClasses
{
    public class EnableETag : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, EntityTagHeaderValue> ETags = new
ConcurrentDictionary<string, EntityTagHeaderValue>();

        public override void OnActionExecuting(HttpActionContext context)
        {
            var request = context.Request;
            //if (request.Method == HttpMethod.Get)
            if (Equals(request.Method, HttpMethod.Get))
            {
                var key = GetKey(request);
                var aaaa = request.Headers;
                ICollection<EntityTagHeaderValue> eTagsFromClient = request.Headers.IfNoneMatch;
                if (eTagsFromClient.Count > 0)
                {
                    EntityTagHeaderValue eTag;
                    if (ETags.TryGetValue(key, out eTag) && eTagsFromClient.Any(t => t.Tag == eTag.Tag))
                    {
                        context.Response = new HttpResponseMessage(HttpStatusCode.NotModified);
                        //SetCacheControl(context.Response);
                    }
                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actContext)
        {
            var request = actContext.Request;
            var key = GetKey(request);
            EntityTagHeaderValue eTag = null;

            bool isGet = (request.Method == HttpMethod.Get),
                isPutOrPost = (request.Method == HttpMethod.Put || request.Method == HttpMethod.Post);

            //if (!ETags.TryGetValue(key, out eTag) || request.Method == HttpMethod.Put || request.Method == HttpMethod.Post)
            if ((isGet && !ETags.TryGetValue(key, out eTag)) || isPutOrPost)
            {
                //eTag = new EntityTagHeaderValue("\"" + Guid.NewGuid().ToString() + "\"");
                eTag = new EntityTagHeaderValue(actContext.Response.Headers.ETag.ToString());
                ETags.AddOrUpdate(key, eTag, (k, val) => eTag);
            }

            if (isGet)
            {
                actContext.Response.Headers.ETag = eTag;
            }
            //SetCacheControl(context.Response);
        }

        private string GetKey(HttpRequestMessage request)
        {
            return request.RequestUri.ToString();
        }

        private void SetCacheControl(HttpResponseMessage response)
        {
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromSeconds(6),
                MustRevalidate = true,
                Private = true
            };
        }
    }
}