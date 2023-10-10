using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{
    /// <summary>
    /// 指令执行结果
    /// </summary>
    public class ExecutionResult
    {
        public ExecutionResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
        /// <summary>
        /// 指令是否执行成功，如果接收到了应答，且应答内容复合要求，则为true，否则为false
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 错误信息，仅在IsSuccess=false时有效
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 接收到的应答值，仅在IsSuccess=true时有效
        /// </summary>
        public CanFrameBase Response { get; internal set; }

        /// <summary>
        /// 创建执行成功的对象
        /// </summary>
        /// <param name="response">应答的对象</param>
        /// <returns></returns>
        public static ExecutionResult Success(CanFrameBase response)
        {
            var result = new ExecutionResult(true)
            {
                Response = response,
            };
            return result;
        }
        /// <summary>
        /// 创建执行失败的对象
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <returns></returns>
        public static ExecutionResult Failed(string errorMessage = null)
        {
            var result = new ExecutionResult(false)
            {
                ErrorMessage = errorMessage,
            };
            return result;
        }
    }
}
