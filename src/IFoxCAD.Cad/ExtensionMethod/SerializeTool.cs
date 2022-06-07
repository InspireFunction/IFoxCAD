using Newtonsoft.Json;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 序列化
    /// </summary>
    public static class SerializeTool
    {
        /// <summary>
        /// 序列化为Json
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>字符串</returns>
        /// 
        public static string SerializeToJson<T>(this T obj)
        {
            try
            {
                var str = JsonConvert.SerializeObject(obj);
                return str;
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return String.Empty;
            }
        }
        /// <summary>
        /// 序列化为Json
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="filename">文件路径名</param>
        /// <returns>成功返回True</returns>
        public static bool SerializeToJson<T>(this T obj, string filename)
        {
            try
            {
                var str = JsonConvert.SerializeObject(obj);
                File.WriteAllText(filename, str);
                return true;
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="filename">文件路径名</param>
        /// <returns>类</returns>
        public static T? DeserializeFromJsonFile<T>(this string filename)
        {
            try
            {
                var str = File.ReadAllText(filename);
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return default;
            }
        }
        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="jsonString">JSON字符串</param>
        /// <returns>类</returns>
        public static T? DeserializeFromJson<T>(this string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return default;
            }
        }
        /// <summary>
        /// 序列化为二进制数组
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>二进制数组</returns>
        public static byte[]? SerializeToByteArray<T>(this T obj)
        {
            try
            {
                var str = JsonConvert.SerializeObject(obj);
                return System.Text.Encoding.UTF8.GetBytes(str);
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return default;
            }
        }
        /// <summary>
        /// 序列化为二进制数组
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="filename">文件路径名</param>
        /// <returns>成功返回True</returns>
        public static bool SerializeToByteArray<T>(this T obj, string filename)
        {
            try
            {
                var str = JsonConvert.SerializeObject(obj);
                var byteArray = System.Text.Encoding.UTF8.GetBytes(str);
                File.WriteAllBytes(filename, byteArray);
                return true;
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 二进制数组反序列化
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="byteArray">二进制数组</param>
        /// <returns>类</returns>
        public static T? DeserializeTo<T>(this byte[] byteArray)
        {
            try
            {
                var str = System.Text.Encoding.UTF8.GetString(byteArray);
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return default;
            }
        }
        /// <summary>
        /// 二进制数组反序列化
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="filename">二进制数组</param>
        /// <returns>类</returns>
        public static T? DeserializeFromByteArrayFile<T>(this string filename)
        {
            try
            {
                var byteArray = File.ReadAllBytes(filename);
                var str = System.Text.Encoding.UTF8.GetString(byteArray);
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (System.Exception ex)
            {
                Env.Editor.WriteMessage(ex.Message);
                return default;
            }
        }
    }
}
