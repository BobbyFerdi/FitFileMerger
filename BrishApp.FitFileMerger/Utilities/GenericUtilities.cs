namespace BrishApp.FitFileMerger.Utilities;

internal class GenericUtilities
{
    internal static string[] GetFitFiles() => Directory.GetFiles(@"..\..\..\Sources\", "*.fit");
}