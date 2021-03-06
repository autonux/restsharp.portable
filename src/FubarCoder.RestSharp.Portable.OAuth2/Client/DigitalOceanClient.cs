﻿using System;
using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;
using System.Threading.Tasks;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// OAuth2 client for Digital Ocean
    /// </summary>
    public class DigitalOceanClient : OAuth2Client
    {
        private string _accessUserInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="configuration"></param>
        public DigitalOceanClient(IRequestFactory factory, IClientConfiguration configuration) 
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "DigitalOcean"; }
        }

        /// <summary>
        /// Defines URI of service which issues access code.
        /// </summary>
        protected override Endpoint AccessCodeServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://cloud.digitalocean.com",
                    Resource = "/v1/oauth/authorize"
                };
            }
        }

        /// <summary>
        /// Called just after obtaining response with access token from service.
        /// Allows to read extra data returned along with access token.
        /// </summary>
        protected override void AfterGetAccessToken(BeforeAfterRequestArgs args)
        {
             _accessUserInfo = args.Response.Content;
        }

        /// <summary>
        /// Defines URI of service which issues access token.
        /// </summary>
        protected override Endpoint AccessTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://cloud.digitalocean.com",
                    Resource = "/v1/oauth/token"
                };
            }
        }

        /// <summary>
        /// Defines URI of service which allows to obtain information about user 
        /// who is currently logged in.
        /// </summary>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Obtains user information using provider API.
        /// </summary>
        protected override Task<UserInfo> GetUserInfo()
        {
            var result = ParseUserInfo(_accessUserInfo);

#if USE_TASKEX
            return TaskEx.FromResult(result);
#else
            return Task.FromResult(result);
#endif
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> using content received from provider.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> using content received from provider.
        /// </summary>
        /// <param name="content">The response which is received from the provider.</param>
        protected virtual UserInfo ParseUserInfo(string content)
        {
            var info = JObject.Parse(content);
            return new UserInfo
            {
                Id = info["uid"].Value<string>(),
                FirstName = info["info"]["name"].Value<string>(),
                LastName = "",
                Email = info["info"]["email"].SafeGet(x => x.Value<string>())
            };
        }
    }
}
