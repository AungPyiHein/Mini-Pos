using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace POS.Frontend.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task ToggleThemeAsync()
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.toggleTheme");
        }

        public async Task SetThemeModesAsync(string lightSidebar, string lightContent, string darkSidebar, string darkContent)
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.setThemeModes", lightSidebar, lightContent, darkSidebar, darkContent);
        }

        public async Task<bool> IsDarkModeAsync()
        {
            var script = "document.documentElement.classList.contains('dark')";
            return await _jsRuntime.InvokeAsync<bool>("eval", script);
        }

        public async Task SetAccentColorAsync(string hexColor)
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.setAccentColor", hexColor);
        }

        public async Task<string> GetAccentColorAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("themeManager.getAccentColor");
        }

        public async Task SetSidebarColorAsync(string hexColor)
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.setSidebarColor", hexColor);
        }

        public async Task<string> GetSidebarColorAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("themeManager.getSidebarColor");
        }

        public async Task SetContentColorAsync(string hexColor)
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.setContentColor", hexColor);
        }

        public async Task<string> GetContentColorAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("themeManager.getContentColor");
        }
    }
}
