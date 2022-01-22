using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media.Audio;
using Windows.Media.Core;
using Windows.Media.Render;
using Windows.Storage;
using HyPlayer.Uta;
using HyPlayer.Uta.AudioService;
using HyPlayer.Uta.MusicProvider;

namespace HyPlayer.AudioGraphService;

public class AudioGraphService : AudioServiceBase,
    IVolumeChangeableService,
    IAudioTicketSeekableService,
    IPlaybackRateChangeableService
{
    private AudioGraph _audioGraph;
    private AudioDeviceOutputNode _deviceOutputNode;
    private AudioTicketBase _mainAudioTicket;
    private PlayCoreEvents _events;
    private readonly List<InputNodeTicket> _audioTickets = new();
    private Timer _timer = new Timer(500);

    public override async Task InitializeService(PlayCoreEvents events, AudioServiceStatus audioServiceStatus)
    {
        _events = events;
        Status = audioServiceStatus;
        // Create an AudioGraph with default settings
        var result = await AudioGraph.CreateAsync(new AudioGraphSettings(AudioRenderCategory.Media));
        if (result.Status != AudioGraphCreationStatus.Success)
        {
            await _events.RaiseFailedEvent(new FailedEventArgs(this, result.ExtendedError,
                FailedEventType.CreatingAudioService));
            return;
        }

        _audioGraph = result.Graph;
        // Create default device output nodes
        var deviceOutputNodeResult = await _audioGraph.CreateDeviceOutputNodeAsync();
        if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
        {
            await _events.RaiseFailedEvent(new FailedEventArgs(this, deviceOutputNodeResult.ExtendedError,
                FailedEventType.CreatingAudioService));
            return;
        }

        Status.PlayStatus = PlayStatus.Stopped;
        _deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
        _deviceOutputNode.OutgoingGain = (double)Status.Volume / 100;
        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
        _audioGraph.Start();
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        // 更新所有的音频节点的播放进度
        for (int i = 0; i < _audioTickets.Count; i++)
        {
            _audioTickets[i].Position = _audioTickets[i].Node switch
            {
                MediaSourceAudioInputNode msi => msi.Position,
                AudioFileInputNode afi => afi.Position,
                _ => _audioTickets[i].Position
            };
        }
        Status.Position = _mainAudioTicket?.Position ?? TimeSpan.Zero;
    }

    public override Task DisposeServiceAsync(PlayCoreEvents events)
    {
        _timer.Stop();
        _timer.Dispose();
        _audioGraph.Stop();
        _audioGraph.Dispose();
        _audioTickets.ForEach(t => t.Node.Dispose());
        _audioTickets.ForEach(t=>t.IsDisposed = true);
        _audioTickets.Clear();
        return Task.CompletedTask;
    }

    public AudioGraphService()
    {
        Id = "AudioGraphService";
    }
    
    public override async Task<AudioTicketBase> GetAudioTicketAsync(MusicMediaSource inputSource)
    {
        Status.Buffering = true;
        await _events.RaiseBufferingEvent(new SimpleEventArgs(this));
        InputNodeTicket ticket;
        switch (inputSource.RealSource)
        {
            case MediaSource mediaSource:
            {
                var result = await _audioGraph.CreateMediaSourceAudioInputNodeAsync(mediaSource);
                if (result.Status != MediaSourceAudioInputNodeCreationStatus.Success)
                {
                    await _events.RaiseFailedEvent(new FailedEventArgs(this, result.ExtendedError,
                        FailedEventType.LoadAudioTicket));
                    return null;
                }

                ticket = new InputNodeTicket(result.Node);
                result.Node.MediaSourceCompleted += (sender, _) =>
                    _events.RaiseEndedEvent(new AudioTicketOperationEventArgs(sender, ticket));
                result.Node.PlaybackSpeedFactor = Status.PlaySpeed;
                break;
            }
            case StorageFile sf:
            {
                var result = await _audioGraph.CreateFileInputNodeAsync(sf);
                if (result.Status != AudioFileNodeCreationStatus.Success)
                {
                    await _events.RaiseFailedEvent(new FailedEventArgs(this, result.ExtendedError,
                        FailedEventType.LoadAudioTicket));
                    return null;
                }

                ticket = new InputNodeTicket(result.FileInputNode);
                result.FileInputNode.FileCompleted += (sender, _) =>
                    _events.RaiseEndedEvent(new AudioTicketOperationEventArgs(sender, ticket));
                result.FileInputNode.PlaybackSpeedFactor = Status.PlaySpeed;
                break;
            }
            default:
                try
                {
                    var mediaSource = MediaSource.CreateFromUri(new Uri(inputSource.Url));
                    var result = await _audioGraph.CreateMediaSourceAudioInputNodeAsync(mediaSource);
                    if (result.Status != MediaSourceAudioInputNodeCreationStatus.Success)
                    {
                        await _events.RaiseFailedEvent(new FailedEventArgs(this, result.ExtendedError,
                            FailedEventType.LoadAudioTicket));
                        return null;
                    }

                    ticket = new InputNodeTicket(result.Node);
                    result.Node.MediaSourceCompleted += (sender, _) =>
                        _events.RaiseEndedEvent(new AudioTicketOperationEventArgs(sender, ticket));
                    result.Node.PlaybackSpeedFactor = Status.PlaySpeed;
                }
                catch (Exception e)
                {
                    await _events.RaiseFailedEvent(new FailedEventArgs(this,
                        e, FailedEventType.LoadAudioTicket));
                    return null;
                }
                break;
        }

        Status.Buffering = false;
        ticket.Node.Stop();
        ticket.Node.AddOutgoingConnection(_deviceOutputNode);
        _audioTickets.Add(ticket);
        await _events.RaiseBufferedEvent(new AudioTicketOperationEventArgs(this, ticket));
        return ticket;
    }

    public override Task<List<AudioTicketBase>> GetAudioTicketListAsync()
    {
        return Task.FromResult(_audioTickets.Cast<AudioTicketBase>().ToList());
    }

    public override Task PlayAudioTicketAsync(AudioTicketBase audioTicket)
    {
        if (audioTicket.IsDisposed) return Task.CompletedTask;
        if (audioTicket is not InputNodeTicket ticket) return Task.CompletedTask;
        Status.PlayStatus = PlayStatus.Playing;
        ticket.Node.Start();
        ticket.Status = PlayStatus.Playing;
        return Task.CompletedTask;
    }

    public override Task PauseAudioTicketAsync(AudioTicketBase audioTicket)
    {
        if (audioTicket.IsDisposed) return Task.CompletedTask;
        if (audioTicket is not InputNodeTicket ticket) return Task.CompletedTask;
        ticket.Node.Stop();
        ticket.Status = PlayStatus.Paused;
        // 如果全部都暂停了的话, 就暂停吧
        if (_audioTickets.All(t => t.Status != PlayStatus.Playing))
            Status.PlayStatus = PlayStatus.Paused;
        return Task.CompletedTask;
    }

    public override Task DisposeAudioTicketAsync(AudioTicketBase audioTicket)
    {
        if (audioTicket.IsDisposed) return Task.CompletedTask;
        if (audioTicket is not InputNodeTicket ticket) return Task.CompletedTask;
        ticket.Node.Dispose();
        ticket.Status = PlayStatus.Disposed;
        _audioTickets.Remove(ticket);
        /*
        if (_audioTickets.All(t => t.Status != PlayStatus.Playing))
            Status.PlayStatus = PlayStatus.Paused;
        */
        return Task.CompletedTask;
    }

    public override void SetMainAudioTicket(AudioTicketBase audioTicket)
    {
        _mainAudioTicket = audioTicket;
    }

    public Task ChangeVolume(int volume)
    {
        Status.Volume = volume;
        _deviceOutputNode.OutgoingGain = (double)volume / 100;
        return Task.CompletedTask;
    }

    public Task SeekAudioTicket(AudioTicketBase audioTicket, TimeSpan position)
    {
        if (audioTicket is not InputNodeTicket inputNodeTicket) return Task.CompletedTask;
        switch (inputNodeTicket.Node)
        {
            case MediaSourceAudioInputNode mai:
                mai.Seek(position);
                break;
            case AudioFileInputNode afi:
                afi.Seek(position);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputNodeTicket.Node));
        }

        return Task.CompletedTask;
    }

    public Task ChangePlaybackSpeed(double playbackSpeed)
    {
        _audioTickets.ForEach(t =>
        {
            switch (t.Node)
            {
                case MediaSourceAudioInputNode mai:
                    mai.PlaybackSpeedFactor = playbackSpeed;
                    break;
                case AudioFileInputNode afi:
                    afi.PlaybackSpeedFactor = playbackSpeed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t.Node));
            }
        });
        return Task.CompletedTask;
    }
}