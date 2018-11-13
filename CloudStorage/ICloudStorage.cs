using System.Threading.Tasks;

namespace Nde.TwitterExperiment.CloudStorage
{
    public interface ICloudStorage
    {
        #region Services

        Task<bool> InsertToTable<T>(T entity, string tableName);

        #endregion
    }
}