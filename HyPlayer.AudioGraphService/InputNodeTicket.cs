using Windows.Media.Audio;
using HyPlayer.Uta.AudioService;

namespace HyPlayer.AudioGraphService
{
    public class InputNodeTicket : AudioTicketBase
    {
        public IAudioInputNode Node { get; private set; }
        
        public InputNodeTicket(IAudioInputNode node)
        {
            Node = node;
        }
    }
}