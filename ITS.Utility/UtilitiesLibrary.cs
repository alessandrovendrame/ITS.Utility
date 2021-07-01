using ITS.Utility.Models;
using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using System.Data.SqlClient;
using System.Text;
using static ITS.Utility.IUtilitiesLibrary;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Text.Json;
using Microsoft.Azure.Devices;

namespace ITS.Utility
{
    public class UtilitiesLibrary : IUtilitiesLibrary
    {
        //Database management functions

        public class DataAccess<TEntity, TKey> : IDataAccess<TEntity, TKey>
            where TEntity : EntityBase<TKey>
        {
            private DbResponse response = new DbResponse();
            public DbResponse Delete(string connectionString, TKey id)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var entityToDelete = connection.Get<TEntity>(id);
                        connection.Delete(entityToDelete);
                        response.Code = 200;
                        response.Message = "Record deleted successfully.";
                        return response;
                    }
                }catch(Exception e)
                {
                    response.Code = 500;
                    response.Message = e.Message;
                    return response;
                }
            }

            public IEnumerable<TEntity> GetAll(string connectionString)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        return connection.GetAll<TEntity>();
                    }
                }catch(Exception e)
                {
                    return null;
                }
                
            }

            public TEntity GetById(string connectionString, TKey id)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        return connection.Get<TEntity>(id);
                    }
                }catch(Exception e)
                {
                    return null;
                }
            }

            public DbResponse Insert(string connectionString, TEntity entity)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Insert<TEntity>(entity);

                        response.Code = 200;
                        response.Message = "Record inserted succesfully";

                        return response;
                    }
                }
                catch (Exception e)
                {
                    response.Code = 500;
                    response.Message = e.Message;
                    return response;
                }
            }

            public DbResponse Update(string connectionString, TEntity entity)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Update(entity);

                        response.Code = 200;
                        response.Message = "Record updated succesfully";

                        return response;
                    }
                }
                catch (Exception e)
                {
                    response.Code = 500;
                    response.Message = e.Message;
                    return response;
                }
            }
        }

        //Azure communication service

        public class AzureCommunicationService<TEntity> : IAzureCommunicationService<TEntity>
            where TEntity : EntityBase
        {

            //To use this function you have to put it inside a while(true) and to get data use .Result
            //azureCommunicationService.ReceiveCloudToDeviceMessage(dev_cs).Result.yourPropertyName
            //
            public async Task<TEntity> ReceiveCloudToDeviceMessage(string dev_connectionString)
            {
                var s_deviceClient = DeviceClient.CreateFromConnectionString(dev_connectionString);
                
                Microsoft.Azure.Devices.Client.Message receivedMessage = await s_deviceClient.ReceiveAsync();
                var msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                if (receivedMessage == null) return null;

                TEntity message = JsonSerializer.Deserialize<TEntity>(msg);

                await s_deviceClient.CompleteAsync(receivedMessage);
                  
                return message;           
                
            }


            //To use this function you have to put .Wait() after call
            //azureCommunicationService.SendCloudToDeviceMessage(hub_cs, "yourDeviceId", message).Wait();
            //
            public async Task SendCloudToDeviceMessage(string hub_connectionString, string deviceId, TEntity message)
            {
                try
                {
                    var serviceClient = ServiceClient.CreateFromConnectionString(hub_connectionString);

                    string mts = JsonSerializer.Serialize(message);
                    var cloudMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(mts));

                    await serviceClient.SendAsync(deviceId, cloudMessage);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
            }

            public async Task SendDeviceToCloudMessage(string dev_connectionString, TEntity message)
            {
                try
                {
                    var s_deviceClient = DeviceClient.CreateFromConnectionString(dev_connectionString);
                    string mts = JsonSerializer.Serialize(message);
                    var deviceMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(mts));

                    await s_deviceClient.SendEventAsync(deviceMessage);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
            }
        }
    }
}
