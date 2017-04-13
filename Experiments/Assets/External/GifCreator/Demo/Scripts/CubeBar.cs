using UnityEngine;

namespace PygmyMonkey.GifCreator
{
	public class CubeBar : MonoBehaviour
	{
		private int randomFrameColorChange;
		private Vector3 randomLocalScale;
		private float randomSpeed;

		void Awake()
		{
			udpateRandoms();
		}

		void Update()
		{
			if (Time.frameCount % randomFrameColorChange == 0)
			{
				udpateRandoms();
			}

			this.transform.localScale = Vector3.Lerp(this.transform.localScale, randomLocalScale, randomSpeed * Time.deltaTime);
		}

		private void udpateRandoms()
		{
			randomFrameColorChange = Random.Range(50, 300);
			randomLocalScale = new Vector3(0.5f, Random.Range(0.2f, 5.5f), 0.5f);
			randomSpeed = Random.Range(0.5f, 2.0f);

			this.GetComponent<Renderer>().material.color = getRandomColor();
		}

		private Color getRandomColor()
		{
			return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
		}
	}
}