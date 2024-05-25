using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using TransferProject.Models;

namespace TransferProject.ObjectResults;

    public class PageResult<T> : OkObjectResult
    {
        private readonly IPage<T> _page;

        public PageResult(IPage<T> page)
            : base(page.Items)
        {
            _page = page;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            SetHeaders(context);

            await base.ExecuteResultAsync(context);
        }

        private void SetHeaders(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("x-page", _page.ToString());
            context.HttpContext.Response.Headers.Add("x-page-index", _page.Index.ToString());
            context.HttpContext.Response.Headers.Add("x-page-size", _page.Size.ToString());
            context.HttpContext.Response.Headers.Add("x-total-records", _page.TotalCount.ToString());
            context.HttpContext.Response.Headers.Add("x-total-pages", _page.TotalPages.ToString());
            context.HttpContext.Response.Headers.Add("x-hasPreviousPage", _page.HasPreviousPage.ToString().ToLowerInvariant());
            context.HttpContext.Response.Headers.Add("x-hasNextPage", _page.HasNextPage.ToString().ToLowerInvariant());
            context.HttpContext.Response.Headers.Add("Link", GetLinkHeaderForPageResult(context.HttpContext.Request, _page));
        }

        private string GetLinkHeaderForPageResult<T>(HttpRequest request, IPage<T> page)
        {
            string requestUrl = request.GetDisplayUrl();
            string headerValue = String.Empty;

            if (page.HasNextPage)
            {
                string nextPageUrl = requestUrl.ToLowerInvariant().Replace($"page={page.Index}", $"page={page.Index + 1}");
                headerValue += $"<{nextPageUrl}>; rel=\"next\",";
            }

            string lastPageUrl = requestUrl.ToLowerInvariant().Replace($"page={page.Index}", $"page={page.TotalPages}");
            headerValue += $"<{lastPageUrl}>; rel=\"last\",";

            string firstPageUrl = requestUrl.ToLowerInvariant().Replace($"page={page.Index}", "page=1");
            headerValue += $"<{firstPageUrl}>; rel=\"first\",";

            if (page.HasPreviousPage)
            {
                string previousPageUrl = requestUrl.ToLowerInvariant().Replace($"page={page.Index}", $"page={page.Index - 1}");
                headerValue += $"<{previousPageUrl}>; rel=\"prev\",";
            }

            if (!string.IsNullOrEmpty(headerValue))
            {
                headerValue = headerValue.TrimEnd(',');
            }

            return headerValue;
        }
    }