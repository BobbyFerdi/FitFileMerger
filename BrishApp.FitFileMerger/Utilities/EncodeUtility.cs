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
        // Generate some FIT messages
        //var fileIdMesg = new FileIdMesg(); // Every FIT file MUST contain a 'File ID' message as the first message
        //var developerIdMesg = new DeveloperDataIdMesg();
        //var fieldDescMesg = new FieldDescriptionMesg();

        //var records = new List<RecordMesg>();

        //byte[] appId = {
        //        1, 2, 3, 4,
        //        5, 6, 7, 8,
        //        9, 10, 11, 12,
        //        13, 14, 15, 16
        //    };

        //fileIdMesg.SetType(Dynastream.Fit.File.Activity);
        //fileIdMesg.SetManufacturer(Manufacturer.Development);
        //fileIdMesg.SetProduct(1);
        //fileIdMesg.SetSerialNumber(12345);
        //fileIdMesg.SetTimeCreated(new Dynastream.Fit.DateTime(621463080));

        //for (var i = 0; i < appId.Length; i++)
        //{
        //    developerIdMesg.SetApplicationId(i, appId[i]);
        //}

        //developerIdMesg.SetDeveloperDataIndex(0);

        //fieldDescMesg.SetDeveloperDataIndex(0);
        //fieldDescMesg.SetFieldDefinitionNumber(0);
        //fieldDescMesg.SetFitBaseTypeId(FitBaseType.Sint8);
        //fieldDescMesg.SetFieldName(0, "doughnuts_earned");
        //fieldDescMesg.SetUnits(0, "doughnuts");

        //for (int i = 0; i < 3; i++)
        //{
        //    var newRecord = new RecordMesg();
        //    var doughnutsEarnedField = new DeveloperField(fieldDescMesg, developerIdMesg);
        //    newRecord.SetDeveloperField(doughnutsEarnedField);

        //    newRecord.SetHeartRate((byte)(140 + (i * 2)));
        //    newRecord.SetCadence((byte)(88 + (i * 2)));
        //    newRecord.SetDistance(510 + (i * 100));
        //    newRecord.SetSpeed(2.8f + (i * 0.4f));
        //    doughnutsEarnedField.SetValue(i + 1);

        //    records.Add(newRecord);
        //}

        // Create file encode object
        var encode = new Encode(ProtocolVersion.V20);
        var fileName = $"BrishApp.FitFileMerger-{System.DateTime.Now:yyyy-MM-dd HH.mm.ss.ffff}.fit";
        var fitDest = new FileStream($"..\\..\\..\\Results\\{fileName}", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        // Write our header
        encode.Open(fitDest);

        // Encode each message, a definition message is automatically generated and output if necessary
        //encode.Write(fileIdMesg);
        //encode.Write(developerIdMesg);
        //encode.Write(fieldDescMesg);
        //encode.Write(records);

        encode.Write(source.FileIdMesgs);
        encode.Write(source.FileCreatorMesgs);
        encode.Write(source.DeviceSettingsMesgs);
        encode.Write(source.UserProfileMesgs);
        encode.Write(source.TimeInZoneMesgs);
        encode.Write(source.ZonesTargetMesgs);
        encode.Write(source.SportMesgs);
        encode.Write(source.ActivityMesgs);
        encode.Write(source.SessionMesgs);
        encode.Write(source.LapMesgs);
        encode.Write(source.RecordMesgs);
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