using ECommon.Configurations;
using ECommon.Extensions;
using ECommon.Socketing;
using EQueue.Broker;
using EQueue.Configurations;
using System;
using System.Collections.Generic;
using System.Net;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace EQueueBrokerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeEQueue();


            #region Environments Configs
            var _brokerAddress = Environment.GetEnvironmentVariable("BROKERSERVER_BINDINGADDRESS");
            var _nameServerAddress = Environment.GetEnvironmentVariable("BROKERSERVER_NAMESERVERADDRESS");
            var _nameServerPort = Environment.GetEnvironmentVariable("BROKERSERVER_NAMESERVERPORT");
            var _isMemoryMode = Environment.GetEnvironmentVariable("BROKERSERVER_ISMEMORYMODE");
            var _fileStoreRootPath = Environment.GetEnvironmentVariable("BROKERSERVER_FILESTOREROOTPATH");
            var _flushInterval = Environment.GetEnvironmentVariable("BROKERSERVER_FLUSHINTERVAL");
            var _chunkCacheMaxCount = Environment.GetEnvironmentVariable("BROKERSERVER_CHUNKCACHEMAXCOUNT");
            var _chunkCacheMinCount = Environment.GetEnvironmentVariable("BROKERSERVER_CHUNKCACHEMINCOUNT");
            var _chunkSize = Environment.GetEnvironmentVariable("BROKERSERVER_CHUNKSIZE");
            var _chunkWriteBuffer = Environment.GetEnvironmentVariable("BROKERSERVER_CHUNKWRITEBUFFER");
            var _enableCache = Environment.GetEnvironmentVariable("BROKERSERVER_ENABLECACHE");
            var _syncFlush = Environment.GetEnvironmentVariable("BROKERSERVER_SYNCFLUSH");
            var _notifyWhenMessageArrived = Environment.GetEnvironmentVariable("BROKERSERVER_NOTIFYWHENMESSAGEARRIVED");
            var _messageWriteQueueThreshold = Environment.GetEnvironmentVariable("BROKERSERVER_MESSAGEWRITEQUEUETHRESHOLD");
            var _deleteMessageIgnoreUnConsumed = Environment.GetEnvironmentVariable("BROKERSERVER_DELETEMESSAGEIGNOREUNCONSUMED");
            var _brokerName = Environment.GetEnvironmentVariable("BROKERSERVER_BROKERNAME");
            var _groupName = Environment.GetEnvironmentVariable("BROKERSERVER_GROUPNAME");
            var _producerPort = Environment.GetEnvironmentVariable("BROKERSERVER_PRODUCERPORT");
            var _consumerPort = Environment.GetEnvironmentVariable("BROKERSERVER_CONSUMERPORT");
            var _adminPort = Environment.GetEnvironmentVariable("BROKERSERVER_ADMINPORT");
            #endregion

            var brokerAddress = string.IsNullOrEmpty(_brokerAddress) ? SocketUtils.GetLocalIPV4() : IPAddress.Parse(_brokerAddress);
            var nameServerAddress = string.IsNullOrEmpty(_nameServerAddress) ? SocketUtils.GetLocalIPV4() : IPAddress.Parse(_nameServerAddress);
            var nameServerPort = !string.IsNullOrEmpty(_nameServerPort) && int.TryParse(_nameServerPort, out int nsPort) ? nsPort : 9493;
            var isMemoryMode = !string.IsNullOrEmpty(_isMemoryMode) && bool.TryParse(_isMemoryMode, out bool outIsMemoryMode) ? outIsMemoryMode : false;
            var fileStoreRootPath = !string.IsNullOrEmpty(_fileStoreRootPath) ? _fileStoreRootPath : "/var/lib/equeue/store";
            var flushInterval = !string.IsNullOrEmpty(_flushInterval) && int.TryParse(_flushInterval, out int outFlushInterval) ? outFlushInterval : 100; // Milliseconds
            var chunkCacheMaxCount = !string.IsNullOrEmpty(_chunkCacheMaxCount) && int.TryParse(_chunkCacheMaxCount, out int outChunkCacheMaxCount) ? outChunkCacheMaxCount : 2;
            var chunkCacheMinCount = !string.IsNullOrEmpty(_chunkCacheMinCount) && int.TryParse(_chunkCacheMinCount, out int outChunkCacheMinCount) ? outChunkCacheMinCount : 1;
            var chunkSize = !string.IsNullOrEmpty(_chunkSize) && int.TryParse(_chunkSize, out int outChunkSize) ? outChunkSize : 256; // MB
            var chunkWriteBuffer = !string.IsNullOrEmpty(_chunkWriteBuffer) && int.TryParse(_chunkWriteBuffer, out int outChunkWriteBuffer) ? outChunkWriteBuffer : 256;//KB
            var enableCache = !string.IsNullOrEmpty(_enableCache) && bool.TryParse(_enableCache, out bool outEnableCache) ? outEnableCache : true;
            var syncFlush = !string.IsNullOrEmpty(_syncFlush) && bool.TryParse(_syncFlush, out bool outSyncFlush) ? outSyncFlush : false;
            var notifyWhenMessageArrived = !string.IsNullOrEmpty(_notifyWhenMessageArrived) && bool.TryParse(_notifyWhenMessageArrived, out bool outNotifyWhenMessageArrived) ? outNotifyWhenMessageArrived : true;
            var messageWriteQueueThreshold = !string.IsNullOrEmpty(_messageWriteQueueThreshold) && int.TryParse(_messageWriteQueueThreshold, out int outMessageWriteQueueThreshold) ? outMessageWriteQueueThreshold : 10000;
            var deleteMessageIgnoreUnConsumed = !string.IsNullOrEmpty(_deleteMessageIgnoreUnConsumed) && bool.TryParse(_deleteMessageIgnoreUnConsumed, out bool outDeleteMessageIgnoreUnConsumed) ? outDeleteMessageIgnoreUnConsumed : false;
            var brokerName = !string.IsNullOrEmpty(_brokerName) ? _brokerName : "Broker1";
            var groupName = !string.IsNullOrEmpty(_groupName) ? _groupName : "BrokerGroup1";
            var producerPort = !string.IsNullOrEmpty(_producerPort) && int.TryParse(_producerPort, out int outProducerPort) ? outProducerPort : 5000;
            var consumerPort = !string.IsNullOrEmpty(_consumerPort) && int.TryParse(_consumerPort, out int outConsumerPort) ? outConsumerPort : 5001;
            var adminPort = !string.IsNullOrEmpty(_adminPort) && int.TryParse(_adminPort, out int outAdminPort) ? outAdminPort : 5002;

            var setting = new BrokerSetting(
                isMessageStoreMemoryMode: isMemoryMode,
                chunkFileStoreRootPath: fileStoreRootPath,
                chunkFlushInterval: flushInterval,
                chunkCacheMaxCount: chunkCacheMaxCount,
                chunkCacheMinCount: chunkCacheMinCount,
                messageChunkDataSize: chunkSize * 1024 * 1024,
                chunkWriteBuffer: chunkWriteBuffer * 1024,
                enableCache: enableCache,
                syncFlush: syncFlush,
                messageChunkLocalCacheSize: 30 * 10000,
                queueChunkLocalCacheSize: 10000)
            {
                NotifyWhenMessageArrived = notifyWhenMessageArrived,
                MessageWriteQueueThreshold = messageWriteQueueThreshold,
                DeleteMessageIgnoreUnConsumed = deleteMessageIgnoreUnConsumed
            };
            setting.NameServerList = new List<IPEndPoint> { new IPEndPoint(nameServerAddress, nameServerPort) };
            setting.BrokerInfo.BrokerName = brokerName;
            setting.BrokerInfo.GroupName = groupName;
            setting.BrokerInfo.ProducerAddress = new IPEndPoint(brokerAddress, producerPort).ToAddress();
            setting.BrokerInfo.ConsumerAddress = new IPEndPoint(brokerAddress, consumerPort).ToAddress();
            setting.BrokerInfo.AdminAddress = new IPEndPoint(brokerAddress, adminPort).ToAddress();
            BrokerController.Create(setting).Start();
            Console.ReadLine();
        }

        static void InitializeEQueue()
        {
            var configuration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .RegisterEQueueComponents()
                .UseDeleteMessageByCountStrategy(5)
                .BuildContainer();
        }
    }
}
