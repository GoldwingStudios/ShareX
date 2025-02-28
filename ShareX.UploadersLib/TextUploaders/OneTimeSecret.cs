﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright © 2007-2015 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Newtonsoft.Json;
using ShareX.HelpersLib;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ShareX.UploadersLib.TextUploaders
{
    public sealed class OneTimeSecret : TextUploader
    {
        private const string API_ENDPOINT = "https://onetimesecret.com/api/v1/share";

        public string API_KEY { get; set; }
        public string API_USERNAME { get; set; }

        public override UploadResult UploadText(string text, string fileName)
        {
            UploadResult result = new UploadResult();

            if (!string.IsNullOrEmpty(text))
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("secret", text);

                result.Response = SendRequest(HttpMethod.POST, API_ENDPOINT, args, CreateAuthenticationHeader(API_USERNAME, API_KEY));

                if (!string.IsNullOrEmpty(result.Response))
                {
                    OneTimeSecretResponse jsonResponse = JsonConvert.DeserializeObject<OneTimeSecretResponse>(result.Response);

                    if (jsonResponse != null)
                    {
                        result.URL = URLHelpers.CombineURL("https://onetimesecret.com/secret/", jsonResponse.secret_key);
                    }
                }
            }

            return result;
        }

        public class OneTimeSecretResponse
        {
            public string custid { get; set; }
            public string metadata_key { get; set; }
            public string secret_key { get; set; }
            public string ttl { get; set; }
            public string updated { get; set; }
            public string created { get; set; }
        }
    }
}