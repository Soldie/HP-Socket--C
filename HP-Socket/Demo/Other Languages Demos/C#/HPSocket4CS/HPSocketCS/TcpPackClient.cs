using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPSocketCS.SDK;

namespace HPSocketCS
{
    public class TcpPackClient : TcpClient
    {
        public TcpPackClient()
        {
            CreateListener();
        }

        /// <summary>
        /// 创建socket监听&服务组件
        /// </summary>
        /// <returns></returns>
        protected override bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pClient != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpClientListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pClient = HPSocketSdk.Create_HP_TcpPackClient (pListener);
            if (pClient == IntPtr.Zero)
            {
                return false;
            }
            
            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 读取或设置数据包最大长度
        /// 有效数据包最大长度不能超过 524287/0x7FFFF 字节，默认：262144/0x40000
        /// </summary>
        public uint MaxPackSize
        {
            get
            {
                return HPSocketSdk.HP_TcpPackClient_GetMaxPackSize(pClient);
            }
            set
            {
                HPSocketSdk.HP_TcpPackClient_SetMaxPackSize(pClient, value);
            }
        }

        /// <summary>
        /// 读取或设置包头标识
        /// 有效包头标识取值范围 0 ~ 8191/0x1FFF，当包头标识为 0 时不校验包头，默认：0
        /// </summary>
        public ushort PackHeaderFlag
        {
            get
            {
                return HPSocketSdk.HP_TcpPackClient_GetPackHeaderFlag(pClient);
            }
            set
            {
                HPSocketSdk.HP_TcpPackClient_SetPackHeaderFlag(pClient, value);
            }
        }

        
    }
}
