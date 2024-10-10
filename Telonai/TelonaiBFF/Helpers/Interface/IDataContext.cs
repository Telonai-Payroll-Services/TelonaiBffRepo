namespace TelonaiWebApi.Helpers.Interface
{
    public interface IDataContext
    {
        public Task<int> SaveChangesAsync();
    }
}
