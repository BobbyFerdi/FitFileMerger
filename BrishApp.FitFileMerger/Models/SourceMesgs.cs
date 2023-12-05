using Dynastream.Fit;

namespace BrishApp.FitFileMerger.Models;

internal class SourceMesgs
{
    public SourceMesgs()
    {
        FileIdMesgs = new List<FileIdMesg>();
        FileCreatorMesgs = new List<FileCreatorMesg>();
        DeviceSettingsMesgs = new List<DeviceSettingsMesg>();
        UserProfileMesgs = new List<UserProfileMesg>();
        TimeInZoneMesgs = new List<TimeInZoneMesg>();
        ZonesTargetMesgs = new List<ZonesTargetMesg>();
        SportMesgs = new List<SportMesg>();
        ActivityMesgs = new List<ActivityMesg>();
        SessionMesgs = new List<SessionMesg>();
        LapMesgs = new List<LapMesg>();
        RecordMesgs = new List<List<Record>>();
        EventMesgs = new List<EventMesg>();
        DeviceInfoMesgs = new List<DeviceInfoMesg>();
        TrainingFileMesgs = new List<TrainingFileMesg>();
        SplitMesgs = new List<SplitMesg>();
        SplitSummaryMesgs = new List<SplitSummaryMesg>();
        DeveloperDataIdMesgs = new List<DeveloperDataIdMesg>();
        WorkoutMesgs = new List<WorkoutMesg>();
        WorkoutStepMesgs = new List<WorkoutStepMesg>();
    }

    public List<FileIdMesg> FileIdMesgs { get; set; }
    public List<FileCreatorMesg> FileCreatorMesgs { get; set; }
    public List<DeviceSettingsMesg> DeviceSettingsMesgs { get; set; }
    public List<UserProfileMesg> UserProfileMesgs { get; set; }
    public List<TimeInZoneMesg> TimeInZoneMesgs { get; set; }
    public List<ZonesTargetMesg> ZonesTargetMesgs { get; set; }
    public List<SportMesg> SportMesgs { get; set; }
    public List<ActivityMesg> ActivityMesgs { get; set; }
    public List<SessionMesg> SessionMesgs { get; set; }
    public List<LapMesg> LapMesgs { get; set; }
    public List<List<Record>> RecordMesgs { get; set; }
    public List<EventMesg> EventMesgs { get; set; }
    public List<DeviceInfoMesg> DeviceInfoMesgs { get; set; }
    public List<TrainingFileMesg> TrainingFileMesgs { get; set; }
    public List<SplitMesg> SplitMesgs { get; set; }
    public List<SplitSummaryMesg> SplitSummaryMesgs { get; set; }
    public List<DeveloperDataIdMesg> DeveloperDataIdMesgs { get; set; }
    public List<WorkoutMesg> WorkoutMesgs { get; set; }
    public List<WorkoutStepMesg> WorkoutStepMesgs { get; set; }
}