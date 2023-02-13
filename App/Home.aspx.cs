using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocuSign.Models;
using System.IO;
using Newtonsoft.Json;
using DocuSign.Services;


namespace App
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private DocuSignConfiguration _config;
        protected void Page_Load(object sender, EventArgs e)
        {
            var path = @"C:\WL_DATA\Code\Github\DocuSign_Integration\App\DocuSign\DocuSignParams.json";
            var json = File.ReadAllText(path);
            _config = JsonConvert.DeserializeObject<DocuSignConfiguration>(json);

        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DocuSignService _oService = new DocuSignService(_config);
                SignerInfo _info = new SignerInfo();
                _info.Id = "567";
                _info.Name = "Sujeet Singh";
                _info.Email = "m2sujeet@gmail.com";
                var file = @"C:\WL_DATA\Code\Github\DocuSign_Integration\App\DocuSign\test.pdf";
                Base64Document _doc = new Base64Document();
                _doc.Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(file));
                _doc.Name = "test.pdf";
                var _resp = _oService.CreateEnvelope(_info, _doc);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
    }
}