using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum PlayerTeam
{
    None,
    BlueTeam,

    RedTeam
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _sharedSpawnPoints = new List<SpawnPoint>();
    System.Random _random = new System.Random();
	float _closestDistance;
    [Tooltip("This will be used to calculate the second filter where algorithm looks for closest friends, if the friends are away from this value, they will be ignored")]
    [SerializeField] private float _maxDistanceToClosestFriend = 30;
    [Tooltip("This will be used to calculate the first filter where algorithm looks for enemies that are far away from this value. Only enemies which are away from this value will be calculated.")]
    [SerializeField] private float _minDistanceToClosestEnemy = 10;
    [Tooltip("This value is to prevent friendly player spawning on top of eachothers. If a player is within the range of this value to a spawn point, that spawn point will be ignored")]
    [SerializeField] private float _minMemberDistance = 2;


    public DummyPlayer PlayerToBeSpawned;
    public DummyPlayer[] DummyPlayers;

    // spawn noktasındaki tüm nesnelerin listesini _sharedSpawnPoints listesiyle birleştirir (_sharedSpawnPoints'e ekler).
    // Tüm aktiv dummyplayer listesini verir.

    private void Awake()
    {
		_sharedSpawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());

		DummyPlayers = FindObjectsOfType<DummyPlayer>();
    }

    #region SPAWN ALGORITHM

    // _sharedSpawnPoints deki elementlerin sayı kadar yeni spawn noktasına ekler.
    // team için spawn nokataları arasındaki mesafeyi hesaplar.
    // Düşmana olan uzaklığa göre spawn noktası seçer.
    // Eğer spawn noktalarının sayı küçüktür eşitdir sıfırdırsa, dostlara olan uzaklığa göre spawn noktası seçer. 
    // spawn noktalarının sayı küçüktür eşitdir birse, spawn noktası spawn noktasını dizininin ilk elementine eşit olur, değilse rastgele bir elementine eşit oluyor.
    // StartTime methodu çalışır.

    public SpawnPoint GetSharedSpawnPoint(PlayerTeam team) //paylaşımlı spawn noktasını seçmek için method
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>(_sharedSpawnPoints.Count);
        CalculateDistancesForSpawnPoints(team);
        GetSpawnPointsByDistanceSpawning(team, ref spawnPoints);
        if (spawnPoints.Count <= 0)
        {
            GetSpawnPointsBySquadSpawning(team, ref spawnPoints);
        }
        SpawnPoint spawnPoint = spawnPoints.Count <= 1 ? spawnPoints[0] : spawnPoints[_random.Next(0, (int)((float)spawnPoints.Count * .5f))];
        spawnPoint.StartTimer();
        return spawnPoint;
    }

    // for döngüsü oluştutarak, paylaşılan spawn noktasının sayı kadar, spawn noktalarının düşmanlara olan uzaklığı minimum düşman uzaklığından büyük olduğu sürece uygun spawn noktaları ekler.

    private void GetSpawnPointsByDistanceSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints) // Düşmana olan uzaklığa göre spawn noktasını seçmek için method
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();
        _sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
        {
            if (a.DistanceToClosestEnemy == b.DistanceToClosestEnemy)
            {
                return 0;
            }
            if (a.DistanceToClosestEnemy > b.DistanceToClosestEnemy)
            {
                return 1;
            }
            return -1;
        });

        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            if (!(_sharedSpawnPoints[i].DistanceToClosestEnemy < _minDistanceToClosestEnemy) && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && !(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
            {
                suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
                Debug.Log(_sharedSpawnPoints[i] + " seçildi çünkü düşmana olan uzaklık minimum düşman uzaklığından küçük değil, düşmana olan uzaklık minimum uzaklığından küçük değil, dosta olan uzaklık minimum uzaklığından küçük değil, bu spawn noktasında 2 saniye önce canlanma olmadı ");
            }

            // Spawn noktalarının seçilib seçilmeme nedenleri eklendi.

           else
            {
                if (_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minDistanceToClosestEnemy)
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü düşmana olan uzaklık minimum düşman uzaklığından küçük.");
                }

                 if (_sharedSpawnPoints[i].DistanceToClosestEnemy < _minMemberDistance)
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü düşmana olan uzaklık minimum uzaklığından küçük.");
                }

                 if (_sharedSpawnPoints[i].DistanceToClosestFriend < _minMemberDistance)
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü dosta olan uzaklık minimum uzaklığından küçük.");
                }
                 if (!(_sharedSpawnPoints[i].SpawnTimer < 0))
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü bu spawn noktasında 2 saniye önce canlanma oldu.");
                }

            }
        }
            if (suitableSpawnPoints.Count <= 0)
             {
                 suitableSpawnPoints.Add(_sharedSpawnPoints[0]);
          }


    }

    // for döngüsü oluştutarak, paylaşılan spawn noktasının sayı kadar, spawn noktalarının dostlara olan uzaklığı maksimim dost uzaklığından küçük olduğu sürece uygun spawn noktaları ekler.

    private void GetSpawnPointsBySquadSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints) // Dostlara olan uzaklığa göre spawn noktasını seçmek için method
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();
        _sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
        {
            if (a.DistanceToClosestFriend == b.DistanceToClosestFriend)
            {
                return 0;
            }
            if (a.DistanceToClosestFriend > b.DistanceToClosestFriend)
            {
                return 1;
            }
            return -1;
        });


        for (int i = 0; i < _sharedSpawnPoints.Count && _sharedSpawnPoints[i].DistanceToClosestFriend <= _maxDistanceToClosestFriend; i++)
        {
            if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
            {
                suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
                Debug.Log(_sharedSpawnPoints[i] + " seçildi çünkü dosta olan uzaklık minimum uzaklığından küçük değil, düşmana olan uzaklık minimum uzaklıkran küçük değil, bu spawn noktasında 2 saniye önce canlanma olmadı. ");
            }
            else
            {
                if (_sharedSpawnPoints[i].DistanceToClosestFriend < _minMemberDistance)
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü dosta olan uzaklık minimum uzaklığından küçük.");
                }

                if (_sharedSpawnPoints[i].DistanceToClosestEnemy < _minMemberDistance)
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü düşmana olan uzaklık minimum uzaklığından küçük.");
                }

                if (!(_sharedSpawnPoints[i].SpawnTimer < 0))
                {
                    Debug.Log(_sharedSpawnPoints[i] + " seçilmedi çünkü bu spawn noktasında 2 saniye önce canlanma oldu.");
                }
            }
        }
        if (suitableSpawnPoints.Count <= 0)
        {
            suitableSpawnPoints.Add(_sharedSpawnPoints[0]);
        }

    }

    // Yakindaki düşmanara ve dostlara olan mesafeyi hesaplar.


    private void CalculateDistancesForSpawnPoints(PlayerTeam playerTeam) // Spawn noktaları için mesafeyi hesaplayan method.
    {
        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            _sharedSpawnPoints[i].DistanceToClosestFriend = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam);
            _sharedSpawnPoints[i].DistanceToClosestEnemy = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam == PlayerTeam.BlueTeam ? PlayerTeam.RedTeam : playerTeam == PlayerTeam.RedTeam ? PlayerTeam.BlueTeam : PlayerTeam.None);
        }
    }

    // Oyuncu sayısı kadar ve oyuncular ölmediğse, takımı varsa, aktivlerse oyunculardan spawn noktalarına kadar olan mesafeyi hesaplar.

    private float GetDistanceToClosestMember(Vector3 position, PlayerTeam playerTeam) // Üyelere olan mesafeyi hesaplayan method.
    {
        _closestDistance = 46; // _closestDistance'a değer atanmamıştı, o yüzden hatalı çalışıyordu.
        foreach (var player in DummyPlayers)
        {
            if (!player.Disabled && player.PlayerTeamValue != PlayerTeam.None && player.PlayerTeamValue == playerTeam && !player.IsDead())
            {
                float playerDistanceToSpawnPoint = Vector3.Distance(position, player.Transform.position);
                if (playerDistanceToSpawnPoint < _closestDistance )
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                }
            }
        }
        return _closestDistance;
    }

    #endregion
	/// <summary>
	/// Test için paylaşımlı spawn noktalarından en uygun olanını seçer.
	/// Test oyuncusunun pozisyonunu seçilen spawn noktasına atar.
	/// </summary>
    public void TestGetSpawnPoint()
    {
    	SpawnPoint spawnPoint = GetSharedSpawnPoint(PlayerToBeSpawned.PlayerTeamValue);
    	PlayerToBeSpawned.Transform.position = spawnPoint.PointTransform.position;
    }

}