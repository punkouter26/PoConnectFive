using Microsoft.JSInterop;

namespace PoConnectFive.Client.Services
{
    public class SoundService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isMuted = false;
        private bool _soundsAvailable = true;

        public SoundService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        private async Task TryPlaySound(string soundName)
        {
            if (!_isMuted && _soundsAvailable)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("playSound", soundName);
                }
                catch (JSException)
                {
                    // If we get a JS exception, sounds might not be available
                    _soundsAvailable = false;
                }
            }
        }

        public async Task PlayPieceDrop()
        {
            await TryPlaySound("piece-drop");
        }

        public async Task PlayWin()
        {
            await TryPlaySound("win");
        }

        public async Task PlayDraw()
        {
            await TryPlaySound("draw");
        }

        public async Task PlayError()
        {
            await TryPlaySound("error");
        }

        public void ToggleMute()
        {
            _isMuted = !_isMuted;
        }

        public bool IsMuted => _isMuted;
    }
} 