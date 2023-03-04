using IO.Agora.Rtc2;

namespace WoWonder.Activities.Call.Agora.Tools
{
    public class AgoraRtcVideoHandler : IRtcEngineEventHandler
    {
        private readonly AgoraVideoCallActivity Context;

        public AgoraRtcVideoHandler(AgoraVideoCallActivity activity)
        {
            Context = activity;
        }

        public override void OnConnectionLost()
        {
            base.OnConnectionLost();
            Context.OnConnectionLost();
        }

        public override void OnRemoteAudioStateChanged(int uid, int state, int reason, int elapsed)
        {
            base.OnRemoteAudioStateChanged(uid, state, reason, elapsed);
            Context.OnRemoteAudioStateChanged(uid, state, reason, elapsed);
        }

        public override void OnRemoteVideoStateChanged(int uid, int state, int reason, int elapsed)
        {
            base.OnRemoteVideoStateChanged(uid, state, reason, elapsed);
            Context.OnRemoteVideoStateChanged(uid, state, reason, elapsed);
        }

        public override void OnFirstLocalVideoFrame(Constants.VideoSourceType source, int width, int height, int elapsed)
        {
            base.OnFirstLocalVideoFrame(source, width, height, elapsed);
            Context.OnFirstLocalVideoFrame(source, width, height, elapsed);
        }

        public override void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {
            base.OnJoinChannelSuccess(channel, uid, elapsed);
            Context.OnJoinChannelSuccess(channel, uid, elapsed);
        }

        public override void OnUserJoined(int uid, int elapsed)
        {
            base.OnUserJoined(uid, elapsed);
            Context.OnUserJoined(uid, elapsed);
        }
    }
}