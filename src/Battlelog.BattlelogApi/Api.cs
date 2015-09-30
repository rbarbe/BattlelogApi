using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Battlelog.BattlelogApi.Models;

namespace Battlelog.BattlelogApi
{
    public class Api
    {
        private readonly Uri _battelogUri;
        private readonly HttpClient _httpClient;

        public Api()
        {
            _httpClient = new HttpClient();
            _battelogUri = new Uri("http://battlelog.battlefield.com");
        }

        public async Task<List<CommunityMission>> GetCommunityMissions(BattlelogGame game)
        {
            var result = new List<CommunityMission>();

            var uri = new Uri(_battelogUri,
                new Uri($"/{game.ToString().ToLower()}/communitymissions/", UriKind.Relative));
            var response = await _httpClient.GetAsync(uri);
            var source = await response.Content.ReadAsStringAsync();

            var htmlParser = new HtmlParser();
            var document = await htmlParser.ParseAsync(source);
            var section = document.QuerySelector("#community-mission");

            var rows = section.QuerySelectorAll("div.row");

            if (rows != null)
                foreach (var row in rows)
                {
                    var communityMission = new CommunityMission();
                    var info = row.QuerySelector("div.info");

                    communityMission.Name = info.QuerySelector(".community-mission-info-title").TextContent;
                    communityMission.Description = info.QuerySelector(".description").TextContent;

                    communityMission.Reward = row.QuerySelector(".reward").QuerySelector(".description").TextContent;

                    var timePeriod = row.QuerySelector(".timeperiod").TextContent.Replace(" UTC", "Z");
                    var datetimes = timePeriod.Split(new[] {" - "}, StringSplitOptions.None);

                    communityMission.Start = DateTimeOffset.Parse(datetimes[0]);
                    communityMission.End = DateTimeOffset.Parse(datetimes[1]);

                    communityMission.Game = game;
                    result.Add(communityMission);
                }
            return result;
        }

        private async Task<GetLoginPageResult> GetLoginPage()
        {
            var result = new GetLoginPageResult();

            var queryParameter = new[]
            {
                new KeyValuePair<string, string>("locale", "en_US"),
                new KeyValuePair<string, string>("state", "bfh"),
                new KeyValuePair<string, string>("redirect_uri",
                    "https://battlelog.battlefield.com/sso/?tokentype=code"),
                new KeyValuePair<string, string>("response_type", "code"),
                new KeyValuePair<string, string>("client_id", "battlelog"),
                new KeyValuePair<string, string>("display", "web/login")
            };

            HttpContent content = new FormUrlEncodedContent(queryParameter);


            var query = await content.ReadAsStringAsync();

            var uri = new Uri("https://accounts.ea.com/connect/auth?" + query, UriKind.Absolute);


            var response = await _httpClient.GetAsync(uri);
            var source = await response.Content.ReadAsStringAsync();
            var parameters = response.RequestMessage.RequestUri.Query.Remove(0, 1).Split('&');

            foreach (var parameter in parameters)
            {
                var parameterSplitted = parameter.Split('=');
                var key = parameterSplitted[0];
                var value = WebUtility.UrlDecode(parameterSplitted[1]);

                if (key == "execution")
                {
                    result.Execution = value;
                }
                else if (key == "initref")
                {
                    result.Initref = value;
                }
            }

            // If HttpUtility is available in .Net 5
            //var queryDuringRedirect = response.RequestMessage.RequestUri.ParseQueryString();
            //result.Execution = queryDuringRedirect["execution"];
            //result.Initref = queryDuringRedirect["initref"];

            return result;
        }

        private async Task<bool> Login(GetLoginPageResult getLoginPageResult, string email, string password)
        {
            var queryList = new[]
            {
                new KeyValuePair<string, string>("execution", getLoginPageResult.Execution),
                new KeyValuePair<string, string>("initref", getLoginPageResult.Initref)
            };

            var bodyList = new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("_rememberMe",
                    "on"),
                new KeyValuePair<string, string>("rememberMe", "on"),
                new KeyValuePair<string, string>("_eventId", "submit"),
                new KeyValuePair<string, string>("gCaptchaResponse", "")
            };

            HttpContent queryStringContent = new FormUrlEncodedContent(queryList);
            HttpContent bodyContent = new FormUrlEncodedContent(bodyList);

            var queryString = await queryStringContent.ReadAsStringAsync();

            var uri = new Uri("https://signin.ea.com/p/web/login?" + queryString, UriKind.Absolute);

            var response = await _httpClient.PostAsync(uri, bodyContent);
            var source = await response.Content.ReadAsStringAsync();

            if (response.RequestMessage.RequestUri == uri)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> SignIn(string email, string password)
        {
            var loginPageResult = await GetLoginPage();
            return await Login(loginPageResult, email, password);
        }
    }
}