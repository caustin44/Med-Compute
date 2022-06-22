using System;
using System.Collections.Generic;
using System.Reflection;
using Nancy;
using Nancy.Extensions;

namespace compute.geometry
{
    public class ConvertModel
    {
        public string b64str { get; set; }
        public string extension { get; set; }
    }

    public class FixedEndPointsModule : NancyModule
    {
        public FixedEndPointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Get[""] = _ => HomePage(Context);
            Get["/healthcheck"] = _ => "healthy";
            Get["version"] = _ => GetVersion(Context);
            Post["convert"] = _ => ConvertTo3DM(Context);
            Post["encode"] = _ => EncodeFile(Context);
            Get["servertime"] = _ => ServerTime(Context);
            Get["sdk/csharp"] = _ => CSharpSdk(Context);
        }
        static Response HomePage(NancyContext ctx)
        {
            return new Nancy.Responses.RedirectResponse("https://www.rhino3d.com/compute");
        }

        static Response ConvertTo3DM(NancyContext ctx)
        {
            using (var doc = Rhino.RhinoDoc.CreateHeadless(null))
            {
                // Decode the file and create an instance of it on the serverside
                var requestBody = ctx.Request.Body.AsString();
                ConvertModel jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ConvertModel>(requestBody);
                System.IO.File.WriteAllBytes("temp." + jsonObject.extension, Convert.FromBase64String(jsonObject.b64str));

                // Create a rhino document based on the file, and export it as a .3dm
                doc.Import("temp." + jsonObject.extension);
                doc.Write3dmFile("temp.3dm", new Rhino.FileIO.FileWriteOptions());

                // Write the document to an instance of Rhino's File3DM so that it can be returned as a byte array
                var rhinoFile = Rhino.FileIO.File3dm.Read("temp.3dm");
                var b64FileStr = Convert.ToBase64String(rhinoFile.ToByteArray());

                return (Nancy.Response)b64FileStr;
            }
        }

        static Response EncodeFile(NancyContext ctx)
        { 
            var requestBody = ctx.Request.Body.AsString();

            var rhinoFile = new Rhino.FileIO.File3dm();
            //rhinoFile.Objects.Add(Convert.FromBase64String(requestBody));

            var b64FileStr = Convert.ToBase64String(rhinoFile.ToByteArray());
            return (Nancy.Response)b64FileStr;
            
        }

        static Response GetVersion(NancyContext ctx)
        {
            var values = new Dictionary<string, string>();
            values.Add("rhino", Rhino.RhinoApp.Version.ToString());
            values.Add("compute", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            string git_sha = null;
            values.Add("git_sha", git_sha);
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(values);
            response.ContentType = "application/json";
            return response;
        }

        static Response ServerTime(NancyContext ctx)
        {
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(DateTime.UtcNow);
            response.ContentType = "application/json";
            return response;
        }

        static Response CSharpSdk(NancyContext ctx)
        {
            string content = "";
            using (var resourceStream = typeof(FixedEndPointsModule).Assembly.GetManifestResourceStream("compute.geometry.RhinoCompute.cs"))
            {
                var stream = new System.IO.StreamReader(resourceStream);
                content = stream.ReadToEnd();
                stream.Close();
            }

            var response = new Response();

            response.Headers.Add("Content-Disposition", "attachment; filename=RhinoCompute.cs");
            response.ContentType = "text/plain";
            response.Contents = stream => {
                using (var writer = new System.IO.StreamWriter(stream))
                {
                    writer.Write(content);
                }
            };
            return response.AsAttachment("RhinoCompute.cs", "text/plain");
        }
    }
}
