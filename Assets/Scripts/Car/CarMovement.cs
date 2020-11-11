using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private const int OneKilometerInMeters = 1000;
    private const int OneHourInSeconds = 3600;
    private const int OneHundredPercents = 100;

    private const float MinSpeedForMoving = 2.777777777778f;
    private const float StopInfelicity = 0.2f;

    private const string RoadTag = "Road";
    private const string PoliceCarTag = "PoliceCar";

    /// <summary>
    /// parameter from 0 to 1, describing exactly where the car is on the current path
    /// </summary>
    private float distanceTravelled;

    /// <summary>
    /// move speed in m/s
    /// </summary>
    private float moveSpeed;

    /// <summary>
    /// variable for saving car's initial speed to increase car's speed to its initial speed
    /// </summary>
    private float initialSpeed;

    /// <summary>
    /// move speed in selected speed unit
    /// </summary>
    [HideInInspector]
    public float MoveSpeedInSpeedUnit { get; set; }

    /// <summary>
    /// time for which car should reduce speed
    /// </summary>
    private float stopTime;

    /// <summary>
    /// time for which car should increase speed
    /// </summary>
    private float startTime;

    /// <summary>
    /// variable for speed setting
    /// </summary>
    private SpeedAdjustmentEnum speedAdjustment;

    /// <summary>
    /// variable to check on which part of the police way the car is
    /// </summary>
    private PolicePathMovingEnum policePathMoving;

    /// <summary>
    /// the road on which the car will move
    /// </summary>
    public Road Road { get; set; }

    /// <summary>
    /// the path the car is currently on
    /// </summary>
    public Path CurrentPath { get; set; }

    /// <summary>
    /// whether the car stopped near a policeman
    /// </summary>
    public bool Stopped { get; set; }

    /// <summary>
    /// variable for checking whether the car did not leave policeman before
    /// </summary>
    private bool didNotLeavePolicemanBefore = true;

    /// <summary>
    /// dictionary for storage cars in the field of visibility
    /// </summary>
    private Dictionary<GameObject, CarMovement> detectedCars = new Dictionary<GameObject, CarMovement>();

    /// <summary>
    /// Sets road, initial position and start/stop time for car
    /// </summary>
    private void Start()
    {
        Road = GameObject.FindWithTag(RoadTag).GetComponent<Road>();
        CurrentPath = Road.commonPath;
        transform.position = CurrentPath.GetControlPoint(0);
        //stop time 2f
        SetStartAndStopTime(moveSpeed / 10f, moveSpeed / 3f);
    }

    /// <summary>
    /// Updates car's position and speed
    /// </summary>
    private void FixedUpdate()
    {
        Move();
        DetectedCarsChecking(detectedCars);
        SetMoveSpeed(speedAdjustment);
        //Debug.Log(gameObject.name + "=> скорость в м/с: " + moveSpeed);
    }

    /// <summary>
    /// sets the movement of the car and calls methods depending on which way the car is
    /// </summary>
    private void Move()
    {
        distanceTravelled += moveSpeed * Time.deltaTime / OneHundredPercents;
        transform.position = CurrentPath.GetPoint(distanceTravelled);
        transform.LookAt(transform.position + CurrentPath.GetDirection(distanceTravelled));

        if (CurrentPath.GetType().Equals(typeof(PolicePath)))
            ActionsOnPolicePath(CurrentPath as PolicePath);
        else
            ActionsOnCommonPath();
    }

    /// <summary>
    /// Setting MoveSpeed in selected speed unit
    /// </summary>
    /// <param name="value"></param>
    public void SetMoveSpeed(float value, SpeedUnitEnum speedUnit)
    {
        MoveSpeedInSpeedUnit = value;
        switch (speedUnit)
        {
            //from km/h to m/s
            case SpeedUnitEnum.KMH:
                moveSpeed = value * OneKilometerInMeters / OneHourInSeconds;
                break;
        }
        initialSpeed = moveSpeed;
    }

    /// <summary>
    /// Setting speed via SpeedAdjustmentEnum variable
    /// </summary>
    /// <param name="speedAdjustment"></param>
    private void SetMoveSpeed(SpeedAdjustmentEnum speedAdjustment)
    {
        if (speedAdjustment == SpeedAdjustmentEnum.Increase)
        {
            if (Stopped)
            {
                moveSpeed = Mathf.SmoothStep(moveSpeed, SpeedLimit.SpeedLimitValue, startTime * Time.deltaTime);
                //Debug.Log(gameObject.name + "=> набираю скорость до допустимой!");
            }
            else
            {
                moveSpeed = Mathf.SmoothStep(moveSpeed, initialSpeed, startTime * Time.deltaTime);
                //Debug.Log(gameObject.name + "=> набираю скорость до изначальной!");
            }


        }
        else if (speedAdjustment == SpeedAdjustmentEnum.Decrease)
        {
            moveSpeed = Mathf.SmoothStep(moveSpeed, MinSpeedForMoving, stopTime * Time.deltaTime);
            //Debug.Log(gameObject.name + "=> торможу!");
        }
        //else if (speedAdjustment == SpeedAdjustmentEnum.ExtremeDe)
        else if (speedAdjustment == SpeedAdjustmentEnum.Stop)
        {
            moveSpeed = 0f;
            //Debug.Log(gameObject.name + "=> стою!");
        }
    }

    /// <summary>
    /// Sets start and stop time in seconds
    /// </summary>
    /// <param name="startT"></param>
    /// <param name="stopT"></param>
    private void SetStartAndStopTime(float startT, float stopT)
    {
        startTime = startT;
        stopTime = stopT;
    }

    /// <summary>
    /// Sets actions on a police path
    /// </summary>
    private void ActionsOnPolicePath(PolicePath policePath)
    {
        //PolicePath policePath = (PolicePath)CurrentPath;
        PolicePathMovingStatements(policePath);
        switch (policePathMoving)
        {
            case PolicePathMovingEnum.SpeedReduction:
                speedAdjustment = SpeedAdjustmentEnum.Decrease;
                //Debug.Log(gameObject.name + "=> сбавляю скорость перед съездом на обочину");
                break;
            case PolicePathMovingEnum.ExitFromTheRoad:
                CheckStopPoint(policePath);
                //Debug.Log(gameObject.name + "=> еду по обочине к полицейскому");
                break;
            case PolicePathMovingEnum.ReturnToTheRoad:
                CheckRemovingCar();
                //Debug.Log(gameObject.name + "=> вернулся на дорогу после остановки");
                break;
        }
    }

    /// <summary>
    /// Changes police path statements (PolicePathMovingEnum)
    /// </summary>
    /// <param name="policePath"></param>
    private void PolicePathMovingStatements(PolicePath policePath)
    {
        if (!Stopped)
        {
            if (transform.position.z >= policePath.GetControlPoint(policePath.ChangePointIndex - 2).z)
            {
                if (transform.position.z >= policePath.GetControlPoint(policePath.ChangePointIndex).z)
                {
                    if (policePathMoving == PolicePathMovingEnum.Stop)
                        return;
                    else
                        policePathMoving = PolicePathMovingEnum.ExitFromTheRoad;
                }
                else
                {
                    policePathMoving = PolicePathMovingEnum.SpeedReduction;
                }
            }
        }
        else
        {
            if (transform.position.z > policePath.GetControlPoint(policePath.StopPointIndex + 3).z)
            {
                if (didNotLeavePolicemanBefore)
                {
                    policePath.Occupied = false;
                    policePathMoving = PolicePathMovingEnum.ReturnToTheRoad;
                    didNotLeavePolicemanBefore = false;
                }
            }
        }
    }

    /// <summary>
    /// Checks whether a car is on a stop point
    /// </summary>
    /// <param name="policePath"></param>
    private void CheckStopPoint(PolicePath policePath)
    {
        if (Vector3.Distance(transform.position, policePath.StopPoint) <= StopInfelicity)
        {
            speedAdjustment = SpeedAdjustmentEnum.Stop;
            policePathMoving = PolicePathMovingEnum.Stop;
            //Debug.Log(gameObject.name + ": стою у полицейского");
            StartCoroutine(StoppingCoroutine());
            StopCoroutine(StoppingCoroutine());
        }
    }

    /// <summary>
    /// Counts 3 seconds at a stop point after which the car can begin to move
    /// </summary>
    /// <returns></returns>
    private IEnumerator StoppingCoroutine()
    {
        //Debug.Log(gameObject.name + "=> Заход в коррутину!");
        while (policePathMoving != PolicePathMovingEnum.Stop)
            yield return null;
        yield return new WaitForSeconds(3);
        policePathMoving = PolicePathMovingEnum.Starting;
        speedAdjustment = SpeedAdjustmentEnum.Increase;
        Stopped = true;
    }

    /// <summary>
    /// Sets actions on a common path
    /// </summary>
    private void ActionsOnCommonPath()
    {
        CheckRemovingCar();
    }

    /// <summary>
    /// Checks if the car has traveled all the way and whether it can be removed
    /// </summary>
    private void CheckRemovingCar()
    {
        if (Vector3.Distance(transform.position, CurrentPath.GetControlPoint(CurrentPath.ControlPointCount - 1)) <= 0.05f)
            Destroy(gameObject);

    }

    /// <summary>
    /// Detects other machines and takes action in this regard
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == PoliceCarTag) return;
        GameObject detectedCar = other.gameObject.transform.parent.gameObject;
        CarMovement detectedCarMovement = detectedCar.GetComponent<CarMovement>();
        detectedCars.Add(detectedCar, detectedCarMovement);
        if (detectedCar.transform.position.z < transform.position.z)//личное пространство замеченной машины сзади
        {
            
            if (policePathMoving == PolicePathMovingEnum.Starting)
            {
                switch (detectedCarMovement.policePathMoving)
                {
                    case PolicePathMovingEnum.None:
                        speedAdjustment = SpeedAdjustmentEnum.Stop;
                        break;
                    case PolicePathMovingEnum.ReturnToTheRoad:
                        speedAdjustment = SpeedAdjustmentEnum.Stop;
                        break;
                }
            }
            //Debug.Log(gameObject.name + "=>" + detectedCar.name + " вижу личное пространство машины сзади: " + detectedCar.name +
            //    "\nМоё состояние на полицейском пути: " + policePathMoving +
            //    "\nМоя настройка скорости после обнаружения: " + speedAdjustment);
        }
        else if (detectedCar.transform.position.z > transform.position.z) //личное пространство замеченной машины спереди
        {
            switch (policePathMoving)
            {
                case PolicePathMovingEnum.None:
                    switch (detectedCarMovement.policePathMoving)
                    {
                        case PolicePathMovingEnum.None:
                            speedAdjustment = SpeedAdjustmentEnum.Decrease;
                            break;
                        case PolicePathMovingEnum.SpeedReduction:
                            speedAdjustment = SpeedAdjustmentEnum.Decrease;
                            break;
                        case PolicePathMovingEnum.ReturnToTheRoad:
                            speedAdjustment = SpeedAdjustmentEnum.Decrease;
                            break;
                    }
                    break;
                case PolicePathMovingEnum.Starting:
                    switch (detectedCarMovement.policePathMoving)
                    {
                        case PolicePathMovingEnum.None:
                            speedAdjustment = SpeedAdjustmentEnum.Stop;
                            break;
                        case PolicePathMovingEnum.SpeedReduction:
                            speedAdjustment = SpeedAdjustmentEnum.Stop;
                            break;
                        case PolicePathMovingEnum.ReturnToTheRoad:
                            speedAdjustment = SpeedAdjustmentEnum.Stop;
                            break;
                    }
                    break;
                case PolicePathMovingEnum.ReturnToTheRoad:
                    switch (detectedCarMovement.policePathMoving)
                    {
                        case PolicePathMovingEnum.None:
                            speedAdjustment = SpeedAdjustmentEnum.Decrease;
                            break;
                        case PolicePathMovingEnum.SpeedReduction:
                            speedAdjustment = SpeedAdjustmentEnum.Decrease;
                            break;
                        case PolicePathMovingEnum.ReturnToTheRoad:
                            speedAdjustment = SpeedAdjustmentEnum.Decrease;
                            break;
                    }
                    break;
            }
            //Debug.Log(gameObject.name + "=>" + detectedCar.name + " вижу личное пространство машины спереди: " + detectedCar.name +
            //    "\nМоё состояние на полицейском пути: " + policePathMoving +
            //    "\nМоя настройка скорости после обнаружения: " + speedAdjustment);

        }
        //Debug.Log(gameObject.name + "=>" + " текущее кол-во машин в поле зрения: " + detectedCars.Count);
    }

    /// <summary>
    /// Actions while cars are in sight
    /// </summary>
    /// <param name="detectedCars"></param>
    private void DetectedCarsChecking(Dictionary<GameObject, CarMovement> detectedCars)
    {
        foreach (var car in detectedCars)
        {
            if (car.Key.Equals(null))
            {
                //Debug.Log(gameObject.name + "=> машины уже нет на карте!");
                detectedCars.Remove(car.Key);
                if (detectedCars.Count == 0)
                {
                    switch (policePathMoving)
                    {
                        case PolicePathMovingEnum.None:
                        case PolicePathMovingEnum.Starting:
                        case PolicePathMovingEnum.ReturnToTheRoad:
                            speedAdjustment = SpeedAdjustmentEnum.Increase;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    int fwdCarsCount = 0;
                    foreach (var remCar in detectedCars)
                    {
                        if (car.Key.transform.position.z > transform.position.z) fwdCarsCount++;
                    }
                    if (fwdCarsCount == 0) speedAdjustment = SpeedAdjustmentEnum.Increase;
                }
                return;
            }
            switch (policePathMoving)
            {
                case PolicePathMovingEnum.Starting:
                    //если машина, которая в поле зрения сзади
                    if (car.Key.transform.position.z < transform.position.z)
                    {
                        switch (car.Value.policePathMoving)
                        {
                            case PolicePathMovingEnum.None:
                                speedAdjustment = SpeedAdjustmentEnum.Stop;
                                break;
                            case PolicePathMovingEnum.ReturnToTheRoad:
                                speedAdjustment = SpeedAdjustmentEnum.Stop;
                                break;
                            default:
                                break;
                        }
                    }
                    else//если машина, которая осталась в поле зрения спереди
                    {
                        switch (car.Value.policePathMoving)
                        {
                            case PolicePathMovingEnum.None:
                                speedAdjustment = SpeedAdjustmentEnum.Stop;
                                break;
                            case PolicePathMovingEnum.SpeedReduction:
                                speedAdjustment = SpeedAdjustmentEnum.Decrease;
                                break;
                            case PolicePathMovingEnum.ReturnToTheRoad:
                                speedAdjustment = SpeedAdjustmentEnum.Stop;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case PolicePathMovingEnum.None:
                    {
                        if (car.Key.transform.position.z > transform.position.z)//если машина, которая осталась в поле зрения спереди
                        {
                            if (car.Value.policePathMoving == PolicePathMovingEnum.ExitFromTheRoad)
                            {
                                PolicePath detectedCarPolicePath = car.Value.CurrentPath as PolicePath;
                                if (car.Key.transform.position.z > detectedCarPolicePath.GetControlPoint(detectedCarPolicePath.ChangePointIndex + 3).z)
                                {
                                    speedAdjustment = SpeedAdjustmentEnum.Increase;
                                }
                            }
                        }
                        break;
                    }
            }
            //Debug.Log(gameObject.name + "=>" + car.Key.name + " нахожусь в личном пространстве машины: " + car.Key.name +
            //    "\nМоё состояние на полицейском пути: " + policePathMoving +
            //    "\nМоя настройка скорости: " + speedAdjustment);
        }
    }

    /// <summary>
    /// Actions after the machine disappeared from sight
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == PoliceCarTag) return;
        GameObject detectedCar = other.gameObject.transform.parent.gameObject;
        //Debug.Log(gameObject.name + "=>" + detectedCar.name + " больше не вижу личное пространство машины: " + detectedCar.name);
        detectedCars.Remove(detectedCar);
        //Debug.Log(gameObject.name + "=>" + " текущее кол-во машин в поле зрения: " + detectedCars.Count);

        switch (policePathMoving)
        {
            case PolicePathMovingEnum.Stop:
                return;
            case PolicePathMovingEnum.SpeedReduction:
                return;
            case PolicePathMovingEnum.ExitFromTheRoad:
                return;
        }
        foreach (var car in detectedCars)
        {
            //если машина, которая осталась в поле зрения сзади
            if (car.Key.transform.position.z < transform.position.z)
            {
                if (policePathMoving == PolicePathMovingEnum.Starting) return;
            }
            else//если машина, которая осталась в поле зрения спереди
            {
                switch (car.Value.policePathMoving)
                {
                    case PolicePathMovingEnum.None:
                        return;
                    case PolicePathMovingEnum.ReturnToTheRoad:
                        return;
                }
            }
        }
        speedAdjustment = SpeedAdjustmentEnum.Increase;
        //Debug.Log(gameObject.name + "=>" + detectedCar.name + " больше не вижу личное пространство машины: " + detectedCar.name +
        //        "\nМоё состояние на полицейском пути: " + policePathMoving +
        //        "\nМоя настройка скорости после ухода: " + speedAdjustment);
    }
}
