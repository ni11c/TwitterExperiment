using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nde.TwitterExperiment.CloudStorage
{
    public interface ICloudStorage
    {
        Task<bool> InsertToTable<T>(T entity, string tableName);
    }
}
