using UnityEngine;

namespace CatEscape.Util
{
    public class Singleton<T> where T : MonoBehaviour
    {
        private static T SingletonInstance { get; set; }

        protected static T Instance
        {
            get
            {
                if (SingletonInstance == null)
                {
                    var obj = Object.FindObjectOfType<T>();
                    if (obj != null)
                    {
                        SingletonInstance = obj;
                    }
                }

                return SingletonInstance;
            }
        }
    }
}
