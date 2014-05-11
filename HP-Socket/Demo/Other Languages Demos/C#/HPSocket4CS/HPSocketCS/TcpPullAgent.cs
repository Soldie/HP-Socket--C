using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPSocketCS.SDK;

namespace HPSocketCS
{
    public class TcpPullAgent : TcpAgent
    {
        protected HPSocketSdk.OnPullReceive OnPullReceiveCallback;

        public TcpPullAgent()
        {
            CreateListener();
        }

        ~TcpPullAgent()
        {
            Destroy();
        }

        /// <summary>
        /// 创建socket监听&服务组件
        /// </summary>
        /// <param name="isUseDefaultCallback">是否使用tcppullAgent类默认回调函数</param>
        /// <returns></returns>
        protected override bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pAgent != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpPullAgentListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pAgent = HPSocketSdk.Create_HP_TcpPullAgent(pListener);
            if (pAgent == IntPtr.Zero)
            {
                return false;
            }

            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 抓取数据
        /// 用户通过该方法从 Socket 组件中抓取数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="pBuffer"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public FetchResult Fetch(uint connId, IntPtr pBuffer, int size)
        {
            return HPSocketSdk.HP_TcpPullAgent_Fetch(pAgent, connId, pBuffer, size);
        }

        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="prepareConnect"></param>
        /// <param name="connect"></param>
        /// <param name="send"></param>
        /// <param name="recv"></param>
        /// <param name="close"></param>
        /// <param name="error"></param>
        /// <param name="agentShutdown"></param>
        public virtual void SetCallback(HPSocketSdk.OnPrepareConnect prepareConnect, HPSocketSdk.OnConnect connect,
            HPSocketSdk.OnSend send, HPSocketSdk.OnPullReceive recv, HPSocketSdk.OnClose close,
            HPSocketSdk.OnError error, HPSocketSdk.OnAgentShutdown agentShutdown)
        {

            // 设置 Socket 监听器回调函数
            OnPullReceiveCallback = new HPSocketSdk.OnPullReceive(recv);

            // 设置 Socket 监听器回调函数
            HPSocketSdk.HP_Set_FN_Server_OnPullReceive(pListener, OnPullReceiveCallback);

            base.SetCallback(OnPrepareConnect, OnConnect, OnSend, OnReceive, OnClose, OnError, OnAgentShutdown); ;
        }


        public virtual void SetOnPullReceiveCallback(HPSocketSdk.OnPullReceive recv)
        {
            OnPullReceiveCallback = new HPSocketSdk.OnPullReceive(recv);
            HPSocketSdk.HP_Set_FN_Server_OnPullReceive(pListener, OnPullReceiveCallback);
        }

        /// <summary>
        /// 释放TcpPullAgent和TcpPullAgentListener
        /// </summary>
        public override void Destroy()
        {
            Stop();

            if (pAgent != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpPullAgent(pAgent);
                pAgent = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpPullAgentListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }

        /// <summary>
        /// 数据到达
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual HandleResult OnPullReceive(uint dwConnID, int iLength)
        {
            return HandleResult.Ok;
        }
    }
}
