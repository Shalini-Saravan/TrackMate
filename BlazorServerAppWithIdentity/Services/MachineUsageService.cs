﻿using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;

namespace BlazorServerAppWithIdentity.Services
{
    public class MachineUsageService
    {
        private readonly HttpClient httpClient;
        public ILocalStorageService LocalStorageService { get; set; }
        public MachineUsageService(ILocalStorageService LocalStorageService, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.LocalStorageService = LocalStorageService;
        }
        public IEnumerable<MachineUsage> GetMachineUsageByUserName(string userName)
        { 
            IEnumerable<MachineUsage> machineUsages = new List<MachineUsage>();
            return httpClient?.GetFromJsonAsync<IEnumerable<MachineUsage>>("api/machineusage/" + userName).Result ?? machineUsages;
        }

    }
}
