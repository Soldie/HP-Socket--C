using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPSocketCS.SDK;

namespace HPSocketCS
{
    public class TcpPackServer : TcpServer
    {
        public TcpPackServer()
        {
            CreateListener();
        }

        /// <summary>
        /// 创建socket监听&服务组件
        /// </summary>
        /// <returns></returns>
        protected override bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pServer != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpServerListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pServer = HPSocketSdk.Create_HP_TcpPackServer(pListener);
            if (pServer == IntPtr.Zero)
            {
                return false;
            }
            
            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 终止服务并释放资源
        /// </summary>
        public override void Destroy()
        {
            Stop();

            if (pServer != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpPackServer(pServer);
                pServer = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpServerListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }

        /// <summary>
        /// 读取或设置数据包最大长度
        /// 有效数据包最大长度不能超过 524287/0x7FFFF 字节，默认：262144/0x40000
        /// </summary>
        public uint MaxPackSize
        {
            get
            {
                return HPSocketSdk.HP_TcpPackServer_GetMaxPackSize(pServer);
            }
            set
            {
                HPSocketSdk.HP_TcpPackServer_SetMaxPackSize(pServer, value );
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
                return HPSocketSdk.HP_TcpPackServer_GetPackHeaderFlag(pServer);
            }
            set
            {
                HPSocketSdk.HP_TcpPackServer_SetPackHeaderFlag(pServer, value);
            }
        }

        
    }
}
