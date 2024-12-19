using BrishApp.FitFileMerger.Models;
using Dynastream.Fit;
using Serilog;

namespace BrishApp.FitFileMerger.Utilities;

internal class DecodeUtility
{
    public DecodeUtility(ILogger? logger) => _logger = logger;

    private readonly ILogger? _logger;

    internal SourceMesgs ProcessFiles()
    {
        _logger.Information($"FIT Decode Example Application - Protocol {Fit.ProtocolMajorVersion}.{Fit.ProtocolMinorVersion} Profile {Fit.ProfileMajorVersion}.{Fit.ProfileMinorVersion}");

        FileStream fitSource = null;
        var sourceMesgs = new SourceMesgs();

        try
        {
            var files = GenericUtilities.GetFitFiles();
            var orderedFiles = new string[2];

            foreach (var file in files)
            {
                if (file.ToLower().Contains("activity"))
                {
                    orderedFiles[0] = file;
                }
                else
                {
                    orderedFiles[1] = file;
                }
            }

            foreach (var file in orderedFiles)
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
                _logger.Information("Decoding...");
                decode.Read(fitSource);
                var fitMessages = fitListener.FitMessages;

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
                    sourceMesgs.LapMesgs.AddRange(fitMessages.LapMesgs.ToList());
                    sourceMesgs.EventMesgs.AddRange(fitMessages.EventMesgs.ToList());
                    sourceMesgs.DeviceInfoMesgs.AddRange(fitMessages.DeviceInfoMesgs.ToList());
                    sourceMesgs.TrainingFileMesgs.AddRange(fitMessages.TrainingFileMesgs.ToList());
                    sourceMesgs.SplitMesgs.AddRange(fitMessages.SplitMesgs.ToList());
                    sourceMesgs.SplitSummaryMesgs.AddRange(fitMessages.SplitSummaryMesgs.ToList());
                    sourceMesgs.WorkoutMesgs.AddRange(fitMessages.WorkoutMesgs.ToList());
                    sourceMesgs.WorkoutStepMesgs.AddRange(fitMessages.WorkoutStepMesgs.ToList());
                }

                var records = fitMessages.RecordMesgs.Select(record => new Record { DateTime = record.GetTimestamp().GetDateTime(), RecordMesg = record }).ToList();
                sourceMesgs.RecordMesgs.Add(records);
                sourceMesgs.SessionMesgs.Add(fitMessages.SessionMesgs[0]);
                _logger.Information($"Decoded FIT file {file}");
            }
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

    private static void OnMesgCustom(object sender, MesgEventArgs e)
    {
    }

    private void OnDeveloperFieldDescriptionCustom(object sender, DeveloperFieldDescriptionEventArgs args)
    {
        _logger.Information("New Developer Field Description");
        _logger.Information($"   App Id: {args.Description.ApplicationId}");
        _logger.Information($"   App Version: {args.Description.ApplicationVersion}");
        _logger.Information($"   Field Number: {args.Description.FieldDefinitionNumber}");
    }
}