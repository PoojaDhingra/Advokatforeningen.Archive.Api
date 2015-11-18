//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Advokatforeningen.Archive.Api.Repositories.HelperClasses
{
    public class CompressFilter : ActionFilterAttribute
    {
        //public override void OnActionExecuting(FilterExecutingContext filterContext)
        //public override void OnActionExecuting(HttpActionContext context)
        //{
        //    //HttpRequestBase request = filterContext.HttpContext.Request;

        //    //string acceptEncoding = request.Headers["Accept-Encoding"];
        //    string acceptEncoding = context.Request.Headers.AcceptEncoding.ToString();

        //    if (string.IsNullOrEmpty(acceptEncoding)) return;

        //    acceptEncoding = acceptEncoding.ToUpperInvariant();

        //    //HttpResponseBase response = filterContext.HttpContext.Response;

        //    var response = context.Response;
        //    if (acceptEncoding.Contains("GZIP"))
        //    {
        //        response.Headers.Add("Content-encoding", "gzip");
        //        response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
        //    }
        //    else if (acceptEncoding.Contains("DEFLATE"))
        //    {
        //        response.AppendHeader("Content-encoding", "deflate");
        //        response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
        //    }
        //}

        public override void OnActionExecuted(HttpActionExecutedContext actContext)
        {
            //string acceptEncoding = actContext.Request.Headers.AcceptEncoding.ToString();
            //var response = actContext.Response;

            ////HttpResponseBase response = actContext. .HttpContext.Response;

            //if (acceptEncoding.ToLower().Contains("GZIP".ToLower()))
            //{
            //    //response.AppendHeader("Content-encoding", "gzip");
            //    response.Headers.Add("Content-encoding", "gzip");
            //    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            //}
            //else if (acceptEncoding.ToLower().Contains("DEFLATE".ToLower()))
            //{
            //    response.AppendHeader("Content-encoding", "deflate");
            //    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
            //}
        }

        //public override void OnActionExecuted(HttpActionExecutedContext actContext)
        //{
        //    var content = actContext.Response.Content;

        //    string contentencoding = ApiHeaderValue.AppContentEncoding.ToString();

        //    if (contentencoding == "GZip")

        //    {
        //        var bytes = (content == null) ? null : content.ReadAsByteArrayAsync().Result;

        //        var zlibbedContent = bytes == null ? new byte[0] :

        //        CompressionHelper.DeflateByte(bytes);

        //        actContext.Response.Content = new ByteArrayContent(zlibbedContent);

        //        actContext.Response.Content.Headers.Remove("Content-Type");

        //        actContext.Response.Content.Headers.Add("Content-encoding", "GZip");

        //        actContext.Response.Content.Headers.Add("Content-Type", "application/json");

        //        base.OnActionExecuted(actContext);
        //    }
        //}
    }

    public class CompressionHelper
    {
        //public static byte[] DeflateByte(byte[] str)
        //{
        //    if (str == null)

        //    {
        //        return null;
        //    }

        //    using (var output = new MemoryStream())

        //    {
        //        using (

        //        var compressor = new Ionic.Zlib.GZipStream(

        //        output, Ionic.Zlib.CompressionMode.Compress,

        //        Ionic.Zlib.CompressionLevel.BestSpeed))

        //        {
        //            compressor.Write(str, 0, str.Length);
        //        }

        //        return output.ToArray();
        //    }
        //}
    }
}