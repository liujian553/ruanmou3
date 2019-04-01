using System;
using System.Configuration;

namespace RM.Common
{
    [Injectable(typeof(Configurations), InjectType.SingleInstance)]
    public interface IConfigurations
    {

        string DBConnectionStringWrite
        {
            get;
        }
        string DBConnectionStringRead
        {
            get;
        }
        string CacheConnectionString
        {
            get;
        }

        /// <summary>
        /// 获取加密秘钥
        /// </summary>
        /// <value>
        /// The encry key.
        /// </value>
        string EncryKey
        {
            get;
        }

        //是否是专网环境
        string ZWPortal
        {
            get;
        }
        
        /// <summary>
        /// 获取加密向量
        /// </summary>
        /// <value>
        /// The encry iv.
        /// </value>
        string EncryIV
        {
            get;
        }

        /// <summary>
        /// kafka地址
        /// </summary>
        string KafkaServer
        {
            get;
        }
        /// <summary>
        /// 工作流服务端连接字符串
        /// </summary>
        string WorkflowApiConnectionString
        {
            get;
        }
        /// <summary>
        /// 本系统在工作流服务器那边的ID
        /// </summary>
        string WorkflowSystemID
        {
            get;
        }
        /// <summary>
        /// 工作流回调地址
        /// </summary>
        string WorkflowCallBackIP
        {
            get;
        }
        /// <summary>
        /// 是否启用MVC权限验证
        /// </summary>
        bool IgnoreMvcAuthorize
        {
            get;
        }
        /// <summary>
        /// 是否启用API权限验证
        /// </summary>
        bool IgnoreApiAuthorize
        {
            get;
        }
        /// <summary>
        /// 是否忽略密码规则
        /// </summary>
        bool IgnorePassword
        {
            get;
        }
        /// <summary>
        /// redis中存储用户信息的过期时间
        /// </summary>
        int RedisSessionExpireTime
        {
            get;
        }

        /// <summary>
        /// ES搜索日志地址
        /// </summary>
        string ElasticSearchLogUrl
        {
            get;
        }
        /// <summary>
        /// 是否是超级管理员
        /// </summary>
        string SuperAdministratorName
        {
            get;
        }
        /// <summary>
        /// 开启账号多登陆限制
        /// </summary>
        bool OpenAccountLimit
        {
            get;
        }
        /// <summary>
        /// 获取用户权限时Redis失败重试次数
        /// </summary>
        int RedisRetryTime
        {
            get;
        }
        /// <summary>
        /// 获取用户权限时Redis失败重试之间的休眠时间
        /// </summary>
        int RedisFailSleepTimeSpan
        {
            get;
        }
        /// <summary>
        /// 任务生成程序时间间隔
        /// </summary>
        int TaskCreatingServiceInterval
        {
            get;
        }
        /// <summary>
        /// 任务处理程序时间间隔
        /// </summary>
        int TaskProcessingServiceInterval
        {
            get;
        }
        /// <summary>
        /// 任务调度程序处理的主题范围
        /// </summary>
        string ProcessingTopics
        {
            get;
        }
        /// <summary>
        /// Windows服务默认用户ID
        /// </summary>
        Guid WinServiceUserID
        {
            get;
        }

        /// <summary>
        /// signalr 服务器地址
        /// </summary>
        /// <value>
        /// The signal r server.
        /// </value>
        string SignalRServer
        {
            get;
        }

        string WinServiceName
        {
            get;
        }

        string CAUrl
        {
            get;
        }
       

        string ResourceCollectionPath
        {
            get;
        }
        string BannedFileExtension {
            get;
        }
    }

    public class Configurations : IConfigurations
    {
        public string DBConnectionStringWrite
        {
            get
            {
                return ConfigurationManager.AppSettings["DBConnectionStringWrite"];
            }
        }
        public string DBConnectionStringRead
        {
            get
            {
                return ConfigurationManager.AppSettings["DBConnectionStringRead"];
            }
        }

        public string KafkaServer
        {
            get
            {
                return ConfigurationManager.AppSettings["KafkaServer"];
            }
        }
        /// <summary>
        /// Redis缓存服务器连接字符串
        /// </summary>
        public string CacheConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["CacheConnectionString"];
            }
        }
        /// <summary>
        /// 工作流服务器webapi的连接字符串
        /// </summary>
        public string WorkflowApiConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["WorkflowAPIUrl"];
            }
        }
        /// <summary>
        /// 本系统在工作流服务器端标记的系统ID
        /// </summary>
        public string WorkflowSystemID
        {
            get
            {
                return ConfigurationManager.AppSettings["WorkflowSystemID"];
            }
        }
        /// <summary>
        /// 是否忽略MVC权限
        /// </summary>
        public bool IgnoreMvcAuthorize
        {
            get
            {
                bool result = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["IgnoreMvcAuthorize"], out result);
                return result;
            }
        }
        /// <summary>
        /// 是否忽略API权限验证
        /// </summary>
        public bool IgnoreApiAuthorize
        {
            get
            {
                bool result = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["IgnoreApiAuthorize"], out result);
                return result;
            }
        }
        /// <summary>
        /// 是否不需要输入密码
        /// </summary>
        public bool IgnorePassword
        {
            get
            {
                bool result = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["IgnorePassword"], out result);
                return result;
            }
        }
        /// <summary>
        /// 在Redis中设置的session过期时间
        /// </summary>
        public int RedisSessionExpireTime
        {
            get
            {
                int result = 20;
                int.TryParse(ConfigurationManager.AppSettings["RedisSessionExpireTime"], out result);
                return result;
            }
        }


        public string ElasticSearchLogUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ElasticSearchLogUrl"];
            }
        }
        /// <summary>
        /// 系统超级管理员
        /// </summary>
        public string SuperAdministratorName
        {
            get
            {
                return ConfigurationManager.AppSettings["SuperAdministratorName"];
            }
        }
        /// <summary>
        /// 开启用户多登录限制
        /// </summary>
        public bool OpenAccountLimit
        {
            get
            {
                bool result = true;
                Boolean.TryParse(ConfigurationManager.AppSettings["OpenAccountLimit"], out result);
                return result;
            }
        }
        /// <summary>
        /// redis的失败重试之间的时间
        /// </summary>
        public int RedisRetryTime
        {
            get
            {
                int result = 2;
                Int32.TryParse(ConfigurationManager.AppSettings["RedisRetryTime"], out result);
                return result;
            }
        }

        public int RedisFailSleepTimeSpan
        {
            get
            {
                int result = 500;
                Int32.TryParse(ConfigurationManager.AppSettings["RedisFailSleepTimeSpan"], out result);
                return result;
            }
        }
        /// <summary>
        /// 任务生成程序时间间隔
        /// </summary>
        public int TaskCreatingServiceInterval
        {
            get
            {
                int result = 1000;
                int.TryParse(ConfigurationManager.AppSettings["TaskCreatingServiceInterval"], out result);
                return result;
            }
        }
        /// <summary>
        /// 任务处理程序时间间隔
        /// </summary>
        public int TaskProcessingServiceInterval
        {
            get
            {
                int result = 1000;
                int.TryParse(ConfigurationManager.AppSettings["TaskProcessingServiceInterval"], out result);
                return result;
            }
        }
        /// <summary>
        /// 任务调度程序处理的主题范围
        /// </summary>
        public string ProcessingTopics
        {
            get
            {
                return ConfigurationManager.AppSettings["ProcessingTopics"];
            }
        }
        /// <summary>
        /// Windows服务默认用户ID
        /// </summary>
        public Guid WinServiceUserID
        {
            get
            {
                return Guid.Parse(ConfigurationManager.AppSettings["WinServiceUserID"]);
            }
        }

        public string WorkflowCallBackIP
        {
            get
            {
                return ConfigurationManager.AppSettings["WorkflowCallBackIP"];
            }
        }

        public string SignalRServer
        {
            get
            {
                return ConfigurationManager.AppSettings["SignalRServer"];
            }
        }

        public string WinServiceName
        {
            get
            {
                return ConfigurationManager.AppSettings["WinService"];
            }
        }
        /// <summary>
        /// 获取加密秘钥
        /// </summary>
        /// <value>
        /// The encry key.
        /// </value>
        public string EncryKey
        {
            get
            {
                return ConfigurationManager.AppSettings["EncryKey"];
            }
        }

        /// <summary>
        /// 获取加密向量
        /// </summary>
        /// <value>
        /// The encry iv.
        /// </value>
        public string EncryIV
        {
            get
            {
                return ConfigurationManager.AppSettings["EncryIV"];
            }
        }

        public string ZWPortal
        {
            get
            {
                return ConfigurationManager.AppSettings["ZWPortal"];
            }

        }

        /// <summary>
        /// 证书服务器地址
        /// </summary>
        /// <value>
        /// The ca URL.
        /// </value>
        public string CAUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["CAUrl"];
            }
        }



        public string ResourceCollectionPath
        {
            get
            {
                return ConfigurationManager.AppSettings["ResourceCollectionPath"];
            }
        }

        public string BannedFileExtension 
        {
            get 
            {
                return ConfigurationManager.AppSettings["BannedFileExtension"];
            }
        }

        public string PrintQRCodePath
        {
            get
            {
                return ConfigurationManager.AppSettings["PrintQRCodePath"];
            }
        }
    }
}
