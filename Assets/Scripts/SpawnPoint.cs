using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public Transform PointTransform { get; private set; }
	private float _distanceToClosestEnemy;
	private float _distanceToClosestFriend;

	public float SpawnTimer { get; private set; }

	public float DistanceToClosestEnemy
	{
		get
		{
			return _distanceToClosestEnemy;
		}

		set
		{
			_distanceToClosestEnemy = value;
		}
	}

	public float DistanceToClosestFriend
	{
		get
		{
			return _distanceToClosestFriend;
		}

		set
		{
			_distanceToClosestFriend = value;
		}
	}

    // Script örneği yüklendiğinde awake çalışır.
    // Eğer objenin x ya da z eksenindeki açısı 0 değilse error mesajı verir.
    
    void Awake()
	{
		PointTransform = transform;
#if UNITY_EDITOR
        if (transform.rotation.eulerAngles.x != 0 || transform.rotation.eulerAngles.z != 0)
        {
            Debug.LogError("This spawn point has some rotation issues : " + name + " rotation : " + transform.rotation.eulerAngles);
        }
#endif
    }

    // SpawnTimer'a 2 değerini atar.

    public void StartTimer()
	{
		SpawnTimer = 2f;
	}

    // SpawnTimer değeri 0 dan yüksekse SpawnTimer Time.deltaTime kadar azalır.

	private void Update()
	{
		if (SpawnTimer > 0)
		{
			SpawnTimer -= Time.deltaTime;
		}
	}
}

