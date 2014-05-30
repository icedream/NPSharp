using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using log4net;
using NPSharp.NP;
using uhttpsharp;
using uhttpsharp.Headers;
using HttpResponse = uhttpsharp.HttpResponse;

namespace NPSharp.CommandLine.File
{
    internal class NP2HTTPPublisherFileHandler : IHttpRequestHandler
    {
        private readonly ILog _log;
        private readonly NPClient _np;

        public NP2HTTPPublisherFileHandler(NPClient np)
        {
            _np = np;
            _log = LogManager.GetLogger(GetType());
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            var uri = context.Request.QueryString.Any()
                ? null
                : string.Join("/", context.Request.Uri.OriginalString.Split('/').Skip(2));
            if (uri == null)
                if (!context.Request.QueryString.TryGetByName("uri", out uri) || uri == null)
                {
                    context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "Invalid request",
                        context.Request.Headers.KeepAliveConnection(),
                        "You need to provide a <code>uri</code> parameter with the URL."
                        );
                    return Task.Factory.GetCompleted();
                }

            _log.InfoFormat("Requesting publisher file {0}", uri);
            var task = _np.GetPublisherFile(uri);
            try
            {
                task.Wait();
            }
            catch
            {
                context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "File not accessible",
                    context.Request.Headers.KeepAliveConnection(),
                    string.Format("<pre><tt><code>{0}</code></tt></pre>",
                        task.Exception == null ? "Unknown error" : task.Exception.ToString())
                    );
                return Task.Factory.GetCompleted();
            }

            // Return file contents
            context.Response = new HttpResponse(HttpResponseCode.Ok, MimeMapping.GetMimeMapping(uri),
                new MemoryStream(task.Result), context.Request.Headers.KeepAliveConnection());

            return Task.Factory.GetCompleted();
        }
    }
}