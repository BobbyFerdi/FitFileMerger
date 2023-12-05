using BrishApp.FitFileMerger.Models;
using Dynastream.Fit;
using Serilog;

namespace BrishApp.FitFileMerger.Utilities;

internal class EncodeUtility
{
    private readonly ILogger _logger;

    public EncodeUtility(ILogger logger) => _logger = logger;

    internal void EncodeActivityFile(SourceMesgs source)
    {
        // Create file encode object
        var encode = new Encode(ProtocolVersion.V20);
        var fileName = $"BrishApp.FitFileMerger-{System.DateTime.Now:yyyy-MM-dd HH.mm.ss.ffff}.fit";
        var fitDest = new FileStream($"..\\..\\..\\Results\\{fileName}", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        // Write our header
        encode.Open(fitDest);
        encode.Write(source.FileIdMesgs);
        encode.Write(source.FileCreatorMesgs);
        encode.Write(source.DeviceSettingsMesgs);
        encode.Write(source.UserProfileMesgs);
        encode.Write(source.TimeInZoneMesgs);
        encode.Write(source.ZonesTargetMesgs);
        encode.Write(source.SportMesgs);
        encode.Write(source.ActivityMesgs);

        encode.Write(source.LapMesgs);
        var records = new List<RecordMesg>();
        ushort maxPower = 0;
        int totalPower = 0;
        var totalPowerCount = 0;
        float maxSpeed = 0;
        float totalSpeed = 0;
        var totalSpeedCount = 0;
        var maxCadence = 0;
        var totalCadence = 0;
        var totalCadenceCount = 0;

        foreach (var record in source.RecordMesgs[1])
        {
            var newRecord = new RecordMesg();
            var closestTimestamp = source.RecordMesgs[0].MinBy(t => Math.Abs((t.DateTime - record.DateTime).Ticks));
            var power = record.RecordMesg.GetPower();
            var speed = record.RecordMesg.GetEnhancedSpeed();
            var cadence = record.RecordMesg.GetCadence();
            newRecord.SetCadence(cadence);
            newRecord.SetCalories(record.RecordMesg.GetCalories());
            newRecord.SetDistance(record.RecordMesg.GetDistance());
            newRecord.SetEnhancedSpeed(record.RecordMesg.GetEnhancedSpeed());
            newRecord.SetPower(power);
            newRecord.SetResistance(record.RecordMesg.GetResistance());
            newRecord.SetSpeed(speed);
            newRecord.SetHeartRate(closestTimestamp?.RecordMesg.GetHeartRate() ?? 0);
            newRecord.SetTimestamp(record.RecordMesg.GetTimestamp());
            records.Add(newRecord);

            if (power != null && power > 0)
            {
                totalPowerCount++;

                if (power > maxPower) maxPower = power ?? 0;

                totalPower += Convert.ToInt32(power);
            }

            if (speed != null && speed > 0)
            {
                totalSpeedCount++;

                if (speed > maxSpeed) maxSpeed = speed ?? 0;

                totalSpeed += speed ?? 0;
            }

            if (cadence != null && cadence > 0)
            {
                totalCadenceCount++;

                if (cadence > maxCadence) maxCadence = cadence ?? 0;

                totalCadence += cadence ?? 0;
            }
        }

        encode.Write(records);

        var avgPower = Convert.ToUInt16(totalPower / totalPowerCount);
        var avgSpeed = Convert.ToSingle(totalSpeed / totalSpeedCount);
        var avgCadence = Math.Ceiling(Convert.ToSingle(totalCadence / totalCadenceCount));
        var session = new SessionMesg
        {
            Name = source.SessionMesgs[0].Name,
            Num = source.SessionMesgs[0].Num,
            LocalNum = source.SessionMesgs[0].LocalNum
        };

        session.SetAvgCadence((byte)avgCadence);
        session.SetAvgHeartRate(source.SessionMesgs[0].GetAvgHeartRate());
        session.SetAvgLapTime(source.SessionMesgs[0].GetAvgLapTime());
        session.SetAvgPower(avgPower);
        session.SetAvgRespirationRate(source.SessionMesgs[0].GetAvgRespirationRate());
        session.SetAvgSpeed(avgSpeed);
        session.SetAvgSpo2(source.SessionMesgs[0].GetAvgSpo2());
        session.SetAvgStress(source.SessionMesgs[0].GetAvgStress());
        session.SetEnhancedAvgAltitude(source.SessionMesgs[0].GetEnhancedAvgAltitude());
        session.SetEnhancedAvgRespirationRate(source.SessionMesgs[0].GetEnhancedAvgRespirationRate());
        session.SetEnhancedAvgSpeed(avgSpeed);
        session.SetEnhancedMaxAltitude(source.SessionMesgs[0].GetEnhancedMaxAltitude());
        session.SetEnhancedMaxRespirationRate(source.SessionMesgs[0].GetEnhancedMaxRespirationRate());
        session.SetEnhancedMaxSpeed(maxSpeed);
        session.SetEnhancedMinAltitude(source.SessionMesgs[0].GetEnhancedMinAltitude());
        session.SetEnhancedMinRespirationRate(source.SessionMesgs[0].GetEnhancedMinRespirationRate());
        session.SetEvent(source.SessionMesgs[0].GetEvent());
        session.SetEventGroup(source.SessionMesgs[0].GetEventGroup());
        session.SetEventType(source.SessionMesgs[0].GetEventType());
        session.SetFirstLapIndex(source.SessionMesgs[0].GetFirstLapIndex());
        session.SetMaxAltitude(source.SessionMesgs[1].GetMaxAltitude());
        session.SetMaxCadence((byte)maxCadence);
        session.SetMaxFractionalCadence(source.SessionMesgs[1].GetMaxFractionalCadence());
        session.SetMaxHeartRate(source.SessionMesgs[0].GetMaxHeartRate());
        session.SetMaxPower(maxPower);
        session.SetMaxRespirationRate(source.SessionMesgs[0].GetMaxRespirationRate());
        session.SetMaxSpeed(maxSpeed);
        session.SetMessageIndex(source.SessionMesgs[0].GetMessageIndex());
        session.SetMinAltitude(source.SessionMesgs[1].GetMinAltitude());
        session.SetMinHeartRate(source.SessionMesgs[0].GetMinHeartRate());
        session.SetMinRespirationRate(source.SessionMesgs[0].GetMinRespirationRate());
        session.SetNumActiveLengths(source.SessionMesgs[0].GetNumActiveLengths());
        session.SetNumLaps(source.SessionMesgs[0].GetNumLaps());
        session.SetNumLengths(source.SessionMesgs[0].GetNumLengths());
        session.SetO2Toxicity(source.SessionMesgs[0].GetO2Toxicity());
        session.SetRmssdHrv(source.SessionMesgs[0].GetRmssdHrv());
        session.SetSdrrHrv(source.SessionMesgs[0].GetSdrrHrv());
        session.SetSport(source.SessionMesgs[0].GetSport());
        session.SetSportIndex(source.SessionMesgs[0].GetSportIndex());
        session.SetSportProfileName(source.SessionMesgs[0].GetSportProfileName());
        session.SetStandCount(source.SessionMesgs[0].GetStandCount());
        session.SetStartTime(source.SessionMesgs[0].GetStartTime());
        session.SetSubSport(source.SessionMesgs[0].GetSubSport());
        session.SetThresholdPower(source.SessionMesgs[1].GetThresholdPower());
        session.SetTimestamp(source.SessionMesgs[0].GetTimestamp());
        session.SetTimeStanding(source.SessionMesgs[1].GetTimeStanding());
        session.SetTotalAnaerobicTrainingEffect(source.SessionMesgs[0].GetTotalAnaerobicTrainingEffect());
        session.SetTotalAscent(source.SessionMesgs[1].GetTotalAscent());
        session.SetTotalCalories(source.SessionMesgs[1].GetTotalCalories());
        session.SetTotalCycles(source.SessionMesgs[0].GetTotalCycles());
        session.SetTotalDescent(source.SessionMesgs[1].GetTotalDescent());
        session.SetTotalDistance(source.SessionMesgs[1].GetTotalDistance());
        session.SetTotalElapsedTime(source.SessionMesgs[0].GetTotalElapsedTime());
        session.SetTotalFatCalories(source.SessionMesgs[1].GetTotalFatCalories());
        session.SetTotalFlow(source.SessionMesgs[0].GetTotalFlow());
        session.SetTotalFractionalAscent(source.SessionMesgs[0].GetTotalFractionalAscent());
        session.SetTotalFractionalCycles(source.SessionMesgs[0].GetTotalFractionalCycles());
        session.SetTotalFractionalDescent(source.SessionMesgs[0].GetTotalFractionalDescent());
        session.SetTotalGrit(source.SessionMesgs[0].GetTotalGrit());
        session.SetTotalMovingTime(source.SessionMesgs[0].GetTotalMovingTime());
        session.SetTotalStrides(source.SessionMesgs[0].GetTotalStrides());
        session.SetTotalStrokes(source.SessionMesgs[0].GetTotalStrokes());
        session.SetTotalTimerTime(source.SessionMesgs[0].GetTotalTimerTime());
        session.SetTotalTrainingEffect(source.SessionMesgs[0].GetTotalTrainingEffect());
        session.SetTotalWork(source.SessionMesgs[0].GetTotalWork());
        session.SetTrainingLoadPeak(source.SessionMesgs[0].GetTrainingLoadPeak());
        session.SetTrainingStressScore(source.SessionMesgs[0].GetTrainingStressScore());
        session.SetTrigger(source.SessionMesgs[0].GetTrigger());

        encode.Write(session);

        encode.Write(source.EventMesgs);
        encode.Write(source.DeviceInfoMesgs);
        encode.Write(source.TrainingFileMesgs);
        encode.Write(source.SplitMesgs);
        encode.Write(source.SplitSummaryMesgs);
        encode.Write(source.DeveloperDataIdMesgs);
        encode.Write(source.WorkoutMesgs);
        encode.Write(source.WorkoutStepMesgs);

        // Update header datasize and file CRC
        encode.Close();
        fitDest.Close();

        _logger.Information($"Encoded FIT file {fileName}");
    }
}