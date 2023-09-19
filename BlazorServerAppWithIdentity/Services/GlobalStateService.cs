using BlazorServerAppWithIdentity.Models;

namespace BlazorServerAppWithIdentity.Services
{
    public class GlobalStateService : IGlobalStateService
    {
        public string TokenValue { get; set; }

    }

}
