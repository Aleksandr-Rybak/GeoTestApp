namespace GeoApp.Services
{
    public interface ICsvHelperService
    {
        void ImportDataFromCsv(Stream stream);
    }
}
