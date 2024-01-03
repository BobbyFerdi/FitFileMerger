using BrishApp.FitFileMerger.Models;
using Dynastream.Fit;
using Serilog;
using System.IO.Compression;

namespace BrishApp.FitFileMerger.Utilities;

internal class EncodeUtility
{
    private readonly ILogger _logger;

    public EncodeUtility(ILogger logger) => _logger = logger;

    internal void EncodeActivityFile(SourceMesgs source)
    {
        // Create file encode object
        try
        {
            #region Record

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

            #endregion Record

            #region Session

            if (totalPowerCount == 0) totalPowerCount = 1;
            if (totalSpeedCount == 0) totalSpeedCount = 1;
            if (totalCadenceCount == 0) totalCadenceCount = 1;

            var avgPower = Convert.ToUInt16(totalPower / totalPowerCount);
            var avgSpeed = Convert.ToSingle(totalSpeed / totalSpeedCount);
            var avgCadence = Math.Ceiling(Convert.ToSingle(totalCadence / totalCadenceCount));
            var session = source.SessionMesgs[0];

            session.SetAvgCadence((byte)avgCadence);
            session.SetAvgPower(avgPower);
            session.SetAvgSpeed(avgSpeed);
            session.SetEnhancedAvgSpeed(avgSpeed);
            session.SetEnhancedMaxSpeed(maxSpeed);
            session.SetMaxCadence((byte)maxCadence);
            session.SetMaxPower(maxPower);
            session.SetMaxSpeed(maxSpeed);
            session.SetTotalCalories(source.SessionMesgs[1].GetTotalCalories());
            session.SetTotalDescent(source.SessionMesgs[1].GetTotalDescent());
            session.SetTotalDistance(source.SessionMesgs[1].GetTotalDistance());

            #endregion Session

            #region Lap

            var laps = new List<LapMesg>();

            foreach (var lap in source.LapMesgs)
            {
                var startTime = lap.GetStartTime();
                var endTime = lap.GetTimestamp();
                var lapRecords = records.Where(x => startTime.GetDateTime() <= x.GetTimestamp().GetDateTime() && x.GetTimestamp().GetDateTime() <= endTime.GetDateTime());
                var startDistance = lapRecords.Min(x => x.GetDistance());
                var endDistance = lapRecords.Max(x => x.GetDistance());
                var l = lap;

                l.SetTotalCalories(Convert.ToUInt16(lapRecords.Max(x => x.GetCalories()) - lapRecords.Min(x => x.GetCalories())));
                l.SetStartTime(startTime);
                l.SetTimestamp(endTime);
                l.SetTotalDistance(endDistance - startDistance);
                l.SetEnhancedAvgSpeed(lapRecords.Average(x => x.GetEnhancedSpeed()));

                laps.Add(l);
            }

            #endregion Lap

            #region Split

            var splits = new List<SplitMesg>();

            foreach (var split in source.SplitMesgs)
            {
                var startTime = split.GetStartTime();
                var endTime = split.GetEndTime();
                var splitRecords = records.Where(x => startTime.GetDateTime() <= x.GetTimestamp().GetDateTime() && x.GetTimestamp().GetDateTime() <= endTime.GetDateTime());
                var startDistance = splitRecords.Min(x => x.GetDistance());
                var endDistance = splitRecords.Max(x => x.GetDistance());
                var s = split;
                s.SetTotalDistance(endDistance - startDistance);
                s.SetAvgSpeed(splitRecords.Average(x => x.GetSpeed()));
                s.SetTotalCalories(splitRecords.Max(x => x.GetCalories()));
                splits.Add(s);
            }

            var splitsSummary = new List<SplitSummaryMesg>();
            var splitCounter = 0;

            foreach (var sum in source.SplitSummaryMesgs)
            {
                var numSplits = sum.GetNumSplits();
                var splitsSum = splits.Where(x => splitCounter <= x.GetMessageIndex() && x.GetMessageIndex() <= splitCounter + numSplits - 1);
                var s = sum;
                s.SetTotalDistance(splitCounter == 0 ? splits.FirstOrDefault(x => x.GetMessageIndex() == splitCounter).GetTotalDistance() : splitsSum.Max(x => x.GetTotalDistance()) - splitsSum.Min(x => x.GetTotalDistance()));
                s.SetAvgSpeed(splitsSum.Average(x => x.GetAvgSpeed()));
                s.SetTotalCalories(Convert.ToUInt16(splitsSum.Max(x => x.GetTotalCalories())));
                splitCounter += Convert.ToInt32(numSplits);

                splitsSummary.Add(s);
            }

            #endregion Split

            var encode = new Encode(ProtocolVersion.V20);
            var fileName = $"BrishApp.FitFileMerger-{System.DateTime.Now:yyyy-MM-dd~HH.mm.ss.ffff}";
            var resultName = $"..\\..\\..\\Results\\{fileName}.fit";
            var fitDest = new FileStream(resultName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            // Write our header
            encode.Open(fitDest);
            encode.Write(source.FileIdMesgs);
            encode.Write(source.FileCreatorMesgs);
            encode.Write(source.EventMesgs);
            encode.Write(source.DeviceInfoMesgs);
            encode.Write(source.DeviceSettingsMesgs);
            encode.Write(source.UserProfileMesgs);
            encode.Write(source.SportMesgs);
            encode.Write(source.ZonesTargetMesgs);
            encode.Write(source.TrainingFileMesgs);
            encode.Write(source.WorkoutMesgs);
            encode.Write(source.WorkoutStepMesgs);
            encode.Write(records);
            encode.Write(laps);
            encode.Write(source.TimeInZoneMesgs);
            encode.Write(splits);
            encode.Write(splitsSummary);
            encode.Write(session);
            encode.Write(source.ActivityMesgs);

            // Update header datasize and file CRC
            encode.Close();
            fitDest.Close();

            _logger.Information($"Encoded FIT file {resultName}");

            #region Zip source file

            var zipFile = $"..\\..\\..\\Sources\\{System.DateTime.Now:yyyyMMdd}.zip";
            System.IO.File.Delete(zipFile);

            using ZipArchive newFile = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            var folder = new DirectoryInfo("..\\..\\..\\Sources\\");

            foreach (string file in GenericUtilities.GetFitFiles())
            {
                newFile.CreateEntryFromFile(file, Path.GetRelativePath(folder.FullName, file), CompressionLevel.SmallestSize);
                System.IO.File.Delete(file);
            }

            #endregion Zip source file
        }
        catch (Exception e)
        {
            _logger.Error($"ERROR: {e.Message}.");

            if (e.InnerException != null) _logger.Error($"Inner Exception:\n{e.InnerException}");

            throw;
        }
    }
}