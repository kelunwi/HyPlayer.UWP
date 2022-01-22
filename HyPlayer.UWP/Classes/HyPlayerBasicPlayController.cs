using System.Threading.Tasks;
using HyPlayer.Uta;
using HyPlayer.Uta.PlayController;

namespace HyPlayer.UWP.Classes;

internal class HyPlayerBasicPlayController : PlayControllerBase
{
    private PlayCore _playCore;
    public override Task InitializeController(PlayCore playCore)
    {
        _playCore = playCore;
        // 监听事件
        playCore.EventListener.OnEnded += SongEndedMoveNext;
        return Task.CompletedTask;
    }

    private Task SongEndedMoveNext(AudioTicketOperationEventArgs args)
    {
        // 释放已经播放完成的音频
        _playCore.AudioService.DisposeAudioTicketAsync(args.AudioTicket);
        _playCore.MoveNext();
        return Task.CompletedTask;
    }


    public override Task DisposeController(PlayCore playCore)
    {
        playCore.EventListener.OnEnded -= SongEndedMoveNext;
        return Task.CompletedTask;
    }

    public HyPlayerBasicPlayController()
    {
        Id = "HyPlayerBasicPlayController";
    }
}

