using ITS.Utility.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITS.Utility
{
    public interface IUtilitiesLibrary
    {
        //Database management functions

        public interface IDataAccess<TEntity,TKey>
            where TEntity : EntityBase<TKey>
        {
            IEnumerable<TEntity> GetAll(string connectionString);
            TEntity GetById(string connectionString, TKey id);
            DbResponse Insert(string connectionString, TEntity entity);
            DbResponse Delete(string connectionString, TKey id);
            DbResponse Update(string connectionString, TEntity entity);
        }

        //Azure communication service

        public interface IAzureCommunicationService<TEntity>
            where TEntity : EntityBase
        {
            public Task SendCloudToDeviceMessage(string hub_connectionString,string deviceId, TEntity message);
            public Task SendDeviceToCloudMessage(string dev_connectionString, TEntity message);
            public Task<TEntity> ReceiveCloudToDeviceMessage(string dev_connectionString);
        }
    }
}
