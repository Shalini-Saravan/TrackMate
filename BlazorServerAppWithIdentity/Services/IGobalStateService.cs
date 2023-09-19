using BlazorServerAppWithIdentity.Models;

namespace BlazorServerAppWithIdentity.Services
{
    public interface IGlobalStateService
    {
        string TokenValue { get; set; }
    }

}
