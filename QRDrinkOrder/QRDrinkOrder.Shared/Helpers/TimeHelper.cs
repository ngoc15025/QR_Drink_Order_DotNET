using System;

namespace QRDrinkOrder.Shared.Helpers
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                // IANA TimeZone ID (Linux/macOS/Render Cloud)
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    // Windows TimeZone ID (Local Dev Windows)
                    return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Fallback to fixed offset UTC+7
                    return TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "Indochina Time", "Indochina Time");
                }
            }
        }

        /// <summary>
        /// Lấy thời gian hiện tại chuẩn theo múi giờ Việt Nam (UTC+7).
        /// Thay thế cho DateTime.Now / DateTime.UtcNow để đảm bảo đồng bộ thời gian trên mọi môi trường (Render Linux / Local Windows).
        /// </summary>
        public static DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }
    }
}
