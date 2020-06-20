//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine.U2D;

namespace UnityEngine.UI
{
	/// <summary>
	/// 扩展的精灵
	/// </summary>
	[RequireComponent(typeof(RectTransform), typeof(Image))]
	public class UISprite : MonoBehaviour
	{
		public SpriteAtlas Atlas;

		/// <summary>
		/// 关联的图片
		/// </summary>
		public Image Image
		{
			get
			{
				return this.transform.GetComponent<Image>();
			}
		}

		/// <summary>
		/// 精灵名称
		/// </summary>
		public string SpriteName
		{
			get
			{
				Image image = this.transform.GetComponent<Image>();
				if (image.sprite == null)
					return string.Empty;
				else
					return image.sprite.name.Replace("(Clone)", "");
			}

			set
			{
				Image image = this.transform.GetComponent<Image>();

				// 精灵置空
				if (string.IsNullOrEmpty(value))
				{
					image.sprite = null;
					return;
				}

				// 精灵设置
				if(Atlas != null)
				{
					image.sprite = Atlas.GetSprite(value);
				}
			}
		}

		/// <summary>
		/// 注意：Awake方法只有在GameObject激活的时候才会起效
		/// </summary>
		private void Awake()
		{
			// 运行时刷新精灵
			if(Application.isPlaying)
			{
				if (Atlas == null)
					return;

				string spriteName = SpriteName;
				SpriteName = spriteName;
			}
		}
	}
}