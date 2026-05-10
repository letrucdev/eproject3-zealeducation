namespace ZealEducation.API.Middleware;

public class ProjectInfoHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Project"] = "EProject 3 - Zeal Education";
            headers["X-Author"] = "Le Chinh Truc - Student1557161 (C2403L0751)";
            headers["X-Email"] = "letruc.work@gmail.com, truc.lc.2427@aptechlearning.edu.vn";
            headers["X-Created"] = "2026-19-04";
            headers["X-Course"] = "ADSE - Aptech Vietnam (https://aptechvietnam.com.vn)";
            headers["X-License"] = "All rights reserved. Unauthorized use prohibited.";
            return Task.CompletedTask;
        });

        return next(context);
    }
}
