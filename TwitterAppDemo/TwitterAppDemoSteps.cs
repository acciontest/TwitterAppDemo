using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using AlteryxGalleryAPIWrapper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;
using System.Collections.Generic;

namespace TwitterAppDemo
{
    [Binding]
    public class TwitterAppDemoSteps
    {

        private string alteryxurl;
        private string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;
        private string _appActualName;
        private dynamic statusresp ;
        private int messagecount;
        

        //   private Client Obj = new Client("https://devgallery.alteryx.com/api/");
        private Client Obj = new Client("https://gallery.alteryx.com/api/");

        private RootObject jsString = new RootObject();

        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string SUT_url)
        {
            alteryxurl = Environment.GetEnvironmentVariable(SUT_url);
        }
        
        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            _sessionid = Obj.Authenticate(user, password).sessionId;
        }
        
        //[Given(@"I publish the application ""(.*)""")]
        //public void GivenIPublishTheApplication(string p0)
        //{
        //    //Publish the app & get the ID of the app
        //    string apppath = @"..\..\docs\Mortgage_Calculator.yxzp";
        //    Action<long> progress = new Action<long>(Console.Write);
        //    var pubResult = Obj.SendAppAndGetId(apppath, progress);
        //    ScenarioContext.Current.Set(Obj, System.Guid.NewGuid().ToString());
        //    _appid = pubResult.id;
        //    validationId = pubResult.validation.validationId;
        //}
        
        //[Given(@"I check if the application is ""(.*)""")]
        //public void GivenICheckIfTheApplicationIs(string p0)
        //{
        //    ScenarioContext.Current.Pending();
        //}
        
        //[When(@"I run Twitter Tracker App and search the tweets using the search phrases (.*)")]
        //public void WhenIRunTwitterTrackerAppAndSearchTheTweetsUsingTheSearchPhrases(string p0)
        //{
        //    ScenarioContext.Current.Pending();
        //}

        [When(@"I run the App ""(.*)"" with the """"(.*)"""" use a twitter handle to search ""(.*)""")]
        public void WhenIRunTheAppWithTheUseATwitterHandleToSearch(string app, string description, string searchterm)
        {
       
            //url + "/apps/gallery/?search=" + appName + "&limit=20&offset=0"
            //Search for App & Get AppId & userId 
            string response = Obj.SearchAppsGallery(app);
            var appresponse =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    response);
            int count = appresponse["recordCount"];
            if (count == 1)
            {
                _appid = appresponse["records"][0]["id"];
                _userid = appresponse["records"][0]["owner"]["id"];
                _appName = appresponse["records"][0]["primaryApplication"]["fileName"];                      
            }
            else
            {
                for (int i = 0; i <= count - 1; i++)
                {
                    
                    _appActualName = appresponse["records"][i]["primaryApplication"]["metaInfo"]["name"];
                    if (_appActualName == app)
                    {
                        _appid = appresponse["records"][i]["id"];
                        _userid = appresponse["records"][i]["owner"]["id"];
                        _appName = appresponse["records"][i]["primaryApplication"]["fileName"];
                        break;
                    }
                }    
                
            }    
            jsString.appPackage.id = _appid;           
            jsString.userId = _userid;
            jsString.appName = _appName;

            //url +"/apps/" + appPackageId + "/interface/
            //Get the app interface - not required
            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);

            //Construct the payload to be posted.
            string header = String.Empty;
          //  string payatbegin = String.Empty;
            string city = "";
            string Address = "";
            string state = "";
            string zipcode = "";
            string radius = "20";
            string username = "";
            string hashtag = "1";
            string search = searchterm;
            List<JsonPayload.Question> questionAnsls = new List<JsonPayload.Question>();
           // questionAnsls.Add(new JsonPayload.Question("Keyword", "\"pitchinvasion\""));
            questionAnsls.Add(new JsonPayload.Question("Keyword", "\"" + searchterm + "\""));
     
            questionAnsls.Add(new JsonPayload.Question("Search by location", "false"));
            questionAnsls.Add(new JsonPayload.Question("Address", Address));
            questionAnsls.Add(new JsonPayload.Question("City", city));
            questionAnsls.Add(new JsonPayload.Question("State", state));
            questionAnsls.Add(new JsonPayload.Question("Zip Code", zipcode));
            questionAnsls.Add(new JsonPayload.Question("Radius", radius));
            questionAnsls.Add(new JsonPayload.Question("username", username));
            questionAnsls.Add(new JsonPayload.Question("Hash Tag Count", hashtag));


            var sortf = new List<JsonPayload.datac>();
            sortf.Add(new JsonPayload.datac() { key = "Date", value = "true" });
            var sorto = new List<JsonPayload.datac>();
            sorto.Add(new JsonPayload.datac() { key = "Descending", value = "true" });
            string SortField = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(sortf);
            string SortOrder= new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(sorto);


            for (int i = 0; i < 3; i++)
            {

                if (i == 0)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "Sort Field";
                    questionAns.answer = SortField;
                    jsString.questions.Add(questionAns);
                }
                else if (i == 1)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "Sort Order";
                    questionAns.answer = SortOrder;
                    jsString.questions.Add(questionAns);
                }
                else
                {
                    jsString.questions.AddRange(questionAnsls);
                }
            }
            jsString.jobName = "Job Name";


            // Make Call to run app

            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);

            var jobqueue =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    resjobqueue);
            jobid = jobqueue["id"];

            //Get the job status

            string status = "";
            while (status != "Completed")
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                 statusresp =
                    new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                        jobstatusresp);
                status = statusresp["status"];
                messagecount = statusresp["messages"].Count;                
            }           
        }

        
        [Then(@"I see the searched phase (.*)has the count (.*)")]
        public void ThenISeeTheSearchedPhaseHasTheCount(string searchterm, int tweetcount)
        {
            //check the output message to check the number of values parsed.
            for (int i = 1; i < messagecount - 1; i++)
            {
                int toolid = statusresp["messages"][i]["toolId"];
                if (toolid == 78)
                {
                    string text = statusresp["messages"][i]["text"];
                    StringAssert.Contains(tweetcount.ToString(),text);
                    break;
                }
            }
            ////url + "/apps/jobs/" + jobId + "/output/"
            //string getmetadata = Obj.GetOutputMetadata(jobid);
            //dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);

            //// outputid = metadataresp[0]["id"];
            //int count = metadataresp.Count;
            //for (int j = 0; j <= count - 1; j++)
            //{
            //    outputid = metadataresp[j]["id"];
            //}

            //string getjoboutput = Obj.GetJobOutput(jobid, outputid, "raw");
            //string htmlresponse = getjoboutput;
        }
    }
}
