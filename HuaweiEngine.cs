using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using APIGATEWAY_SDK;

namespace APITest
{

    public class HuaweiEngine
    {
        Signer signer = null;
        string Scene = "common";
        string ProjectID = "0cbfb339b080f40c2fe7c01fd250fb19";
        string Region = "cn-north-4";

        public string URL { get; set; }

        public HuaweiEngine()
        {
        }

        public HuaweiEngine(string apikey, string apisecret)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            signer = new Signer
            {
                //Set the AK/SK to sign and authenticate the request.
                Key = apikey,
                Secret = apisecret
            };

            //POST https://{endpoint}/v1/{project_id}/machine-translation/text-translation
            //https://nlp-ext.cn-north-4.myhuaweicloud.com/v1/0cbfb339b080f40c2fe7c01fd250fb19/machine-translation/text-translation
            URL = "https://nlp-ext."+Region+".myhuaweicloud.com/v1/" + ProjectID + "/machine-translation/text-translation";

        }

        ////zh,en,ja,ru,ko,fr,es,de,ar
        public string translateText(string text, string sourceLanguage, string targetLanguage)
        {
            /*
            * Request Body:
           {
               "text": "欢迎使用机器翻译服务",
               "from": "zh",
               "to": "en",
               "scene":"common"
           }  
           Response
           {
               "src_text": "欢迎使用机器翻译服务",
               "translated_text": "Welcome to use machine translation services",
               "from": "zh",
               "to": "en"
           }          */
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("text", text);
            keyValues.Add("from", sourceLanguage);
            keyValues.Add("to", targetLanguage);
            keyValues.Add("scene", Scene);
            string requestJson = JsonConvert.SerializeObject(keyValues);

            //sign and send request
            string translated = sendRequest(requestJson);

            //parse json
            dynamic TempResult = JsonConvert.DeserializeObject(translated);
            string resultText = Convert.ToString(TempResult["translated_text"]);

            return resultText;
        }

        protected static string ComputeHash256_base64(Byte[] inputBytes, HashAlgorithm algorithm)
        {
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashedBytes);
        }

        private string sendRequest(string requestBody)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);

            //sign
            HttpRequest r = new HttpRequest("POST", new Uri(URL));
            r.headers.Add("Content-Type", "application/json");
            r.headers.Add("X-Project-Id", ProjectID);
            r.body = requestBody;

            HttpWebRequest request = signer.Sign(r);

            Console.WriteLine(request.Headers.GetValues("x-sdk-date")[0]);
            Console.WriteLine(string.Join(", ", request.Headers.GetValues("authorization")));
           
            //send
            int respondCode = 0;
            string respondStr = string.Empty;

            try
            {
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                {
                    respondCode = (int)webResponse.StatusCode;
                    if (respondCode == 200)
                    {
                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                        {
                            respondStr = sr.ReadToEnd();
                        }

                        //sw.WriteLine("result");                     

                    }

                }

                return respondStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        
    }

}
