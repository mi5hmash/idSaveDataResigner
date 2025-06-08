using idSaveDataResigner.Helpers;

namespace idSaveDataResigner.Infrastructure;

public class Directories
{
    public string Output { get; } = Path.Combine(MyAppInfo.RootPath, "_OUTPUT");

    public void CreateAll()
    {
        Directory.CreateDirectory(Output);
    }
}