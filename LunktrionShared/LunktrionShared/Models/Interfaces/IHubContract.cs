using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;

namespace LunktrionShared.Models.Interfaces
{
    public interface IHubContract
    {
        // ОТПРАВКА СООБЩЕНИЙ КЛИЕНТАМ

        /// <summary>
        /// Регистрационный метод для устройств. Необходимо его вызвать для уведомления всех о новом устрйостве в системе
        /// </summary>
        /// <param name="device">Регистрируемый девайс, который подключается к системе</param>
        /// <returns></returns>
        Task RegisterDevice(RegisterDeviceReuest device);

        /// <summary>
        /// Метод для запроса краткой информации о устройстве (CPU, GPU, RAM и память)
        /// </summary>
        /// <param name="targetDeviceId">Id устройства, у которого запрашивается информация</param>
        /// <returns></returns>
        Task RequestDeviceInfo(DeviceInfoRequest request);

        /// <summary>
        /// Метод для выполнения команды на требуемом устройстве
        /// </summary>
        /// <param name="targetDeviceId">Id устройства, у которого нужно выполнить команду</param>
        /// <param name="command">Команда, которая должна выполнить на устройстве</param>
        /// <returns></returns>
        Task RequestDeviceCommand(DeviceExecuteCommandRequest request);

        // ПОЛУЧЕНИЕ ОТВЕТА ОТ КЛИЕНТА

        /// <summary>
        /// Метод возвращающий краткую информацию о устройстве
        /// </summary>
        /// <param name="response">Информация, которую собрало устройство</param>
        /// <returns></returns>
        Task ReceiveDeviceInfo(DeviceInfoResponse response);

        /// <summary>
        /// Метод возвращающий результат выполненной команды на устройстве
        /// </summary>
        /// <param name="response">Результат выполненой команды на устройстве</param>
        /// <returns></returns>
        Task ReceiveCommandResult(DeviceExecuteCommandResponse response);
    }
}
