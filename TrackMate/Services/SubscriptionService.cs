﻿using Blazored.LocalStorage;
using TrackMate.Pages;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using TrackMate.Models;

namespace TrackMate.Services
{
    public class SubscriptionService
    {
        private readonly HttpClient httpClient;
        protected string message { get; set; }

        private readonly ILocalStorageService LocalStorageService;
        public SubscriptionService(ILocalStorageService LocalStorageService, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.LocalStorageService = LocalStorageService;
        }
        public string UpdateSubscription(Subscription subscription, string? userName)
        {
            if (userName != null && userName != string.Empty)
            {
                var jsonString = "{'subscription' :" + JsonConvert.SerializeObject(subscription) + ", 'userName' : '" + userName + "'}";
                var response = httpClient.PostAsJsonAsync("api/Subscription", jsonString).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return "Failed Operation!";
            }
        }
        public Subscription GetSubscriptionByUserName(string userName)
        {
            if (userName != null && userName != string.Empty)
            {
                return httpClient.GetFromJsonAsync<Subscription>("api/subscription/" + userName).Result ?? new Subscription();
            }
            else
            {
                return new Subscription();
            }
        }

    }
}
