namespace LunktrionShared.Models.Utils
{
    public static class HubCommands
    {
        // ПОДКЛЮЧЕНИЕ / ОТКЛЮЧЕНИЕ

        /// <summary>
        /// Принимает и отправляет <see cref="Entities.DeviceIdentity"/>
        /// </summary>
        public const string DeviceOnline = "DeviceOnline";

        /// <summary>
        /// Отправляет <see cref="string"/> Id устройства
        /// </summary>
        public const string DeviceOffline = "DeviceOffline";

        // УВЕДОМЛЕНИЯ

        /// <summary>
        /// Отправляет <see cref="string"/> Message
        /// </summary>
        public const string Notification = "Notification";

        /// <summary>
        /// Отправляет <see cref="string"/> Message
        /// </summary>
        public const string Error = "Error";

        // ИНФОРМАЦИЯ О УСТРОЙСТВЕ

        /// <summary>
        /// Принимает и отправляет <see cref="Requests.DeviceInfoRequest"/>
        /// </summary>
        public const string CollectAndSendInfo = "CollectAndSendInfo";

        /// <summary>
        /// Принимает и отправляет <see cref="Responses.DeviceInfoResponse"/>
        /// </summary>
        public const string DeviceInfoReceived = "DeviceInfoReceived";

        // ВЫПОЛНЕНИЕ КОМАНД

        /// <summary>
        /// Принимает и отправляет <see cref="Requests.DeviceExecuteCommandRequest"/>
        /// </summary>
        public const string ExecuteCommand = "ExecuteCommand";

        /// <summary>
        /// Принимает и отправляет <see cref="Responses.DeviceExecuteCommandResponse"/>
        /// </summary>
        public const string CommandResult = "CommandResult";
    }
}
