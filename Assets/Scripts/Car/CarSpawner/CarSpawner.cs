using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] cars;
    [Header("Random Spawn Interval")]
    [SerializeField]
    private float spawnFrom;
    [SerializeField]
    private float spawnTo;

    private float spawnInterval;

    private GameObject currCar;
    private CarMovement currCarMovement;
    private float randomSpeed;
    private int randomIndex;

    [Space]
    [SerializeField]
    private SpeedUnitEnum speedUnit;
    [Header("Speed Range")]
    [SerializeField]
    private float speedFrom;
    [SerializeField]
    private float speedTo;

    void Awake()
    {
        spawnInterval = Random.Range(spawnFrom, spawnTo);
        StartCoroutine(Spawner());
    }

    IEnumerator Spawner()
    {
        int i = 0;
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            spawnInterval = Random.Range(spawnFrom, spawnTo);
            //Debug.Log("текущий интервал спавна: " + spawnInterval);
            randomSpeed = Random.Range(speedFrom, speedTo);
            randomIndex = Random.Range(0, cars.Length);

            currCar = Instantiate(cars[randomIndex]);
            currCar.name = cars[randomIndex].name + " " + i;

            currCarMovement = currCar.GetComponent<CarMovement>();
            currCarMovement.SetMoveSpeed(randomSpeed, speedUnit);
            currCarMovement.MoveSpeedInSpeedUnit = randomSpeed;
            i++;
        }
    }
}
