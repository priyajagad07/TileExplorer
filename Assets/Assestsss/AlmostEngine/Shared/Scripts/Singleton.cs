using UnityEngine;

namespace AlmostEngine
{

		/// <summary>
		/// Use the singleton to manage persistent data between scenes.
		/// </summary>
		public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
		{
			public bool DontDestroyObjectOnLoad = false;
			private static T _instance;
			public static T Instance
			{
				get
				{
					return _instance;
				}
			}

			public virtual void Awake()
			{
				if (_instance == null)
				{
					_instance = this as T;
					if (DontDestroyObjectOnLoad)
					{
						DontDestroyOnLoad(this.gameObject);
					}
				}
				else
				{
					Destroy(gameObject);
				}
			}
		}

}