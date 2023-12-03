using BrishApp.FitFileMerger.Models;
using Dynastream.Fit;
using Serilog;
using System.IO;
using System.Text.Json;

namespace BrishApp.FitFileMerger.Utilities;

internal class DecodeUtility
{
    public DecodeUtility(ILogger logger) => _logger = logger;

    private readonly ILogger _logger;

    internal SourceMesgs ProcessFiles()
    {
        _logger.Information($"FIT Decode Example Application - Protocol {Fit.ProtocolMajorVersion}.{Fit.ProtocolMinorVersion} Profile {Fit.ProfileMajorVersion}.{Fit.ProfileMinorVersion}");

        FileStream fitSource = null;
        var sourceMesgs = new SourceMesgs();

        try
        {
            var files = Directory.GetFiles("..\\..\\..\\Sources\\", "*.fit");

            foreach (var file in files)
            {
                // Attempt to open .FIT file
                fitSource = new FileStream(file, FileMode.Open);
                _logger.Information($"Opening {file}");

                var decode = new Decode();

                // Use a FitListener to capture all decoded messages in a FitMessages object
                var fitListener = new FitListener();
                decode.MesgEvent += fitListener.OnMesg;

                // Use a custom event handlers to process messages as they are being decoded, and to
                // capture message definitions and developer field definitions
                decode.MesgEvent += OnMesgCustom;
                decode.MesgDefinitionEvent += OnMesgDefinitionCustom;
                decode.DeveloperFieldDescriptionEvent += OnDeveloperFieldDescriptionCustom;

                // Use a MesgBroadcaster for easy integration with existing projects
                //MesgBroadcaster mesgBroadcaster = new MesgBroadcaster();
                //mesgBroadcaster.MesgEvent += OnMesgCustom;
                //mesgBroadcaster.MesgDefinitionEvent += OnMesgDefinitionCustom;
                //decodeDemo.MesgEvent += mesgBroadcaster.OnMesg;
                //decodeDemo.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;

                _logger.Information("Decoding...");
                decode.Read(fitSource);

                var fitMessages = fitListener.FitMessages;
                //_logger.Information(JsonSerializer.Serialize(fitMessages));

                if (file.ToLower().Contains("activity"))
                {
                    sourceMesgs.FileIdMesgs.AddRange(fitMessages.FileIdMesgs.ToList());
                    sourceMesgs.FileCreatorMesgs.AddRange(fitMessages.FileCreatorMesgs.ToList());
                    sourceMesgs.DeviceSettingsMesgs.AddRange(fitMessages.DeviceSettingsMesgs.ToList());
                    sourceMesgs.UserProfileMesgs.AddRange(fitMessages.UserProfileMesgs.ToList());
                    sourceMesgs.TimeInZoneMesgs.AddRange(fitMessages.TimeInZoneMesgs.ToList());
                    sourceMesgs.ZonesTargetMesgs.AddRange(fitMessages.ZonesTargetMesgs.ToList());
                    sourceMesgs.SportMesgs.AddRange(fitMessages.SportMesgs.ToList());
                    sourceMesgs.ActivityMesgs.AddRange(fitMessages.ActivityMesgs.ToList());
                    sourceMesgs.SessionMesgs.AddRange(fitMessages.SessionMesgs.ToList());
                    sourceMesgs.LapMesgs.AddRange(fitMessages.LapMesgs.ToList());
                    sourceMesgs.RecordMesgs.AddRange(fitMessages.RecordMesgs.ToList());
                    sourceMesgs.EventMesgs.AddRange(fitMessages.EventMesgs.ToList());
                    sourceMesgs.DeviceInfoMesgs.AddRange(fitMessages.DeviceInfoMesgs.ToList());
                    sourceMesgs.TrainingFileMesgs.AddRange(fitMessages.TrainingFileMesgs.ToList());
                    sourceMesgs.SplitMesgs.AddRange(fitMessages.SplitMesgs.ToList());
                    sourceMesgs.SplitSummaryMesgs.AddRange(fitMessages.SplitSummaryMesgs.ToList());
                    sourceMesgs.DeveloperDataIdMesgs.AddRange(fitMessages.DeveloperDataIdMesgs.ToList());
                    sourceMesgs.WorkoutMesgs.AddRange(fitMessages.WorkoutMesgs.ToList());
                    sourceMesgs.WorkoutStepMesgs.AddRange(fitMessages.WorkoutStepMesgs.ToList());
                }
                else
                {
                    sourceMesgs.RecordMesgs.AddRange(fitMessages.RecordMesgs.ToList());
                }

                //foreach (var mesg in fitMessages.FileIdMesgs)
                //{
                //    PrintFileIdMesg(mesg);
                //}

                //foreach (var mesg in fitMessages.UserProfileMesgs)
                //{
                //    PrintUserProfileMesg(mesg);
                //}

                //foreach (var mesg in fitMessages.DeviceInfoMesgs)
                //{
                //    PrintDeviceInfoMesg(mesg);
                //}

                //foreach (var mesg in fitMessages.MonitoringMesgs)
                //{
                //    PrintMonitoringMesg(mesg);
                //}

                //foreach (var mesg in fitMessages.RecordMesgs)
                //{
                //    PrintRecordMesg(mesg);
                //}

                _logger.Information($"Decoded FIT file {file}");
            }

            //Console.Write("Press any key to continue...");
            //Console.ReadKey();
        }
        catch (FitException ex)
        {
            _logger.Information($"A FitException occurred when trying to decode the FIT file. Message: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.Information($"Exception occurred when trying to decode the FIT file. Message: {ex.Message}");
        }
        finally
        {
            fitSource?.Close();
        }

        return sourceMesgs;
    }

    private void OnMesgDefinitionCustom(object sender, MesgDefinitionEventArgs e)
    {
        _logger.Information($"OnMesgDef: Received Defn for local message #{e.mesgDef.LocalMesgNum}, global num {e.mesgDef.GlobalMesgNum}");
        _logger.Information($"\tIt has {e.mesgDef.NumFields} fields {e.mesgDef.NumDevFields} developer fields and is {e.mesgDef.GetMesgSize()} bytes long");
    }

    private void OnMesgCustom(object sender, MesgEventArgs e)
    {
        // TODO - Implement a custom Mesg handler for use with a decoder
        //switch (e.mesg.Num)
        //{
        //    case MesgNum.FileId:
        //        break;
        //    default:
        //        break;
        //}
    }

    private void OnDeveloperFieldDescriptionCustom(object sender, DeveloperFieldDescriptionEventArgs args)
    {
        _logger.Information("New Developer Field Description");
        _logger.Information($"   App Id: {args.Description.ApplicationId}");
        _logger.Information($"   App Version: {args.Description.ApplicationVersion}");
        _logger.Information($"   Field Number: {args.Description.FieldDefinitionNumber}");
    }

    public void PrintFileIdMesg(FileIdMesg mesg)
    {
        _logger.Information("File ID:");

        if (mesg.GetType() != null)
        {
            _logger.Information("   Type: ");
            _logger.Information(mesg.GetType().Value.ToString());
        }

        if (mesg.GetManufacturer() != null)
        {
            _logger.Information("   Manufacturer: ");
            _logger.Information(mesg.GetManufacturer().ToString());
        }

        if (mesg.GetProduct() != null)
        {
            _logger.Information("   Product: ");
            _logger.Information(mesg.GetProduct().ToString());
        }

        if (mesg.GetSerialNumber() != null)
        {
            _logger.Information("   Serial Number: ");
            _logger.Information(mesg.GetSerialNumber().ToString());
        }

        if (mesg.GetNumber() != null)
        {
            _logger.Information("   Number: ");
            _logger.Information(mesg.GetNumber().ToString());
        }
    }

    public void PrintUserProfileMesg(UserProfileMesg mesg)
    {
        _logger.Information("User profile:");

        if (mesg.GetFriendlyNameAsString() != null)
        {
            _logger.Information($"\tFriendlyName: \"{mesg.GetFriendlyNameAsString()}\"");
        }

        if (mesg.GetGender() != null)
        {
            _logger.Information($"\tGender: {mesg.GetGender()}");
        }

        if (mesg.GetAge() != null)
        {
            _logger.Information($"\tAge: {mesg.GetAge()}");
        }

        if (mesg.GetWeight() != null)
        {
            _logger.Information($"\tWeight:  {mesg.GetWeight()}");
        }
    }

    public void PrintDeviceInfoMesg(DeviceInfoMesg mesg)
    {
        _logger.Information("Device info:");

        if (mesg.GetTimestamp() != null)
        {
            _logger.Information($"\tTimestamp: {mesg.GetTimestamp().ToString()}");
        }

        if (mesg.GetBatteryStatus() != null)
        {
            _logger.Information("\tBattery Status: ");

            switch (mesg.GetBatteryStatus())
            {
                case BatteryStatus.Critical:
                    _logger.Information("Critical");
                    break;

                case BatteryStatus.Good:
                    _logger.Information("Good");
                    break;

                case BatteryStatus.Low:
                    _logger.Information("Low");
                    break;

                case BatteryStatus.New:
                    _logger.Information("New");
                    break;

                case BatteryStatus.Ok:
                    _logger.Information("OK");
                    break;

                default:
                    _logger.Information("Invalid");
                    break;
            }
        }
    }

    public void PrintMonitoringMesg(MonitoringMesg mesg)
    {
        _logger.Information("Monitoring:");

        if (mesg.GetTimestamp() != null)
        {
            _logger.Information($"\tTimestamp: {mesg.GetTimestamp().ToString()}");
        }

        if (mesg.GetActivityType() != null)
        {
            _logger.Information($"\tActivityType: {mesg.GetActivityType()}");
            switch (mesg.GetActivityType()) // Cycles is a dynamic field
            {
                case ActivityType.Walking:
                case ActivityType.Running:
                    _logger.Information($"\tSteps: {mesg.GetSteps()}");
                    break;

                case ActivityType.Cycling:
                case ActivityType.Swimming:
                    _logger.Information("\tStrokes: {0}", mesg.GetStrokes());
                    break;

                default:
                    _logger.Information($"\tCycles: {mesg.GetCycles()}");
                    break;
            }
        }
    }

    public void PrintRecordMesg(RecordMesg mesg)
    {
        _logger.Information("Record:");

        PrintFieldWithOverrides(mesg, RecordMesg.FieldDefNum.HeartRate);
        PrintFieldWithOverrides(mesg, RecordMesg.FieldDefNum.Cadence);
        PrintFieldWithOverrides(mesg, RecordMesg.FieldDefNum.Speed);
        PrintFieldWithOverrides(mesg, RecordMesg.FieldDefNum.Distance);
        PrintFieldWithOverrides(mesg, RecordMesg.FieldDefNum.EnhancedAltitude);

        PrintDeveloperFields(mesg);
    }

    private void PrintDeveloperFields(Mesg mesg)
    {
        foreach (var devField in mesg.DeveloperFields)
        {
            if (devField.GetNumValues() <= 0)
            {
                continue;
            }

            if (devField.IsDefined)
            {
                _logger.Information($"\t{devField.Name}");

                if (devField.Units != null)
                {
                    _logger.Information($" [{devField.Units}]");
                }
                _logger.Information(": ");
            }
            else
            {
                _logger.Information("\tUndefined Field: ");
            }

            _logger.Information($"{devField.GetValue(0)}");

            for (var i = 1; i < devField.GetNumValues(); i++)
            {
                _logger.Information($",{devField.GetValue(i)}");
            }

            //_logger.Information();
        }
    }

    private void PrintFieldWithOverrides(Mesg mesg, byte fieldNumber)
    {
        var profileField = Profile.GetField(mesg.Num, fieldNumber);
        var nameWritten = false;

        if (null == profileField)
        {
            return;
        }

        var fields = mesg.GetOverrideField(fieldNumber);

        foreach (var field in fields)
        {
            if (!nameWritten)
            {
                _logger.Information($"   {profileField.GetName()}");
                nameWritten = true;
            }

            if (field is Field)
            {
                _logger.Information($"      native: {field.GetValue()}");
            }
            else
            {
                _logger.Information($"      override: {field.GetValue()}");
            }
        }
    }
}