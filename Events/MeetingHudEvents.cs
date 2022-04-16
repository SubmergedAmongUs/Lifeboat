namespace Lifeboat.Events;

public static class MeetingHudEvents
{
    public static void Clear()
    {
        OnMeetingStart = null;
        OnMeetingUpdate = null;
    }
    
    public delegate void MeetingStartEvent(MeetingHud meetingHud);
    public static MeetingStartEvent OnMeetingStart;

    public delegate void MeetingUpdateEvent(MeetingHud meetingHud);
    public static MeetingUpdateEvent OnMeetingUpdate;
}